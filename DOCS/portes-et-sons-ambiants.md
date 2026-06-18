# Portes et sons ambiants — DoorInteractable, DoorMuffledVolume, SeatsDiscussion, FanRotator

## Résumé

Toutes les portes du jeu partagent le même script `DoorInteractable`. Chaque fois qu'une porte s'ouvre ou se ferme — n'importe laquelle — elle prévient automatiquement les systèmes de son ambiant pour qu'ils s'étouffent ou se rétablissent, sans avoir à câbler quoi que ce soit manuellement entre les deux.

---

## DoorInteractable.cs

Porte interactive : survol + gâchette pour ouvrir/fermer, verrouillable par script (`BureauManager` l'utilise pour bloquer une porte pendant un dialogue).

### API publique

| Méthode/Propriété | Rôle |
|---|---|
| `Verrouiller()` | Désactive l'interaction (`interactable.enabled = false`) — la porte ne répond plus au survol/gâchette. |
| `Deverrouiller()` | Réactive l'interaction. |
| `Fermer()` | Lance l'animation de fermeture si la porte est actuellement ouverte. |
| `InverserSens()` | Inverse le sens d'ouverture pour la prochaine animation (utilisé quand une salle est validée, pour que la porte s'ouvre "vers la sortie"). |
| `EstOuverte` | `true`/`false` selon l'état actuel. |
| `event Animee` | Invoqué à chaque animation `(bool vaOuvrir, float dureeAnimation)` — pour qu'un script tiers (ex: `FanRotator`) réagisse à **cette porte précise**. |

### Ce qui se passe à chaque animation (`AnimerPorte`)

1. Joue `sonOuverture` ou `sonFermeture` selon le sens.
2. Appelle `DoorMuffledVolume.RetablirToutes()` (ouverture) ou `EtoufferToutes()` (fermeture) — **toutes les portes du jeu déclenchent ce même appel global**, peu importe laquelle.
3. Déclenche l'event `Animee`.
4. Anime la rotation sur la durée `dureeAnimation`.

### Sécurité

`Update()` vérifie `interactable.enabled` en plus de `survolActif` avant de traiter un appui gâchette — une protection supplémentaire contre un hover qui resterait "collé" malgré une désactivation en cours de survol.

⚠️ **Piège évité** : ne pas utiliser `CommonUsages.triggerButton` (flag digital) pour détecter l'appui gâchette — utiliser `CommonUsages.trigger` (valeur analogique) avec un seuil (`> 0.5f`), car le flag digital n'est pas remonté de façon fiable par tous les profils de contrôleurs OpenXR.

---

## DoorMuffledVolume.cs

Un seul script générique pour **tous** les sons de couloir qui doivent réagir aux portes : fondu de volume + occlusion par mur optionnelle.

### Pourquoi un seul script et pas un par son ?

Avant, chaque son ambiant (`AmbianceCouloir`, `SeatsDiscussion`...) avait sa propre logique de fondu dupliquée. Maintenant, `DoorMuffledVolume` centralise tout : on lui donne une liste explicite d'`AudioSource` à gérer (`sources[]`), et il s'occupe du reste. On peut en poser plusieurs instances dans la scène avec des réglages différents, ou tout regrouper sur un seul GameObject.

### Fonctionnement

- **Fondu sur porte** (toujours actif) : `EtoufferToutes(duree)` / `RetablirToutes(duree)` sont des méthodes **statiques**, appelées par `DoorInteractable` à chaque porte. Toutes les instances de `DoorMuffledVolume` présentes dans la scène réagissent ensemble (registre statique via `OnEnable`/`OnDisable`).
- **Occlusion par mur** (optionnelle) : si `calqueMurs` est configuré (différent de "Nothing"), un raycast caméra → chaque source détecte un mur sur le chemin et applique un filtre passe-bas (`AudioLowPassFilter`, ajouté automatiquement si absent). Si `calqueMurs` reste vide, cette partie est entièrement ignorée (zéro coût).

### Champs Inspector

| Champ | Rôle |
|---|---|
| `sources[]` | Liste explicite des `AudioSource` à gérer — **glisser-déposer manuel**, pas de détection automatique de hiérarchie (pour éviter de capturer des sons de salle par accident). |
| `volumeNormal` / `volumeEtouffe` | Volume porte ouverte / porte fermée. |
| `calqueMurs` | Layer des murs pour l'occlusion (laisser sur "Nothing" pour désactiver). |
| `frequenceEtouffeeParMur` / `frequenceNormale` | Fréquences de coupure du filtre passe-bas. |

### Setup typique

Un GameObject "GestionSonCouloir" avec un `DoorMuffledVolume`, et dans `sources[]` : l'`AudioSource` de l'ambiance du couloir, celui des discussions (`SeatsDiscussion`), et ceux des sons de détail (téléphone, horloge...). Pour ces derniers, configurer aussi `calqueMurs` si on veut l'effet d'occlusion.

---

## SeatsDiscussion.cs

Joue une séquence de répliques de couloir (figurants assis, etc.) : à chaque passage du joueur dans son `BoxCollider`, la `Replique` suivante du tableau `repliques[]` est jouée — son `clip` (optionnel) et **toutes** les `timelines[]` associées, lancées ensemble (ex: une discussion entre deux personnages : un seul son de dialogue, mais les deux avatars animent en même temps). Si le son est déjà intégré dans une Timeline plutôt que sur un `AudioClip` séparé, laisser `clip` vide. Une fois toutes les répliques jouées, ne fait plus rien.

⚠️ **Piège évité** : un verrou booléen (`discussionEnCours`) est posé **immédiatement et de façon synchrone** dès `OnTriggerEnter`, avant même de jouer le son/les timelines — ne jamais se fier uniquement à `AudioSource.isPlaying` ou `PlayableDirector.state` pour bloquer un double déclenchement, ces états peuvent ne pas être encore à jour si le trigger est touché deux fois dans la même frame (fréquent avec un `CharacterController` VR qui traverse la zone), ce qui laisserait les deux discussions se lancer en même temps. Le verrou n'est relâché qu'une fois le son **et** toutes les timelines de la réplique en cours effectivement terminés (coroutine `JouerReplique`).

N'a **pas** sa propre logique de fondu — pour que ses sons s'étouffent aux portes, ajouter en plus un `DoorMuffledVolume` sur le même GameObject (avec son `AudioSource` dans la liste `sources[]`).

---

## FanRotator.cs

Ventilateur qui tourne en continu (`vitesseRotation`) et joue un son en boucle, **étouffé spécifiquement quand la porte de sa propre salle est fermée** — contrairement à `DoorMuffledVolume` qui réagit à *n'importe quelle* porte, celui-ci s'abonne à l'event `Animee` d'**une porte précise** (champ `porte`).

| Champ | Rôle |
|---|---|
| `porte` | La porte de la salle où se trouve le ventilateur — son fermeture étouffe le son. |
| `volume` | Volume quand la porte est ouverte. |
| `volumeEtouffe` | Volume quand la porte est fermée. |

> À ne pas confondre avec `DoorMuffledVolume` : celui-ci est pour des sons **spécifiques à une salle donnée** (le son ne doit pas s'entendre depuis le couloir), alors que `DoorMuffledVolume` est pour des sons de **couloir** qui doivent baisser quand on entre *dans n'importe quelle* salle.
