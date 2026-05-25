# SkullThrone — Project Coding Guidelines

## Project Overview
SkullThrone is a retro first-person shooter (DOOM/Wolfenstein-style) built with C# .NET 10 and MonoGame.
The player is a betrayed World Eater tearing through loyalist Astartes and Imperial Guard, harvesting skulls for the Skull Throne.

### Core Technical Pillars
- **Rendering**: Grid-based raycasting (DDA), pseudo-3D (Y-shearing vertical look, sector height differences, jumping)
- **Resolution**: Retro low-res (320×200 logical resolution, scaled to window)
- **Framework**: MonoGame (no Unity/Unreal/Godot)
- **Platform**: Windows (cross-platform consideration later)
- **Single-player only**

### Gameplay Pillars
- First-person perspective
- Melee + ranged weapons (chainsword, bolt pistol, more later)
- Blood Rage power-up (timed instant-kill melee)
- DOOM-style HUD (health, ammo, skull score)
- Pick-ups: skulls (score/bragging rights), ammunition, health

### Enemy Factions
- Loyalist Astartes (heavy, durable, slow)
- Imperial Guard (numerous, weaker, fast)
- Adeptus Mechanicus (future addition)

### Level Design
- Hand-crafted levels via custom JSON-based map editor (priority)
- Procedural generation planned for later development phases

### Milestones
1. Playable level (raycasting engine, player movement, map editor)
2. Pick-ups (skulls/score, ammo, health) + chainsword (melee weapon)
3. Bolt pistol (ranged weapon)
4. Enemies (AI, combat)

---

## Software Versioning
- Semantic Versioning 2.0.0: https://semver.org/#semantic-versioning-200

## Commit Message Format
- Conventional Commits: https://www.conventionalcommits.org/en/v1.0.0/

## Branching Strategy
- Single `main` branch for Continuous Delivery
- Branch name format:
  * `feat-<description>` — new feature
  * `fix-<description>` — bug fix
  * `refactor-<description>` — refactor without behavior change

---

## Development Tools
- **Language**: C# 13 / .NET 10
- **Framework**: MonoGame
- **IDE**: Visual Studio 2026
- **Build**: MSBuild / dotnet CLI
- **Testing**: xUnit
- **Static Analysis**: .NET Analyzers, nullable reference types enabled
- **Documentation**: XML doc comments, Markdown in `docs/`

## C# Coding Guidelines
- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Nullable reference types: enabled (`<Nullable>enable</Nullable>`)
- Use file-scoped namespaces
- Prefer `readonly` and immutability where practical
- Use pattern matching and switch expressions where they improve clarity
- Game loop code must be allocation-free in hot paths (avoid GC pressure)
- Separate engine/core systems from game-specific logic

## Architecture Guidelines
- **Entity-Component pattern** for game objects — keep data and behavior separable
- **Systems**: Rendering, Physics, Input, Audio, AI, UI — each in its own namespace/folder
- **Map data**: JSON format, deserialized into typed C# models
- **State management**: Game states via a state machine (Menu, Playing, Paused, GameOver)

## Performance Rules
- Zero allocations in Update/Draw hot path
- Pre-allocate arrays/buffers for raycasting
- Use `Span<T>` and `stackalloc` where appropriate
- Pool frequently created/destroyed objects (projectiles, particles)
- Profile before optimizing — measure first

## Map Format (JSON)
Maps are grid-based. Example schema:
```json
{
  "name": "Blood Pits",
  "width": 32,
  "height": 32,
  "playerSpawn": { "x": 5, "y": 5, "angle": 90 },
  "tiles": [],
  "sectors": [],
  "entities": [
    { "type": "SkullPickup", "x": 10, "y": 12 },
    { "type": "HealthPickup", "x": 14, "y": 8 },
    { "type": "AmmoPickup", "x": 6, "y": 3 },
    { "type": "EnemyGuard", "x": 20, "y": 15, "angle": 180 }
  ]
}
```

## Project Structure
```
SkullThrone/
├── .github/
│   ├── copilot-instructions.md
│   └── agents/
│       ├── gamedev.md
│       ├── map-editor.md
│       └── tests.md
├── src/
│   ├── SkullThrone/                # Main game project
│   │   ├── Core/                   # Engine-level systems
│   │   │   ├── Raycaster/          # Raycasting engine (DDA, rendering)
│   │   │   ├── Input/              # Input handling
│   │   │   ├── Audio/              # Sound/music
│   │   │   ├── Physics/            # Collision, movement, jumping
│   │   │   └── StateManagement/    # Game state machine
│   │   ├── Game/                   # Game-specific logic
│   │   │   ├── Entities/           # Player, enemies, pickups
│   │   │   ├── Weapons/            # Weapon definitions & behavior
│   │   │   ├── Levels/             # Level loading, map data models
│   │   │   ├── AI/                 # Enemy AI
│   │   │   ├── Powerups/           # Blood Rage, etc.
│   │   │   └── HUD/               # DOOM-style UI
│   │   ├── Content/                # MonoGame content pipeline assets
│   │   │   ├── Textures/
│   │   │   ├── Sprites/
│   │   │   ├── Maps/              # JSON map files
│   │   │   ├── Sounds/
│   │   │   └── Fonts/
│   │   └── Program.cs
│   └── SkullThrone.MapEditor/      # Map editor tool (separate project)
├── tests/
│   └── SkullThrone.Tests/          # Unit tests (xUnit)
├── docs/                           # Design documents, notes
├── SkullThrone.sln
└── README.md
```
