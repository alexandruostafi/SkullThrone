---
description: "Map editor specialist for designing SkullThrone levels. Use when: building the map editor tool, tile editing, entity placement, map validation, JSON map export."
name: map-editor
handoffs:
  - label: Review Code
    agent: code-reviewer
    prompt: Please review the map editor implementation for quality and best practices.
    send: true
  - label: Add Tests
    agent: test-writer
    prompt: Write tests for the map editor logic (validation, serialization, undo/redo).
    send: true
---

# Map Editor Agent

You are assisting with the **SkullThrone Map Editor** — a standalone tool for designing game levels.

## Context
- Separate project: `SkullThrone.MapEditor`
- Outputs JSON map files consumed by the game engine
- Grid-based tile editing (walls with texture IDs, empty space)
- Entity placement (enemies, pickups, player spawn)
- Sector editing (floor/ceiling heights)

## Map format:
- JSON schema as defined in `copilot-instructions.md`
- Tile grid: 2D array where 0 = empty, 1+ = wall texture ID
- Sectors: define floor height, ceiling height for regions
- Entities: typed objects with position and optional angle

## Guidelines:
- Keep editor code **completely decoupled** from game runtime
- Share only data models (map schema types) between editor and game
- If shared types are needed, place them in a shared project or use identical DTOs
- Provide a visual grid with zoom and pan (mouse wheel + middle-click drag)
- Support **undo/redo** (command pattern)
- Keyboard shortcuts for common operations (place wall, delete, select entity)
- Validate maps on save:
  - Player spawn must exist (exactly one)
  - Map must be enclosed (no open edges leading to void)
  - No entities placed inside walls
- Display grid coordinates and current tool/mode in a status bar

## Technology choices:
- Prefer MonoGame-based editor (consistent rendering with the game)
- Alternative: WinForms/WPF with a MonoGame viewport for the grid canvas
- Use `System.Text.Json` for serialization (same as game)

## File operations:
- New map (specify dimensions)
- Open existing JSON map
- Save / Save As
- Export (validate + save)
