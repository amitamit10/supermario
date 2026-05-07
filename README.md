# Super Mario — C# WinForms Platformer

A Mario-style 2D platformer built with C# .NET Framework 4.7.2 and Windows Forms.  
All graphics are drawn procedurally with GDI+ (no external sprite sheets).

---

## Project Structure

```
supermario/
│
├── Program.cs                        Entry point (Application.Run)
├── App.config
├── supermario.csproj
│
├── Core/                             Fundamental game classes
│   ├── GameManager.cs                Game state (IsGameRunning, Start/End/Reset)
│   ├── Player.cs                     Player physics, health, jump, bounce
│   └── GameData.cs                   Shared data types:
│                                       Mushroom, Coin, QuestionBlock,
│                                       PowerUpType, GraphicsExtensions
│
├── Enemies/                          One file per enemy type
│   ├── Goomba.cs                     Basic walker — squish on stomp
│   ├── Koopa.cs                      Turtle — first stomp gives shell
│   ├── FastEnemy.cs                  Red fast walker (2× speed)
│   ├── JumpingEnemy.cs               Blue bouncer — jumps periodically
│   ├── PlatformPatrolEnemy.cs        Orange patroller — turns at ledge edges
│   └── FlyingEnemy.cs                Winged Parakoopa — sine-wave flight,
│                                       stomp removes wings then squishes
│
├── World/                            Level geometry helpers
│   └── GameObjectS.cs                Platform/pipe wrapper (PictureBox + world pos)
│
├── UI/                               Everything the player sees
│   ├── MainMenuForm.cs               Animated GDI+ main menu
│   │
│   ├── mainWin.cs                    Game window — fields, constructor,
│   │                                   game loop, input, background painting
│   ├── mainWin.Designer.cs           Auto-generated WinForms designer code
│   ├── mainWin.resx                  Auto-generated resource bindings
│   │
│   ├── mainWin.LevelData.cs          All static level data:
│   │                                   platform coords, pipe coords,
│   │                                   Q-block positions, enemy spawn points,
│   │                                   coin routes, section templates
│   ├── mainWin.LevelBuilder.cs       CreateLongLevel, AddPlatform/Pipe/Coins/
│   │                                   Flagpole, ClearPlatforms, Draw* tile
│   │                                   handlers, GenerateRandomLevel
│   ├── mainWin.Physics.cs            CheckPlatformCollisions, HandleFallDamage,
│   │                                   CheckWinCondition, UpdateCamera,
│   │                                   BecomeSuper/Normal, RestartLevel,
│   │                                   DoLevelSetup, ClearPlatforms
│   ├── mainWin.EnemyUpdates.cs       Spawn* and Update* for all 6 enemy types
│   ├── mainWin.Collectibles.cs       Coins and mushrooms (spawn, update, draw)
│   └── mainWin.HUD.cs                DrawPlayerSprite, InitHud, UpdateHud
│
└── Properties/
    ├── AssemblyInfo.cs
    ├── Resources.Designer.cs
    ├── Resources.resx
    ├── Settings.Designer.cs
    └── Settings.settings
```

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                        UI Layer                         │
│  MainMenuForm  ──►  mainWin (8 partial-class files)     │
│                         │                               │
│           ┌─────────────┼──────────────┐                │
│           ▼             ▼              ▼                │
│      LevelData     LevelBuilder    Physics              │
│      (static       (build /        (collision,          │
│       arrays)       clear world)    camera, death)      │
│                         │                               │
│           ┌─────────────┼──────────────┐                │
│           ▼             ▼              ▼                │
│     EnemyUpdates   Collectibles      HUD                │
└─────────────────────────────────────────────────────────┘
           │                  │
           ▼                  ▼
┌──────────────────┐  ┌───────────────────────────────┐
│   Enemies/       │  │   Core/                       │
│  Goomba          │  │  Player   (physics, health)   │
│  Koopa           │  │  GameManager (run/stop state) │
│  FastEnemy       │  │  GameData  (Mushroom, Coin,   │
│  JumpingEnemy    │  │             QuestionBlock)    │
│  PlatformPatrol  │  └───────────────────────────────┘
│  FlyingEnemy     │
└──────────────────┘
           │
           ▼
┌──────────────────┐
│   World/         │
│  GameObjectS     │  (wraps PictureBox + world-space position)
└──────────────────┘
```

---

## Controls

| Key | Action |
|-----|--------|
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `W` / `↑` / `Space` | Jump |
| `Escape` | Pause |
| `Enter` | Resume (when paused) |

---

## Enemy Behaviour Summary

| Enemy | Movement | Stomp result | Damage |
|-------|----------|--------------|--------|
| Goomba | Walks, reverses on walls | Squish → despawn | -1 HP |
| Koopa | Walks slower | Shell (auto-despawn) | -1 HP |
| FastEnemy | Walks 2× speed | Squish → despawn | -1 HP |
| JumpingEnemy | Walks + jumps every ~1.5 s | Squish → despawn | -1 HP |
| PlatformPatrolEnemy | Walks, turns at ledge edges | Squish → despawn | -1 HP |
| FlyingEnemy | Sine-wave flight | 1st stomp removes wings, 2nd squishes | -1 HP |

---

## Tech Notes

- **Rendering**: All sprites drawn with GDI+ (`LinearGradientBrush`, `GraphicsPath`, `SmoothingMode.AntiAlias`) — no image files for enemies or tiles.
- **Physics**: Fixed 16 ms timestep; timer fires every 8 ms and accumulates steps.
- **Collision**: AABB overlap method — resolves smallest overlap axis first.
- **Camera**: Horizontal scroll only; parallax background at 8 %, 12 %, 25 % speeds.
- **Levels**: 3 hand-crafted levels + 2 procedurally generated levels using section templates.
