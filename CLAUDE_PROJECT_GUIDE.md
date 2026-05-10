# Super Mario WinForms - Claude Project Guide

This guide provides a comprehensive overview of the `supermario` C# Windows Forms codebase, detailing the architecture, systems, and guidelines for integrating new textures and assets in future Claude Code sessions.

## 1. Full Project Architecture

The project is built as a **C# Windows Forms Application** targeting .NET Framework (v4.7.2). It simulates a 2D side-scrolling platformer without using a heavy game engine (like Unity or MonoGame). All rendering, physics, and game loop logic are implemented using WinForms UI components, timers, and GDI+ (`System.Drawing`).

### File and Folder Structure

```
supermario/
├── supermario.sln                  # Main Visual Studio Solution
├── supermario/                     # Main Project Directory
│   ├── supermario.csproj           # C# Project definition
│   ├── Program.cs                  # Application Entry Point
│   ├── assets/
│   │   └── textures/               # [NEW] Dedicated texture folder
│   │       ├── player/
│   │       ├── enemies/
│   │       ├── terrain_blocks/
│   │       ├── pipes/
│   │       ├── collectibles/
│   │       ├── ui_menu/
│   │       ├── backgrounds/
│   │       └── effects/
│   ├── Core/                       # Core gameplay data & entity definitions
│   │   ├── GameManager.cs
│   │   ├── Player.cs
│   │   └── GameData.cs
│   ├── Enemies/                    # Enemy logic and behaviors
│   │   ├── Goomba.cs
│   │   ├── Koopa.cs
│   │   ├── FastEnemy.cs
│   │   ├── JumpingEnemy.cs
│   │   ├── PlatformPatrolEnemy.cs
│   │   └── FlyingEnemy.cs
│   ├── World/                      # World objects and physics bodies
│   │   └── GameObjectS.cs
│   ├── UI/                         # UI screens, rendering, and partial game loop
│   │   ├── MainMenuForm.cs         # Title Screen Menu
│   │   ├── mainWin.cs              # Main Game Window
│   │   ├── mainWin.LevelData.cs    # Level map definitions
│   │   ├── mainWin.LevelBuilder.cs # Translates map data to physical objects
│   │   ├── mainWin.Collectibles.cs # Coin/Mushroom logic
│   │   ├── mainWin.EnemyUpdates.cs # Enemy loop integration
│   │   ├── mainWin.HUD.cs          # UI Overlays (score, lives)
│   │   └── mainWin.Physics.cs      # Collision & Gravity calculation
│   ├── Properties/                 # Assembly info & resx properties
│   └── Resources/                  # Pre-existing embedded resources (.resx)
```

## 2. Core Systems

* **Game Loop**: Driven by a `Timer` inside `mainWin.cs`. On every tick, the game updates physics, moves enemies, checks collisions, and triggers `Invalidate()` to force a redraw.
* **Rendering**: Handled via WinForms `OnPaint` (or `Paint` event). Elements are drawn directly onto the form's `Graphics` context using `DrawImage`.
* **State Management**: Handled via `GameManager` and UI state logic in `mainWin`.
* **Physics & Collisions**: Found in `mainWin.Physics.cs`. Uses Axis-Aligned Bounding Box (AABB) collision checks between the player/enemies and the tiles (`GameObjectS`).

## 3. Entity Systems

### Player System
* `Core/Player.cs`: Maintains player state (health, speed, jump velocity, state such as Small/Super Mario).
* Player rendering and control logic are deeply intertwined with the `mainWin` partial classes, specifically physics for jumping/gravity and keyboard events for movement.

### Enemy System
* Located under `Enemies/`.
* Base concepts rely on shared behavior arrays or inheritance.
* The game loop iterates over active enemies (handled in `mainWin.EnemyUpdates.cs`) updating their positions and handling collision with the player (damage/stomping) or walls (reversing direction).
* Different enemy types feature custom movement rules (e.g., `FlyingEnemy` ignores gravity, `JumpingEnemy` applies upward velocity on timers).

### Level & World Systems
* Defined in `mainWin.LevelData.cs` typically as 2D arrays or string maps.
* `mainWin.LevelBuilder.cs` parses the level maps and generates `GameObjectS` (which are effectively solid blocks or interactive tiles).
* `World/GameObjectS.cs` acts as the base data structure for world colliders.

## 4. Texture Integration & Asset Replacement

**Currently**, assets are embedded into the `.resx` files (e.g., `Properties/Resources.resx` and `Resources/` folder).

**New Workflow for Asset Expansion:**
1. **Place Assets**: Put all new PNG/sprites in the newly created `assets/textures/...` subdirectories.
2. **Dynamic Loading (Recommended)**: Instead of locking assets into the `Resources.resx`, future modifications should update the code to load images dynamically using `Image.FromFile()` from the `assets/textures/` directory. This makes swapping textures dramatically easier.
   - *Example:* `Image playerSprite = Image.FromFile("assets/textures/player/mario_idle.png");`
3. **Sprite States**: Group animations logically. Ensure filenames denote state (e.g., `goomba_walk_1.png`, `goomba_walk_2.png`).
4. **Caching**: Because WinForms GDI+ can be slow when loading images every frame, ensure images are loaded *once* during `mainWin_Load` or a custom initialization step, and cached in `Dictionary<string, Image>` or similar structures.

## 5. Build & Execution Instructions

Since this is a standard .NET WinForms project:
1. **Building**: Open `supermario.sln` in Visual Studio (or Rider) and build.
   - Alternatively, build via MSBuild: `msbuild supermario.sln /p:Configuration=Debug`
2. **Running**: Execute the generated `.exe` in `bin/Debug/supermario.exe` or hit `F5` in Visual Studio.

## 6. Important Technical Constraints

- **WinForms GDI+ Performance**: WinForms is NOT a game engine. Heavy rendering of hundreds of high-res textures per frame *will* cause lag. Keep textures optimized (small dimensions) and strictly avoid calling `Image.FromFile` inside the `Paint` event.
- **Double Buffering**: Ensure `DoubleBuffered = true` remains active on the main form to prevent screen flickering.
- **No Native Animation Engine**: Animations must be handled manually by cycling arrays of `Image` objects based on a timer tick or frame counter.

## 7. Best Practices for Future Claude Code Sessions

- **Context limits**: When editing physics, only include `mainWin.Physics.cs` rather than the whole `mainWin` family.
- **Asset references**: If you add new UI assets or textures, ensure the paths are relative to the executable (e.g., `AppDomain.CurrentDomain.BaseDirectory + @"\assets\textures\..."`) OR set the assets in `.csproj` to "Copy if newer". For this project, we'll configure MSBuild to copy the `assets/` folder to the output directory automatically.
- **Avoid massive refactors**: WinForms UI files and `.Designer.cs` can easily break if the partial class structure is modified carelessly. Always rely on targeted file edits.
