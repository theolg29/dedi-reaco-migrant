# Système de salle — BureauManager, PrinterAnimation

## Résumé

`BureauManager` est le script central qui orchestre **tout** ce qui se passe dans une salle : interaction avec l'avatar, affichage du dialogue, fermeture/verrouillage de la porte, déclenchement de l'imprimante, et validation de la salle (déverrouillage de la porte suivante). Un `BureauManager` par salle.

---

## BureauManager.cs

### Déroulé complet

1. **Le joueur survole l'avatar** et appuie sur la **gâchette** → `DemarrerSalle()`. Deux chemins déclenchent `DemarrerSalle()` en parallèle, tous deux protégés par le même verrou `salleDemarree` (idempotent) : la lecture manuelle de la gâchette analogique (`CommonUsages.trigger`, fiable avec un contrôleur physique) **et** l'event natif `avatar.selectEntered` (fiable aussi en hand-tracking, où il n'y a pas de gâchette physique à lire).
2. `DemarrerSalle()` lance en parallèle :
   - La fermeture + verrouillage de la porte de la salle (`porte.Fermer()` / `porte.Verrouiller()`).
   - La disparition des objets du couloir si `effetCouloir` est assigné (`DemarrerDisparitionObjets()`).
   - La Timeline de l'avatar si assignée (`timeline.Play()`).
   - La coroutine `JouerDialogues()` (texte + son du dialogue).
   - La coroutine `LancerImpressionDifferee()` si une imprimante est assignée (délai puis impression — purement visuel, n'affecte pas la validation).
   - La coroutine `LancerBattementCoeurDiffere()` si `effetCouloir` est assigné (délai puis Phase 1 de la panique).
3. **`JouerDialogues()`** affiche les lignes de `dialogues[]` à l'écran (chacune pendant sa `duree`), puis attend que **le son du dialogue ait réellement fini de jouer** (et que la Timeline soit terminée si utilisée) avant de fermer le panneau. À la fin : si `changementLumiere` est assigné, `Basculer()` est appelé (ex: changement d'éclairage de la salle), puis `dialogueTermine = true` et `TenterValidation()`.
4. **`TenterValidation()`** valide la salle (`ValiderSalle()`) dès que `dialogueTermine` est vrai — la validation ne dépend que du dialogue, pas de l'imprimante.
5. **`ValiderSalle()`** déverrouille la porte de la salle (en inversant son sens d'ouverture) et la porte de la salle suivante.

> **L'imprimante est purement décorative.** Elle s'anime en parallèle du dialogue pour l'ambiance (feuille qui sort, voyant qui clignote), mais la salle se valide uniquement sur la fin du dialogue, qu'une feuille soit imprimée ou non.

### Champs Inspector

| Champ | Rôle |
|---|---|
| `avatar` | Le `XRSimpleInteractable` sur lequel appuyer pour démarrer la salle. **Doit avoir un `AudioSource` sur le même GameObject** — c'est lui qui joue le son du dialogue. |
| `timeline` | Optionnel — `PlayableDirector` à lancer en même temps (ex: animation de l'avatar). Si le son du dialogue est intégré dans la Timeline plutôt que sur un `AudioSource` séparé, le système attend tout de même la fin de la Timeline grâce à la sécurité dans `JouerDialogues()`. |
| `imprimante` | Optionnel — référence vers le `PrinterAnimation` de la salle, purement décorative. |
| `delaiAvantImpression` | Délai (secondes) entre le début de l'interaction avatar et le lancement de l'impression. **À régler à la main pour correspondre au moment voulu dans le dialogue.** |
| `bourragePapier` | Coché = simule un bourrage papier (`ImprimerAvecBourrage()`) au lieu d'imprimer normalement. Aucune feuille ne sort, le voyant clignote en orange indéfiniment. |
| `changementLumiere` | Optionnel — référence vers un `GameObjectSwapper` ; `Basculer()` est appelé automatiquement à la fin du dialogue (ex: désactiver un setup d'éclairage et activer un autre, pour la salle B02). |
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
- **`Awake()` appelle `porteSuivante.Verrouiller()`** — voir le piège correspondant dans [`portes-et-sons-ambiants.md`](./portes-et-sons-ambiants.md#doorinteractablecs) sur l'ordre d'exécution des `Awake()` entre scripts. Si une salle entière ne réagit plus à aucune interaction (alors que le highlight fonctionne toujours), suspecter en premier une exception silencieuse à cet endroit plutôt que le câblage de l'avatar.

---

## PrinterAnimation.cs

Anime une imprimante physique en plusieurs phases calées sur un fichier son (`sonImpression`) : clignotement du voyant, aspiration de la feuille vierge, impression à coups réguliers, sortie de la feuille imprimée. **Purement décoratif** — la feuille imprimée n'est plus ramassable, elle reste simplement visible une fois sortie.

### Deux modes

| Méthode | Comportement |
|---|---|
| `Imprimer()` | Séquence complète (~12.3s, calée sur les constantes `T_DEBUT_ASPIRATION` etc.). |
| `ImprimerAvecBourrage()` | Aspire la feuille vierge puis, après `delaiAvantBourrage` secondes, le voyant passe en `couleurBourrage` (orange par défaut) et **clignote indéfiniment**. Aucune feuille ne sort. |

### Champs clés

| Champ | Rôle |
|---|---|
| `feuilleVierge` / `feuilleImprimee` | Les `Transform` des deux feuilles à déplacer. |
| `cibleVierge` / `ciblePrint` | GameObjects vides marquant les positions cibles. |
| `voyantLED` | `Renderer` du voyant — clignote en activant/désactivant le composant (mode normal) ou via `_EmissionColor` (mode bourrage). |
| `sonImpression` | Clip joué dans les deux modes (pas de clip séparé pour le bourrage). |
| `delaiAvantBourrage` | Temps après le début de l'animation avant que le voyant passe en orange. |
| `couleurBourrage` | Couleur d'émission du voyant en cas de bourrage. |
