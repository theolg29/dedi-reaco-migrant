# Utilitaires divers

## XRHoverHighlight.cs

Surbrillance jaune (ou autre couleur) au survol d'un `XRSimpleInteractable`. S'applique automatiquement à **tous les renderers** trouvés sur le GameObject et ses enfants (utile pour les objets composés de plusieurs matériaux). Utilise l'émission (`_EmissionColor`), couleur/intensité définies dans un `HighlightSettings` partagé.

À poser sur n'importe quel objet interactif simple (avatar, porte verrouillée visuellement, etc.) qui doit juste s'illuminer au survol sans logique supplémentaire.

## ButtonBIndicator.cs

Différent de `XRHoverHighlight` : ici, la surbrillance **clignote** et s'accompagne d'une **vibration manette en boucle** tant que le joueur survole **n'importe quel** `XRSimpleInteractable` de la scène (détection via `FindObjectsByType` au démarrage, pas un objet précis). Gère un compteur (`survolsActifs`) pour rester actif si plusieurs interactables se survolent en même temste.

> Si une nouvelle scène ajoute des interactables après le `Start()` de ce script, ils ne seront pas détectés automatiquement — ce script suppose que tous les `XRSimpleInteractable` existent déjà au chargement de la scène.

## HighlightSettings.cs

`ScriptableObject` (menu `Reaco/Highlight Settings`) partagé entre `XRHoverHighlight` et `ButtonBIndicator` : couleur + intensité d'émission. Modifier cet asset change la surbrillance **partout où il est assigné** — pratique pour garder une cohérence visuelle, mais attention à ne pas le modifier en pensant n'affecter qu'un seul objet.

## FadeManager.cs

Singleton (`FadeManager.Instance`, `DontDestroyOnLoad`) qui gère un écran noir plein écran en World Space, toujours repositionné devant la caméra active. Survit aux changements de scène.

| Méthode | Rôle |
|---|---|
| `FadeIn(duree, onTermine)` | Noir → transparent. |
| `FadeOut(duree, onTermine)` | Transparent → noir. |
| `FadeOutThenIn(action, ...)` | Fondu au noir, exécute `action` (ex: changer de scène), puis fondu en transparent. |

Au chargement d'une scène (`OnSceneChargee`), si le mode inclut `FonduDebut`, un fade-in automatique se lance dès que `Camera.main` est disponible.

Utilisé par `IntroManager` pour le fondu de transition vers la scène de jeu.

## IntroManager.cs

Joue une vidéo d'intro (avec son intégré) sur un écran World Space créé dynamiquement devant la caméra, puis charge la scène suivante (`sceneSuivante`) — avec un fondu via `FadeManager` si disponible.

⚠️ **Confort VR** : l'écran est positionné **une seule fois** (au moment de sa création), dans l'espace du monde, à partir de la position/direction de la caméra à cet instant — il n'est **pas** parenté à la caméra. Un écran qui suit la tête de façon rigide à chaque frame donne le mal des transports ; un écran fixe dans l'espace (comme un vrai écran de cinéma) ne pose pas ce problème.

| Champ | Rôle |
|---|---|
| `video` | Le `VideoClip` à jouer. |
| `sceneSuivante` | Nom **exact** de la scène à charger après l'intro. |
| `distanceEcran` / `hauteurEcran` | Taille/position de l'écran virtuel devant le joueur. |

## DevLocomotion.cs

Utilitaire de **développement uniquement** : active/désactive le GameObject du joystick de locomotion de l'XR Origin (`DynamicMoveProvider`). Pratique pour tester rapidement avec ou sans déplacement au joystick, en plus du déplacement physique.

⚠️ Ne pas laisser ce script désactiver la locomotion par erreur dans une build de test — vérifier `joystickActif` avant de partager une build.

## GameObjectSwapper.cs

Désactive un GameObject et active un autre, via la méthode publique `Basculer()`. Générique — utilisé par exemple par `BureauManager` (champ `changementLumiere`) pour basculer entre deux setups d'éclairage à la fin d'un dialogue, mais peut servir pour n'importe quel échange simple de deux objets.

## CanvasFaceCamera.cs

Fait pivoter un Canvas (ou tout `Transform`) pour qu'il fasse **toujours face au joueur**, sur l'axe Y uniquement (jamais de pitch/roll, donc le panneau ne penche jamais même si le joueur regarde en haut/bas). Utilisé sur le panneau de dialogue de `BureauManager`.

⚠️ **Sens de rotation important** : le code fait pointer l'avant du Canvas (+Z) **à l'opposé** de la caméra (`Quaternion.LookRotation(-direction)`), pas vers elle — c'est l'inverse du "look at" classique. Si le texte apparaît inversé/mirroré après une modif, vérifier ce signe en premier.
