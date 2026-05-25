# SkullThrone

> A betrayed World Eater tears through endless armies of loyalist Astartes and Imperial Guard, harvesting skulls for the Skull Throne.

A retro first-person shooter inspired by DOOM and Wolfenstein 3D, built from scratch with C# .NET 10 and MonoGame.

---

## Features (Planned for v1.0)

- Grid-based raycasting engine (DDA) with pseudo-3D rendering
- 320×200 logical resolution scaled to resizable/fullscreen window
- Melee combat (chainsword) and ranged combat (bolt pistol)
- Power-ups: Blood Rage (instant-kill melee) and Invincibility (no damage)
- Pick-ups: skulls (score), ammo, health
- Two enemy factions: Imperial Guard and Loyalist Astartes
- DOOM-style HUD
- JSON-based hand-crafted levels
- Standalone map editor

## Tech Stack

| | |
|---|---|
| Language | C# 13 / .NET 10 |
| Framework | MonoGame |
| Testing | xUnit |
| Platform | Windows |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (or `dotnet` CLI)

### Build & Run

```bash
dotnet restore
dotnet build
dotnet run --project src/SkullThrone
```

## Project Structure

```
SkullThrone/
├── src/
│   ├── SkullThrone/            # Main game
│   └── SkullThrone.MapEditor/  # Level editor tool
├── tests/
│   └── SkullThrone.Tests/      # Unit tests (xUnit)
└── docs/                       # PRD, Roadmap, design notes
```

## Documentation

- [Product Requirements](docs/PRD.md)
- [Roadmap](docs/ROADMAP.md)

## Contributing

This is a personal hobby project. Not accepting contributions at this time.

## License

TBD
