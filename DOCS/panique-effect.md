# Effet de panique — HallwayPanicEffect

## Résumé

L'effet de panique simule une crise d'angoisse progressive quand le joueur traverse le couloir après la salle B02. Il se décompose en **3 phases** qui s'enchaînent automatiquement au fil de la progression du joueur, du plus subtil au plus intense.

---

## Vue d'ensemble des 3 phases

| Phase | Déclencheur | Ce qui se passe |
|-------|-------------|-----------------|
| **1 — Battement de cœur** | `BureauManager` (salle B02), après `delaiAvantBattementCoeur` secondes depuis le début de l'interaction avatar | Un son de battement de cœur (boucle naturelle de ~15s) commence à jouer à son rythme normal, le volume monte progressivement ; une très légère vibration manette accompagne chaque battement |
| **2 — Respiration + vignette** | Le joueur entre dans le couloir (BoxCollider trigger) | Un son de respiration apparaît, un voile noir commence à assombrir la vision ; le cœur **bat plus vite** (pitch augmenté) et la vibration s'intensifie un peu |
| **3 — Intensité max** | Le couloir commence à s'allonger (WallSlide) | Le cœur bat **très rapidement**, shake caméra, vibrations manettes au maximum, tout monte au maximum |

> Le déclenchement de la Phase 1 par **délai** (et non par "dernière ligne de dialogue") est volontaire : avec un dialogue à une seule ligne, "la dernière ligne" est aussi la première, ce qui ferait partir le battement dès le début. Le délai, réglé à la main sur `BureauManager`, permet de viser précisément "vers la fin" peu importe le nombre de lignes.

> Chaque phase **s'ajoute** à la précédente — rien ne s'arrête, tout s'intensifie.

---

## Comment c'est câblé dans Unity

### GameObjects et scripts impliqués

```
Scène
├── BureauManager (sur le GO de gestion de la salle B02)
│   └── Champ "Effet Couloir" → pointe vers le HallwayPanicEffect
│
├── HallwayPanicEffect (sur un GO dans le couloir avec un BoxCollider trigger)
│   └── Le BoxCollider doit couvrir l'entrée du couloir (là où le joueur sort de B02)
│
└── WallSlide (sur un GO plus loin dans le couloir avec un BoxCollider trigger)
    └── Champ "Effet Panique" → pointe vers le même HallwayPanicEffect
```

### Références à relier dans l'Inspector

#### Sur le BureauManager (salle B02)

| Champ | Quoi assigner | Rôle |
|-------|---------------|------|
| `Effet Couloir` | Le GameObject qui porte le script `HallwayPanicEffect` | Permet au BureauManager de démarrer la disparition des objets et le battement de cœur |
| `Delai Avant Battement Coeur` | Une durée en secondes | Délai depuis le début de l'interaction avatar avant de déclencher la Phase 1 — à régler pour tomber vers la fin du dialogue |

#### Sur le HallwayPanicEffect

| Champ | Quoi assigner | Rôle |
|-------|---------------|------|
| `Objets A Disparaitre` | Liste de GameObjects (meubles, PNJ, décorations) | Objets qui disparaissent un par un pendant le dialogue |
| `Son Battement Coeur` | AudioClip de battement de cœur (ambiance continue, ex: ~15s) | Joué **en boucle naturelle** dès la Phase 1 — le clip contient déjà son propre rythme, il n'est pas redémarré périodiquement |
| `Son Respiration` | AudioClip de respiration en boucle | Joué en continu à partir de la phase 2 |
| `Cible Shake` | Le **Camera Offset** du XR Origin | Objet secoué pour simuler le tremblement (⚠️ jamais la caméra directement) |
| `Intervalle Battement` | Une durée en secondes | Rythme de base des impulsions de vibration manette (+ shake en Phase 3), dès la Phase 1 — se resserre automatiquement à mesure que le cœur accélère (pitch) |
| `Pitch Battement Depart` / `Pitch Battement Phase2` / `Pitch Battement Max` | Des multiplicateurs de vitesse (1 = normal) | Vitesse de lecture du son du cœur par phase — fait littéralement battre le cœur plus vite (et plus aigu) à mesure que la panique monte |
| `Intensite Vibration Phase1` / `Intensite Vibration Phase2` / `Intensite Vibration` | Des intensités (0-1) | Intensité de la vibration manette par phase — légère en Phase 1, modérée en Phase 2, maximale en Phase 3 |

⚠️ **Piège évité** : ne jamais redémarrer (`Stop()` + `PlayOneShot()`) un clip long à intervalle court — ça ne joue que l'attaque du clip en boucle serrée, on n'entend jamais le reste du son ("tick" répétitif au lieu d'une vraie ambiance). Pour un son qui doit jouer dans son intégralité, toujours utiliser `loop = true` + `Play()` une seule fois.

#### Sur le WallSlide

| Champ | Quoi assigner | Rôle |
|-------|---------------|------|
| `Effet Panique` | Le même GameObject `HallwayPanicEffect` | Permet au WallSlide de déclencher la phase 3 (intensité max) |

---

## Déroulé complet de l'expérience

### Avant les phases — Disparition des objets

Dès que le joueur interagit avec l'avatar dans B02 :
- La porte se ferme et se verrouille
- `DemarrerDisparitionObjets()` est appelé
- Les objets de la liste disparaissent **un par un dans un ordre aléatoire** sur la durée configurée
- Le joueur ne s'en rend pas compte immédiatement (il écoute le dialogue)

### Phase 1 — Battement de cœur

**Quand :** `delaiAvantBattementCoeur` secondes après le début de l'interaction avec l'avatar (réglé indépendamment du dialogue lui-même).

**Ce qui se passe :**
- Le son du battement de cœur démarre **une seule fois**, en boucle naturelle (`loop = true`) — il joue son intégralité (ex: 15s) puis recommence depuis le début, sans redémarrage prématuré
- Le volume part de `Volume Battement Depart` (très bas, ex: 0.1)
- Il monte progressivement vers `Volume Battement Milieu` (ex: 0.35)
- Le cœur bat à son rythme normal (`Pitch Battement Depart` = 1)
- Une vibration manette très légère (`Intensite Vibration Phase1`, ex: 0.05) accompagne chaque battement
- La vitesse de montée est contrôlée par `Vitesse Montee Battement`

**Ressenti joueur :** Un léger malaise, quelque chose ne va pas.

### Phase 2 — Respiration + vignette

**Quand :** Le joueur sort de la salle B02 et entre dans le BoxCollider du couloir.

**Ce qui se passe :**
- Le son de respiration démarre (volume à 0, monte vers `Volume Respiration Milieu`)
- Un voile noir apparaît progressivement sur les bords de la vision (vignette)
- L'opacité monte vers `Opacite Vignette Milieu`
- Le battement de cœur continue sa progression et **accélère** (pitch monte vers `Pitch Battement Phase2`, ex: 1.25) — le cœur bat plus vite
- La vibration manette s'intensifie un peu (`Intensite Vibration Phase2`, ex: 0.15) et ses impulsions se rapprochent (suivent l'accélération du cœur)
- Le joueur voit que les meubles et PNJ ont disparu

**Ressenti joueur :** L'angoisse monte, la vision se réduit, on entend sa propre respiration.

### Phase 3 — Intensité max

**Quand :** Le joueur atteint le BoxCollider du WallSlide (le couloir commence à s'allonger).

**Ce qui se passe :**
- **Battement de cœur** → volume monte vers 1.0 (maximum), pitch monte vers `Pitch Battement Max` (ex: 1.6) — le cœur bat **très rapidement**
- **Respiration** → volume monte vers 1.0
- **Vignette** → opacité monte vers `Opacite Vignette Max` (ex: 0.85)
- **Shake caméra** → la caméra tremble à chaque battement (intensité monte vers `Intensite Shake`)
- **Vibrations manettes** → intensité au maximum (`Intensite Vibration`) et impulsions au rythme le plus rapide (suivent le pitch du cœur)
- Le couloir s'allonge devant le joueur (géré par WallSlide)

**Ressenti joueur :** Panique totale, le couloir n'en finit pas, la vision se trouble, tout vibre.

---

## Réglage des vitesses de montée

Les vitesses utilisent un **fondu exponentiel** (`Lerp` par frame). Plus la valeur est haute, plus l'effet monte vite.

| Vitesse | Temps pour ~63% | Temps pour ~95% | Usage typique |
|---------|-----------------|-----------------|---------------|
| 0.3 | ~3.3s | ~10s | Montée lente (phase 3 : le max arrive sur toute la durée du couloir) |
| 0.4 | ~2.5s | ~7.5s | Montée moyenne (phase 2 : respiration) |
| 0.5 | ~2s | ~6s | Montée rapide (phase 1 : le cœur se fait sentir vite) |

**Valeurs par défaut :**
- `Vitesse Montee Battement` = 0.5 (le cœur se fait sentir rapidement)
- `Vitesse Montee Respiration` = 0.4 (la respiration arrive naturellement)
- `Vitesse Montee Intense` = 0.3 (le shake/vibrations montent sur ~10s dans le couloir)

---

## Sécurités intégrées

- **Phase 1 obligatoire** : Les phases 2 et 3 ne peuvent se déclencher que si la phase 1 (battement de cœur) a été démarrée par le BureauManager. Cela permet de réactiver sans risque le BoxCollider de `HallwayPanicEffect` même sur le trajet **avant** B02 (ex: en allant de AO3 vers B02) — sans Phase 1 déjà déclenchée, traverser ce collider à ce moment-là ne fait rien.
- **Cascade Phase 3 → 2** : Si la phase 3 est déclenchée (WallSlide) et que la phase 2 n'a pas encore démarré, la phase 2 se lance automatiquement (idéalement, ça ne devrait jamais arriver si le BoxCollider de la Phase 2 est bien positionné et activé — voir section setup ci-dessus).
- **Attente de la Timeline** : Le BureauManager attend la fin de la Timeline en plus de l'AudioSource avant de considérer le dialogue comme terminé (l'audio de l'avatar peut être intégré dans la Timeline).
- **Idempotence** : Appeler deux fois la même phase n'a aucun effet — la garde `if (phase >= X) return` empêche les doublons.
- **Scintillement de la vignette** : La vignette noire n'est pas figée, elle utilise du bruit de Perlin pour un effet de scintillement organique.
- **Pas de chevauchement audio** : le son du battement boucle nativement (`AudioSource.loop`) plutôt que d'être redéclenché à intervalle court — voir l'avertissement dans la section setup.

---

## Schéma récapitulatif

```
  SALLE B02                          COULOIR
┌────────────────────┐    ┌──────────────────────────────────────┐
│                    │    │                                      │
│  [Avatar]          │    │  [BoxCollider HallwayPanicEffect]    │
│    ↓ interaction   │    │    ↓ OnTriggerEnter                  │
│  Disparition objets│    │  PHASE 2 : respiration + vignette    │
│    ↓ délai réglé   │    │                                      │
│  PHASE 1 : cœur   │    │        [BoxCollider WallSlide]       │
│    ↓ fin dialogue  │    │          ↓ OnTriggerEnter            │
│  Porte s'ouvre ←───┼────┤        PHASE 3 : tout au max        │
│                    │    │        + couloir s'allonge            │
└────────────────────┘    └──────────────────────────────────────┘
```
