# Effet de panique — HallwayPanicEffect

## Résumé

L'effet de panique simule une crise d'angoisse progressive quand le joueur traverse le couloir après la salle B02. Il se décompose en **3 phases** qui s'enchaînent automatiquement au fil de la progression du joueur, du plus subtil au plus intense.

---

## Vue d'ensemble des 3 phases

| Phase | Déclencheur | Ce qui se passe |
|-------|-------------|-----------------|
| **1 — Battement de cœur** | `BureauManager` (salle B02), après `delaiAvantBattementCoeur` secondes depuis le début de l'interaction avatar | Un battement court ("lub-dub") est rejoué à intervalle régulier (rythme calme), le volume monte progressivement ; une très légère vibration manette accompagne **chaque** battement, exactement en même temps que le son |
| **2 — Respiration + vignette** | Le joueur entre dans le couloir (BoxCollider trigger) | Un son de respiration apparaît, un voile noir commence à assombrir la vision ; le cœur **bat plus souvent** (intervalle plus court) et la vibration s'intensifie un peu |
| **3 — Intensité max** | Le couloir commence à s'allonger (WallSlide) | Le cœur bat **très vite** (intervalle minimal), shake caméra, vibrations manettes au maximum, tout monte au maximum |

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
| `Son Battement Coeur` | AudioClip **court** contenant un battement complet "lub-dub" (deux pulsations, ex: ~0.4-0.6s) | Rejoué via `PlayOneShot` à chaque battement — ⚠️ pas une ambiance longue, voir piège ci-dessous |
| `Son Respiration` | AudioClip de respiration en boucle | Joué en continu à partir de la phase 2 |
| `Cible Shake` | Le **Camera Offset** du XR Origin | Objet secoué pour simuler le tremblement (⚠️ jamais la caméra directement) |
| `Intervalle Battement Depart` / `Phase2` / `Max` | Des durées en secondes entre deux battements | Rythme du cœur par phase (ex: 1s ≈ 60 bpm → 0.6s ≈ 100 bpm → 0.35s ≈ 170 bpm) — pilote **à la fois** le déclenchement du son et la vibration manette, donc les deux sont toujours parfaitement synchronisés |
| `Intensite Vibration Phase1` / `Intensite Vibration Phase2` / `Intensite Vibration` | Des intensités (0-1) | Intensité de la vibration manette par phase — légère en Phase 1, modérée en Phase 2, maximale en Phase 3 |

⚠️ **Piège évité (mis à jour)** : un cœur qui s'accélère ne change pas de hauteur (pitch), il bat juste plus **souvent** — la vitesse vient donc de l'intervalle entre les battements, pas d'un pitch qu'on augmente. Et puisque le clip du battement est **court** (durée ≤ intervalle le plus serré), le rejouer via `PlayOneShot` à chaque battement ne tronque jamais le son (contrairement à redémarrer un clip **long** à intervalle court, qui ne ferait entendre que son attaque en boucle serrée).

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
- Le battement court (`Son Battement Coeur`) commence à être rejoué à intervalle régulier, en partant de `Intervalle Battement Depart` (ex: 1s ≈ 60 bpm, rythme calme)
- Le volume part de `Volume Battement Depart` (très bas, ex: 0.1)
- Il monte progressivement vers `Volume Battement Milieu` (ex: 0.35)
- Une vibration manette très légère (`Intensite Vibration Phase1`, ex: 0.05) accompagne **chaque** battement, exactement en même temps que le son
- La vitesse de montée (volume + intervalle) est contrôlée par `Vitesse Montee Battement`

**Ressenti joueur :** Un léger malaise, quelque chose ne va pas.

### Phase 2 — Respiration + vignette

**Quand :** Le joueur sort de la salle B02 et entre dans le BoxCollider du couloir.

**Ce qui se passe :**
- Le son de respiration démarre (volume à 0, monte vers `Volume Respiration Milieu`)
- Un voile noir apparaît progressivement sur les bords de la vision (vignette)
- L'opacité monte vers `Opacite Vignette Milieu`
- Le battement de cœur continue sa progression et **accélère** (l'intervalle se resserre vers `Intervalle Battement Phase2`, ex: 0.6s ≈ 100 bpm) — le cœur bat plus souvent
- La vibration manette s'intensifie un peu (`Intensite Vibration Phase2`, ex: 0.15) et ses impulsions se rapprochent en même temps que le son (même intervalle)
- Le joueur voit que les meubles et PNJ ont disparu

**Ressenti joueur :** L'angoisse monte, la vision se réduit, on entend sa propre respiration.

### Phase 3 — Intensité max

**Quand :** Le joueur atteint le BoxCollider du WallSlide (le couloir commence à s'allonger).

**Ce qui se passe :**
- **Battement de cœur** → volume monte vers 1.0 (maximum), intervalle se resserre vers `Intervalle Battement Max` (ex: 0.35s ≈ 170 bpm) — le cœur bat **très souvent**
- **Respiration** → volume monte vers 1.0
- **Vignette** → opacité monte vers `Opacite Vignette Max` (ex: 0.85)
- **Shake caméra** → la caméra tremble à chaque battement (intensité monte vers `Intensite Shake`), au même instant que le son et la vibration
- **Vibrations manettes** → intensité au maximum (`Intensite Vibration`) et impulsions au rythme le plus rapide, toujours synchronisées avec le son
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
- **Pas de troncature audio** : le battement est un clip **court** rejoué via `PlayOneShot` (qui superpose les voix sans rien couper) plutôt qu'un long clip redémarré — voir l'avertissement dans la section setup.

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
