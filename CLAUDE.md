# DEDI ReaCo — Projet VR Préfecture

## Contexte du projet

Projet de Master DEDI. Objectif : créer une expérience immersive à destination des fonctionnaires en préfecture travaillant avec des personnes étrangères dans le cadre de l'obtention de papiers.

Le concept global est une expérience **AR** (documentée dans le dossier de production). La partie développée en Unity est un **segment VR** concentré sur une portion spécifique de la courbe émotionnelle.

## Courbe émotionnelle (gauche → droite)

```
ESPOIR → PRÉOCCUPÉ → AGACEMENT → CONTRÔLE → INCROMPRHÉNSION → FRUSTRÉ/SURCHARGÉ/STRESS → DÉSESPOIR → SOULAGEMENT/APAISÉ
```

**Le segment VR couvre : CONTRÔLE → DÉSESPOIR** (avant la chute de la courbe).

## Scénario VR

1. **Bureau général** : un avatar accueille le joueur et l'oriente vers un bureau situé dans un couloir
2. **Bureau 1** : la fonctionnaire a tous les papiers requis → redirige vers le Bureau 2
3. **Bureau 2** : problèmes, documents manquants ou refus → redirige vers un autre bureau
4. **Couloir de l'espoir brisé** : le couloir s'allonge visuellement à mesure que le joueur avance (effet de désespoir)
5. **Sortie** : le couloir finit par s'arrêter, le joueur peut prendre la sortie → fin de la démo VR

La suite de l'expérience (soulagement, apaisement) est documentée dans les livrables de rendu, pas développée en Unity.

## Stack technique

- **Moteur** : Unity (URP) — template VR Unity de base
- **Casque cible** : Meta Quest 2
- **SDK** : OpenXR + XR Interaction Toolkit (template XR téléchargé pour les mécaniques prêtes à l'emploi : portes, interactables, etc.)
- **Locomotion VR** : déplacement au joystick de la manette (continuous move)
- **Locomotion AR** (concept, non développé) : déplacement physique dans une grande pièce (roomscale)

## Structure du projet

### Scènes Unity
| Scène | Usage |
|---|---|
| `LaSceneDeGame.unity` | **Scène principale partagée** — contient le jeu final, à merger avec précaution |
| `Hassan.unity` | Scène de dev perso — Hassan |
| `Mathilde.unity` | Scène de dev perso — Mathilde |
| `Ronan.unity` | Scène de dev perso — Ronan |
| `Theo.unity` | Scène de dev perso — Theo |

Chaque développeur travaille dans **sa propre scène** pour éviter les conflits de merge.

## Workflow Git

### Branches
| Branche | Propriétaire |
|---|---|
| `main` | Branche d'intégration (merge ici quand stable) |
| `hassan` | Hassan |
| `mathilde` | Mathilde |
| `ronan` | Ronan |
| `theo` | Theo |

### Règles
- Chaque dev travaille sur **sa branche personnelle**, merge sur `main` quand la feature est stable
- **À chaque nouvelle instance Claude** : demander sur quelle branche on travaille, attendre la réponse, puis faire `git checkout <branche>` avant tout développement
- **Après chaque feature développée avec succès** : proposer dans le chat de faire un commit avec un message clair

### Commit workflow
Après une feature réussie, proposer :
```
git add <fichiers concernés>
git commit -m "feat: <description courte>"
```
Ne jamais faire `git add -A` ou `git add .` sans vérifier les fichiers concernés.
