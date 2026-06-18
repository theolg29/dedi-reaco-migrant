# Menu principal (00_MainMenu)

## MainMenuManager.cs

Gère le menu d'accueil : charge la scène suivante dès que le joueur appuie sur la gâchette droite — peu importe où il regarde, pas besoin de viser un bouton.

| Champ | Rôle |
|---|---|
| `sceneSuivante` | Nom **exact** de la scène à charger à la pression de la gâchette (`01_Intro`). |

### Câblage dans la scène

1. **Visuel "Jouer"** : un Canvas World Space (Image + Texte) positionné devant le joueur — par convention enfant de `Main Camera` pour toujours rester dans le champ de vision, comme l'écran vidéo de `IntroManager`. Purement décoratif, aucun `Collider`/`XRSimpleInteractable` requis puisque l'action ne dépend pas du regard.
2. **`MainMenuManager`** : posé sur n'importe quel GameObject de la scène (lit directement la gâchette manette à chaque frame, comme `DoorInteractable`).
3. **`FadeManager`** : si présent dans la scène, le passage vers `sceneSuivante` se fait avec un fondu (`FadeOutThenIn`) ; sinon le chargement est immédiat.
