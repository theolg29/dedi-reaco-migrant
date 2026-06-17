# Menu principal (00_MainMenu)

## MainMenuManager.cs

Gère le menu d'accueil : fond vidéo en boucle + bouton "Jouer" interactif à la manette. Un seul script, posé directement sur le bouton.

| Champ | Rôle |
|---|---|
| `videoFond` | `VideoClip` joué en boucle derrière le menu (son intégré dans le fichier). |
| `distanceEcran` / `hauteurEcran` | Taille/position de l'écran de fond devant la caméra. |
| `sceneSuivante` | Nom **exact** de la scène à charger en appuyant sur Jouer (`01_Intro`). |

### Câblage dans la scène

1. **Bouton Jouer** : un GameObject avec une icône (ex: Quad + texture triangle ▶), un `Collider` (ex: `BoxCollider`) ajusté à l'icône, un `XRSimpleInteractable`, et ce script `MainMenuManager`.
   - Comme pour les portes/imprimantes, ajouter aussi `XRHoverHighlight` (avec un `HighlightSettings` assigné) sur le bouton pour la surbrillance au survol manette.
2. **Fond vidéo** : créé automatiquement au lancement par `MainMenuManager` (même technique que `IntroManager`, mais avec `isLooping = true`) — pas besoin de le construire à la main dans l'éditeur.
3. **FadeManager** : si présent dans la scène, le passage vers `sceneSuivante` se fait avec un fondu (`FadeOutThenIn`) ; sinon le chargement est immédiat.

Suit le même modèle d'interaction que `DoorInteractable`/`FeuilleRecuperable` (Collider + `XRSimpleInteractable`) plutôt qu'un `Button` Unity UI classique, pour rester cohérent avec le reste du projet (pas de raycaster XR-UI utilisé ailleurs).
