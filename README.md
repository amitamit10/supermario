# Super Mario — C# WinForms Platformer

A Mario-style 2D platformer built with C# .NET Framework 4.7.2 and Windows Forms.  
Core sprites render from generated pixel-art sprite sheets, with GDI+ fallbacks for variants not covered by the texture pack.

---

## File Organization

```mermaid
flowchart TD
    classDef entry   fill:#4a4a8a,stroke:#9090ff,color:#fff
    classDef core    fill:#2d6a2d,stroke:#60c060,color:#fff
    classDef enemies fill:#7a2d2d,stroke:#d06060,color:#fff
    classDef world   fill:#5a4a1a,stroke:#c09030,color:#fff
    classDef ui      fill:#1a4a6a,stroke:#4090c0,color:#fff

    PC["Program.cs\n▸ entry point"]:::entry

    subgraph CORE["📂 Core/"]
        GM["GameManager.cs\n▸ game state"]:::core
        PL["Player.cs\n▸ physics · health · jump"]:::core
        GD["GameData.cs\n▸ Mushroom · Coin · QuestionBlock"]:::core
    end

    subgraph ENEMIES["📂 Enemies/"]
        GOO["Goomba.cs\n▸ basic walker"]:::enemies
        KOP["Koopa.cs\n▸ shell turtle"]:::enemies
        FAS["FastEnemy.cs\n▸ 2× speed walker"]:::enemies
        JMP["JumpingEnemy.cs\n▸ periodic jumper"]:::enemies
        PAT["PlatformPatrolEnemy.cs\n▸ ledge-aware patroller"]:::enemies
        FLY["FlyingEnemy.cs\n▸ sine-wave flyer"]:::enemies
    end

    subgraph WORLD["📂 World/"]
        GOS["GameObjectS.cs\n▸ platform / pipe wrapper"]:::world
    end

    subgraph UI["📂 UI/"]
        MMF["MainMenuForm.cs\n▸ animated main menu"]:::ui
        MW["mainWin.cs\n▸ fields · game loop · input"]:::ui
        MWD["mainWin.Designer.cs\n▸ auto-generated"]:::ui
        LD["mainWin.LevelData.cs\n▸ level arrays & spawn data"]:::ui
        LB["mainWin.LevelBuilder.cs\n▸ build / clear world"]:::ui
        PH["mainWin.Physics.cs\n▸ collision · camera · death"]:::ui
        EU["mainWin.EnemyUpdates.cs\n▸ spawn · update all enemies"]:::ui
        CO["mainWin.Collectibles.cs\n▸ coins · mushrooms"]:::ui
        HUD["mainWin.HUD.cs\n▸ sprites · HUD overlay"]:::ui
    end

    PC --> CORE
    PC --> UI
    UI --> ENEMIES
    UI --> WORLD
```

---

## Architecture — How the Layers Connect

```mermaid
flowchart LR
    classDef core    fill:#2d6a2d,stroke:#60c060,color:#fff
    classDef enemies fill:#7a2d2d,stroke:#d06060,color:#fff
    classDef world   fill:#5a4a1a,stroke:#c09030,color:#fff
    classDef ui      fill:#1a4a6a,stroke:#4090c0,color:#fff

    MMF["MainMenuForm\n(animated menu)"]:::ui

    subgraph PARTIAL["mainWin — partial class × 7 files"]
        direction TB
        MW["mainWin.cs\ngame loop · fields · input"]:::ui
        LD["LevelData\nstatic arrays"]:::ui
        LB["LevelBuilder\nbuild / clear level"]:::ui
        PH["Physics\ncollision · camera · death"]:::ui
        EU["EnemyUpdates\nspawn · update"]:::ui
        CO["Collectibles\ncoins · mushrooms"]:::ui
        HUD["HUD\nsprites · overlay"]:::ui
    end

    subgraph CORE["Core/"]
        GM["GameManager\ngame state"]:::core
        PL["Player\nphysics · health"]:::core
        GD["GameData\nMushroom · Coin\nQuestionBlock"]:::core
    end

    subgraph ENEMIES["Enemies/"]
        GOO["Goomba"]:::enemies
        KOP["Koopa"]:::enemies
        FAS["FastEnemy"]:::enemies
        JMP["JumpingEnemy"]:::enemies
        PAT["PlatformPatrolEnemy"]:::enemies
        FLY["FlyingEnemy"]:::enemies
    end

    GOS["GameObjectS\nplatform wrapper"]:::world

    MMF -->|"launches"| PARTIAL
    PARTIAL -->|"runs"| CORE
    PARTIAL -->|"spawns & updates"| ENEMIES
    PARTIAL -->|"builds levels with"| GOS
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

## Enemy Behaviour

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

- **Rendering**: Player, Goomba, Koopa, items, blocks, and background draw from `assets/textures/sprite_sheets/`; specialized variants still use GDI+ procedural fallbacks.
- **Physics**: Fixed 16 ms timestep; timer fires every 8 ms and accumulates steps.
- **Collision**: AABB overlap — resolves smallest overlap axis first.
- **Camera**: Horizontal scroll only; parallax background at 8 %, 12 %, 25 % speeds.
- **Levels**: 3 hand-crafted levels + 2 procedurally generated levels using section templates.
