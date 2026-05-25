# SkullThrone — Roadmap

## Versioning Strategy

- Follows [Semantic Versioning 2.0.0](https://semver.org/)
- Each feature branch (`feat-*`) results in a minor or patch version bump
- Milestones represent major capability gates, not single releases
- Multiple versions will ship between milestones as features are completed incrementally

---

## Milestone 1 — Playable Level

The player can move through a rendered level in first-person with collision detection.

| Version | Feature | Branch Example |
|---------|---------|----------------|
| v0.1.0 | Project scaffolding (MonoGame, solution structure, build) | `feat-project-setup` |
| v0.2.0 | Basic raycasting engine (DDA, wall rendering, single texture) | `feat-raycaster-walls` |
| v0.3.0 | Player movement (forward, back, strafe) + input handling | `feat-player-movement` |
| v0.4.0 | Collision detection with wall sliding | `feat-collision-detection` |
| v0.5.0 | Textured walls (multiple texture IDs from map data) | `feat-wall-textures` |
| v0.6.0 | Floor and ceiling rendering | `feat-floor-ceiling` |
| v0.7.0 | Vertical look (Y-shearing) | `feat-vertical-look` |
| v0.8.0 | Sector height differences (variable floor/ceiling) | `feat-sector-heights` |
| v0.9.0 | Jumping | `feat-jumping` |
| v0.10.0 | Fullscreen/resizable window with aspect ratio preservation | `feat-window-scaling` |
| v0.11.0 | Game state machine (Menu, Playing, Paused, Game Over) | `feat-game-states` |
| v0.12.0 | JSON map loading (hand-written maps) | `feat-map-loading` |
| v0.13.0 | First test level (hand-crafted JSON) | `feat-first-level` |

---

## Milestone 2 — Pick-ups & Melee Combat

The player can collect items and fight with a chainsword.

| Version | Feature | Branch Example |
|---------|---------|----------------|
| TBD | Sprite rendering (billboarded entities in 3D view) | `feat-sprite-rendering` |
| TBD | Pick-up system (collect on overlap) | `feat-pickup-system` |
| TBD | Skull pick-up (+score) | `feat-skull-pickup` |
| TBD | Health pick-up (+HP) | `feat-health-pickup` |
| TBD | Ammo pick-up (+ammo) | `feat-ammo-pickup` |
| TBD | HUD: health, ammo, skull score (DOOM-style bar) | `feat-hud` |
| TBD | Chainsword weapon (melee state machine, cone attack) | `feat-chainsword` |
| TBD | Blood Rage power-up (timed instant-kill melee) | `feat-blood-rage` |
| TBD | Invincibility power-up (timed no-damage) | `feat-invincibility` |

---

## Milestone 3 — Ranged Combat

The player can shoot enemies at range.

| Version | Feature | Branch Example |
|---------|---------|----------------|
| TBD | Bolt pistol weapon (hitscan, ammo consumption) | `feat-bolt-pistol` |
| TBD | Weapon switching (chainsword ↔ bolt pistol) | `feat-weapon-switching` |
| TBD | Hit feedback (visual/screen flash) | `feat-hit-feedback` |

---

## Milestone 4 — Enemies

Enemies populate the level with AI-driven combat.

| Version | Feature | Branch Example |
|---------|---------|----------------|
| TBD | Enemy entity base (health, damage, death) | `feat-enemy-base` |
| TBD | Imperial Guard AI (fast, weak, group behavior) | `feat-imperial-guard` |
| TBD | Loyalist Astartes AI (slow, durable, heavy attacks) | `feat-loyalist-astartes` |
| TBD | Enemy sprites (idle, walk, attack, death animations) | `feat-enemy-sprites` |
| TBD | Line-of-sight detection (raycasting) | `feat-enemy-los` |
| TBD | A* pathfinding on tile grid | `feat-enemy-pathfinding` |
| TBD | Enemy attacks damage player | `feat-enemy-attacks` |
| TBD | Death/Game Over flow | `feat-death-flow` |

---

## Milestone 5 — Map Editor

Standalone tool for designing levels visually.

| Version | Feature | Branch Example |
|---------|---------|----------------|
| TBD | Editor project scaffolding | `feat-editor-setup` |
| TBD | Grid rendering with zoom/pan | `feat-editor-grid` |
| TBD | Tile painting (wall placement/deletion) | `feat-editor-tiles` |
| TBD | Entity placement (enemies, pickups, spawn) | `feat-editor-entities` |
| TBD | Sector editing (floor/ceiling heights) | `feat-editor-sectors` |
| TBD | Undo/redo | `feat-editor-undo` |
| TBD | Map validation + JSON export | `feat-editor-export` |

---

## Post-1.0 (Future)

- Audio / music / sound effects
- Procedural level generation
- Adeptus Mechanicus enemy faction
- Additional weapons
- Multiple hand-crafted levels
- Cross-platform support (Linux, Mac)

---

## Release: v1.0.0

All of Milestones 1–4 complete. One fully playable level with:
- Raycasting engine with all visual features
- Two weapons (chainsword + bolt pistol)
- Two power-ups (Blood Rage + Invincibility)
- Three pick-up types (skulls, ammo, health)
- Two enemy factions (Imperial Guard + Loyalist Astartes)
- DOOM-style HUD
- Game state management (menu, pause, game over)
- Map editor (Milestone 5) available as a development tool
