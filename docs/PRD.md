# SkullThrone — Product Requirements Document

## 1. Overview

**SkullThrone** is a retro first-person shooter inspired by DOOM and Wolfenstein 3D. The player is a betrayed World Eater — a Chaos Space Marine — who tears through loyalist Astartes and Imperial Guard, harvesting skulls for the Skull Throne.

**Type:** Personal / hobby project
**Platform:** Windows (cross-platform consideration later)
**Framework:** C# .NET 10, MonoGame

---

## 2. Vision

A fast-paced, violent retro FPS with a Warhammer 40K-inspired aesthetic. Low-res pixel art, chunky weapons, screen-filling enemies, and satisfying combat. One level, one goal: harvest skulls.

---

## 3. Technical Specifications

| Spec | Value |
|------|-------|
| Rendering | Grid-based raycasting (DDA algorithm) |
| Projection | Pseudo-3D: Y-shearing vertical look, sector height differences, jumping |
| Logical resolution | 320×200 |
| Window | Resizable, supports fullscreen mode |
| Aspect ratio | Maintain logical aspect ratio with letterboxing/pillarboxing |
| Frame rate | Uncapped (target 60+ FPS) |
| Audio | None for v1.0 (silent) |
| Multiplayer | None (single-player only) |

---

## 4. Gameplay

### 4.1 Core Loop

1. Player spawns in level
2. Navigate grid-based map in first-person
3. Kill enemies with melee and ranged weapons
4. Collect skulls (score), ammo, and health pick-ups
5. Survive and maximize skull count

### 4.2 Player

- First-person perspective only
- Movement: forward, backward, strafe left/right
- Vertical look: Y-shearing (pseudo look up/down)
- Jumping: parabolic arc, affects render height
- Health: 0–100 (dies at 0)

### 4.3 Weapons

| Weapon | Type | Description |
|--------|------|-------------|
| Chainsword | Melee | Short-range cone attack, moderate damage |
| Bolt Pistol | Ranged (hitscan) | Medium damage, uses ammo |

Weapons are state machines: Idle → Firing → Cooldown → Idle

### 4.4 Power-ups

| Power-up | Effect |
|----------|--------|
| Blood Rage | Timed buff — all melee attacks instantly kill for duration |
| Invincibility | Timed buff — player takes no damage for duration |

### 4.5 Pick-ups

| Pick-up | Effect |
|---------|--------|
| Skull | +1 score (bragging rights only) |
| Ammo | Restores bolt pistol ammunition |
| Health | Restores player health |

### 4.6 Enemies

| Faction | Traits |
|---------|--------|
| Imperial Guard | Numerous, weak, fast movement, group tactics |
| Loyalist Astartes | Few, durable, slow movement, heavy damage |

Enemy AI: Finite state machine (Idle → Alert → Chasing → Attacking → Dead)
Detection: Line-of-sight via raycasting
Pathfinding: A* on tile grid

### 4.7 HUD

DOOM-style bottom bar:
- Health (numeric)
- Ammo count (numeric)
- Skull score (numeric)

---

## 5. Level Design

### 5.1 Map Format

- Grid-based tile maps stored as JSON
- Tiles: 2D array (0 = empty, 1+ = wall texture ID)
- Sectors: floor/ceiling height overrides
- Entities: typed objects with position and optional facing angle

### 5.2 Map Editor

- Standalone tool (`SkullThrone.MapEditor` project)
- Visual grid with zoom/pan
- Tile painting, entity placement, sector editing
- Undo/redo support
- Validation on save (spawn exists, map enclosed, no entities in walls)
- Export: JSON

### 5.3 Procedural Generation

Planned for post-1.0. Not in scope for initial release.

---

## 6. Release Scope (v1.0)

**One complete level** with all systems functional:

- [x] Raycasting engine rendering walls, floors, ceilings
- [x] Player movement + jumping + vertical look
- [x] Collision detection with wall sliding
- [x] Map editor producing JSON levels
- [x] Pick-ups: skulls, ammo, health
- [x] Chainsword (melee weapon)
- [x] Bolt pistol (ranged weapon)
- [x] Blood Rage power-up
- [x] Enemies: Imperial Guard + Loyalist Astartes
- [x] Enemy AI (patrol, detect, chase, attack)
- [x] DOOM-style HUD
- [x] Game states: Menu → Playing → Paused → Game Over
- [ ] Audio (deferred — silent for v1.0)
- [ ] Multiple levels (post-1.0)
- [ ] Procedural generation (post-1.0)
- [ ] Adeptus Mechanicus enemies (post-1.0)

---

## 7. Milestones

| # | Milestone | Deliverables |
|---|-----------|-------------|
| 1 | Playable Level | Raycasting engine, player movement, collision, map editor, one test map |
| 2 | Combat (Melee) | Skull/ammo/health pick-ups, chainsword weapon, Blood Rage |
| 3 | Combat (Ranged) | Bolt pistol, ammo system |
| 4 | Enemies | Imperial Guard AI, Loyalist Astartes AI, full combat loop |

---

## 8. Non-Goals (Explicitly Out of Scope)

- Multiplayer / co-op
- Story / cutscenes / dialogue
- Audio / music (v1.0)
- Cross-platform (v1.0)
- Procedural level generation (v1.0)
- Economy / currency / unlocks (skulls are score only)
- Save system (single level, play in one session)

---

## 9. Technical Architecture

```
Core/                    Game/
├── Raycaster/           ├── Entities/
├── Input/               ├── Weapons/
├── Physics/             ├── Levels/
├── Audio/ (future)      ├── AI/
└── StateManagement/     ├── Powerups/
						 └── HUD/
```

- Dependency flows one way: `Game/` depends on `Core/`, never the reverse
- Entity-Component pattern for game objects
- Zero allocations in Update/Draw hot paths
- `System.Text.Json` for map serialization

---

## 10. Quality Requirements

- Unit tests (xUnit) for all core systems
- Nullable reference types enabled
- .NET Analyzers enabled
- Build must pass with zero warnings
- All tests pass before merge to `main`
