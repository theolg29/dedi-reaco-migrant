# Système de salle — BureauManager, PrinterAnimation, FeuilleRecuperable

## Résumé

`BureauManager` est le script central qui orchestre **tout** ce qui se passe dans une salle : interaction avec l'avatar, affichage du dialogue, fermeture/verrouillage de la porte, déclenchement de l'imprimante, et validation de la salle (déverrouillage de la porte suivante). Un `BureauManager` par salle.

---

## BureauManager.cs

### Déroulé complet

1. **Le joueur survole l'avatar** et appuie sur la **gâchette** → `DemarrerSalle()`.
2. `DemarrerSalle()` lance en parallèle :
   - La fermeture + verrouillage de la porte de la salle (`porte.Fermer()` / `porte.Verrouiller()`).
   - La disparition des objets du couloir si `effetCouloir` est assigné (`DemarrerDisparitionObjets()`).
   - La Timeline de l'avatar si assignée (`timeline.Play()`).
   - La coroutine `JouerDialogues()` (texte + son du dialogue).
   - La coroutine `LancerImpressionDifferee()` si une imprimante est assignée (délai puis impression).
   - La coroutine `LancerBattementCoeurDiffere()` si `effetCouloir` est assigné (délai puis Phase 1 de la panique).
3. **`JouerDialogues()`** affiche les lignes de `dialogues[]` à l'écran (chacune pendant sa `duree`), puis attend que **le son du dialogue ait réellement fini de jouer** (et que la Timeline soit terminée si utilisée) avant de fermer le panneau. À la fin : `dialogueTermine = true` puis `TenterValidation()`.
4. **`SignalerMissionTerminee()`** est appelée par `FeuilleRecuperable` quand le joueur récupère la feuille imprimée → `feuilleRecuperee = true` puis `TenterValidation()`.
5. **`TenterValidation()`** valide la salle (`ValiderSalle()`) seulement si **les deux conditions sont remplies** : dialogue terminé ET (feuille récupérée OU pas d'imprimante OU bourrage papier — dans ces deux derniers cas, il n'y a jamais de feuille à récupérer, donc la condition est automatiquement satisfaite).
6. **`ValiderSalle()`** déverrouille la porte de la salle (en inversant son sens d'ouverture) et la porte de la salle suivante.

> **Pourquoi deux conditions découplées ?** L'impression et le dialogue tournent en parallèle (pas l'un après l'autre) — le délai d'impression est réglé à la main pour correspondre au bon moment du dialogue. Comme ils ne se terminent pas forcément au même instant, la validation attend les deux indépendamment plutôt que de supposer un ordre fixe.

### Champs Inspector

| Champ | Rôle |
|---|---|
| `avatar` | Le `XRSimpleInteractable` sur lequel appuyer pour démarrer la salle. **Doit avoir un `AudioSource` sur le même GameObject** — c'est lui qui joue le son du dialogue. |
| `timeline` | Optionnel — `PlayableDirector` à lancer en même temps (ex: animation de l'avatar). Si le son du dialogue est intégré dans la Timeline plutôt que sur un `AudioSource` séparé, le système attend tout de même la fin de la Timeline grâce à la sécurité dans `JouerDialogues()`. |
| `imprimante` | Optionnel — référence vers le `PrinterAnimation` de la salle. |
| `delaiAvantImpression` | Délai (secondes) entre le début de l'interaction avatar et le lancement de l'impression. **À régler à la main pour correspondre au moment voulu dans le dialogue.** |
| `bourragePapier` | Coché = simule un bourrage papier (`ImprimerAvecBourrage()`) au lieu d'imprimer normalement. Aucune feuille ne sortira jamais — la condition de validation "feuille" est donc automatiquement remplie. |
| `porte` | La porte de **cette** salle. |
| `porteSuivante` | La porte de la salle suivante — verrouillée au démarrage (`Awake`), déverrouillée à la validation. |
| `effetCouloir` | Optionnel, **normalement seulement sur la salle qui précède la séquence de panique** (B02 au moment de cette doc) — voir [`panique-effect.md`](./panique-effect.md). |
| `delaiAvantBattementCoeur` | Délai (secondes) avant de déclencher la Phase 1 du `HallwayPanicEffect`. **À régler pour tomber vers la fin du dialogue**, indépendamment du nombre de lignes. |
| `panneauDialogue` | Le `GameObject` du Canvas WorldSpace affichant le dialogue (un `CanvasGroup` y est ajouté automatiquement si absent). |
| `dureeTransition` | Durée du fondu d'apparition/disparition du panneau de dialogue. |
| `texteDialogue` | Le `TextMeshProUGUI` qui affiche le texte. |
| `dialogues[]` | Tableau de lignes `{ texte, duree }` affichées dans l'ordre pendant que le son du dialogue joue. Si vide, le système attend simplement la durée du clip audio à la place. |

### Pièges connus (déjà corrigés, à ne pas réintroduire)

- **Ne jamais effacer le texte après les sécurités d'attente du son/Timeline** — sinon la dernière ligne reste figée à l'écran bien après sa `duree` prévue. Le texte doit être vidé **juste après la boucle de dialogue**, avant les `while` de sécurité.
- **Ne jamais utiliser le bouton B (`CommonUsages.secondaryButton`)** pour la gâchette — le projet utilise `CommonUsages.trigger` (valeur analogique, seuil à 0.5) car le flag digital `triggerButton` n'est pas fiable sur tous les profils de contrôleurs OpenXR.

---

## PrinterAnimation.cs

Anime une imprimante physique en plusieurs phases calées sur un fichier son (`sonImpression`) : clignotement du voyant, aspiration de la feuille vierge, impression à coups réguliers, sortie de la feuille imprimée.

### Deux modes

| Méthode | Comportement |
|---|---|
| `Imprimer()` | Séquence complète (~12.3s, calée sur les constantes `T_DEBUT_ASPIRATION` etc.) → à la fin, `feuilleRecuperable.ActiverRecuperation()` est appelée. |
| `ImprimerAvecBourrage()` | Aspire la feuille vierge puis, après `delaiAvantBourrage` secondes, le voyant passe en `couleurBourrage` (orange par défaut) et **clignote indéfiniment**. Aucune feuille ne sort, `ActiverRecuperation()` n'est jamais appelée. |

### Champs clés

| Champ | Rôle |
|---|---|
| `feuilleVierge` / `feuilleImprimee` | Les `Transform` des deux feuilles à déplacer. |
| `cibleVierge` / `ciblePrint` | GameObjects vides marquant les positions cibles. |
| `voyantLED` | `Renderer` du voyant — clignote en activant/désactivant le composant (mode normal) ou via `_EmissionColor` (mode bourrage). |
| `sonImpression` | Clip joué dans les deux modes (pas de clip séparé pour le bourrage). |
| `delaiAvantBourrage` | Temps après le début de l'animation avant que le voyant passe en orange. |
| `couleurBourrage` | Couleur d'émission du voyant en cas de bourrage. |

---

## FeuilleRecuperable.cs

La feuille imprimée que le joueur doit attraper (gâchette + survol) pour terminer la mission de la salle.

- Désactivée par défaut (`interactable.enabled = false` dans `Awake`) — activée uniquement par `ActiverRecuperation()` (appelée par `PrinterAnimation` en fin d'impression normale).
- Une fois activée, **clignote en continu** (émission de couleur via `HighlightSettings`) pour attirer l'attention, indépendamment du survol — contrairement aux autres surbrillances du projet qui ne réagissent qu'au hover.
- À la récupération (`Recuperer()`) : désactive l'interactable, arrête le clignotement, cache la feuille, et notifie `BureauManager.SignalerMissionTerminee()`.

| Champ | Rôle |
|---|---|
| `bureauManager` | Référence vers le `BureauManager` de la salle à notifier. |
| `parametres` | `HighlightSettings` partagé — **doit être assigné** sinon le clignotement passe de noir à noir (invisible). |
| `intervalleClignotement` | Vitesse du clignotement. |
