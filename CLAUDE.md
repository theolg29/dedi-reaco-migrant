# dedi-reaco-migrant

Projet Unity VR (XR Interaction Toolkit). Expérience narrative immersive.

## Conventions

- **Noms de variables, méthodes et champs** : en français (ex: `joueur`, `couloirSortie`, `actif`)
- **Noms de fichiers et classes C#** : en anglais (ex: `WallSlide.cs`)
- **Headers et Tooltips Inspector** : en français
- **Préférer un seul script/GameObject** plutôt que plusieurs quand c'est possible — plus clair, plus propre

## Structure des salles

Vue du dessus : AO3 | B02 | AB42 (au fond)
Ordre de jeu : on commence par AO3, puis B02, puis on termine par AB42.

Anciens noms (pour référence) : AO3 = ex-Bureau 01, B02 = ex-Bureau 02, AB42 = ex-Bureau 03.

L'effet de panique progressif (battement de cœur → respiration/vignette → intensité max) se déclenche après la salle **B02**, dans le couloir vers AB42 — voir `DOCS/panique-effect.md`.

## Documentation des scripts

Le dossier `DOCS/` contient une documentation détaillée de tous les scripts C# du projet, par système :

- [`DOCS/README.md`](./DOCS/README.md) — index général, vue d'ensemble de tous les scripts
- [`DOCS/bureau-et-salles.md`](./DOCS/bureau-et-salles.md) — `BureauManager`, `PrinterAnimation`, `FeuilleRecuperable`
- [`DOCS/panique-effect.md`](./DOCS/panique-effect.md) — `HallwayPanicEffect`, `WallSlide`
- [`DOCS/portes-et-sons-ambiants.md`](./DOCS/portes-et-sons-ambiants.md) — `DoorInteractable`, `DoorMuffledVolume`, `SeatsDiscussion`, `FanRotator`
- [`DOCS/divers.md`](./DOCS/divers.md) — surbrillance, fondu d'écran, intro, locomotion dev

À consulter avant de modifier un script existant, et à mettre à jour après tout changement de comportement ou de câblage entre scripts.
