# Documentation des scripts — dedi-reaco-migrant

Index de tous les scripts C# du projet (`Assets/Scripts/`), organisés par système. Chaque script a un rôle précis ; cette doc explique à quoi il sert et comment il est câblé dans Unity, pour que n'importe qui reprenant le projet puisse s'y retrouver sans relire tout le code.

## Convention du projet

- Variables, méthodes, champs, Headers/Tooltips Inspector : **en français**.
- Noms de fichiers et classes C# : **en anglais**.
- Un seul script/GameObject plutôt que plusieurs quand c'est possible.
- Voir `CLAUDE.md` à la racine pour les conventions complètes et l'ordre des salles.

## Vue d'ensemble par système

| Doc | Système | Scripts couverts |
|---|---|---|
| [`bureau-et-salles.md`](./bureau-et-salles.md) | Gestion d'une salle (avatar, dialogue, porte, imprimante) | `BureauManager`, `PrinterAnimation`, `FeuilleRecuperable` |
| [`panique-effect.md`](./panique-effect.md) | Effet de panique progressif dans le couloir | `HallwayPanicEffect`, `WallSlide` |
| [`portes-et-sons-ambiants.md`](./portes-et-sons-ambiants.md) | Portes interactives et sons de couloir étouffés | `DoorInteractable`, `DoorMuffledVolume`, `SeatsDiscussion`, `FanRotator` |
| [`divers.md`](./divers.md) | Utilitaires (surbrillance, fondu, intro, locomotion) | `XRHoverHighlight`, `ButtonBIndicator`, `HighlightSettings`, `FadeManager`, `IntroManager`, `DevLocomotion`, `CanvasFaceCamera` |
| [`menu-principal.md`](./menu-principal.md) | Menu d'accueil (fond vidéo en boucle + bouton Jouer) | `MainMenuManager` |

## Tous les scripts en un coup d'œil

| Script | Rôle en une ligne |
|---|---|
| `BureauManager.cs` | Orchestre une salle entière : interaction avatar, dialogue, porte, imprimante, validation |
| `PrinterAnimation.cs` | Anime l'imprimante (aspiration, impression, sortie de feuille, ou bourrage papier) |
| `FeuilleRecuperable.cs` | Feuille imprimée à récupérer, clignote tant qu'elle n'est pas prise |
| `HallwayPanicEffect.cs` | Effet de panique en 3 phases (battement de cœur, respiration/vignette, intensité max) |
| `WallSlide.cs` | Déplace le couloir de sortie progressivement + déclenche la Phase 3 de la panique |
| `DoorInteractable.cs` | Porte ouvrable/fermable à la gâchette, verrouillable par script |
| `DoorMuffledVolume.cs` | Étouffe en fondu le volume de sons (discussion, ambiance, sons de détail) à chaque porte ; occlusion par mur optionnelle |
| `SeatsDiscussion.cs` | Joue une séquence de répliques de couloir, une à chaque passage |
| `FanRotator.cs` | Ventilateur qui tourne + son étouffé quand la porte de sa salle est fermée |
| `XRHoverHighlight.cs` | Surbrillance jaune au survol d'un objet interactif |
| `ButtonBIndicator.cs` | Surbrillance + vibration manette en boucle au survol |
| `HighlightSettings.cs` | ScriptableObject partagé (couleur + intensité) pour toutes les surbrillances |
| `FadeManager.cs` | Singleton : écran noir qui fond en entrée/sortie de scène |
| `IntroManager.cs` | Joue la vidéo d'intro puis charge la scène suivante |
| `DevLocomotion.cs` | Active/désactive le joystick de locomotion (développement uniquement) |
| `CanvasFaceCamera.cs` | Fait tourner un Canvas pour qu'il fasse toujours face au joueur (axe Y uniquement) |
| `MainMenuManager.cs` | Menu d'accueil : fond vidéo en boucle + bouton Jouer (Collider + `XRSimpleInteractable`) |

## Ordre des salles

Voir `CLAUDE.md` — l'ordre de jeu et les noms des salles (AO3, B02, AB42) peuvent évoluer ; cette doc reflète l'état au moment de la dernière mise à jour. En cas de doute, se référer au `CLAUDE.md` à la racine du projet, qui est la source de vérité sur ce point.
