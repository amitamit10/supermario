# Architecture

This page maps out **how the SuperMario WinForms game is structured** — on both branches. Where the luigi branch differs from master, it is called out explicitly.

## High-Level Component View

```mermaid
flowchart TB
  subgraph Process["WinForms Process"]
    direction TB
    Program["Program.cs<br/>Application.Run()"]
    Menu["MainMenuForm<br/>(animated GDI+)"]
    Game["mainWin (Form)<br/>game loop + input"]
    Train["TrainingForm<br/>🌱 luigi branch only"]
  end

  Program --> Menu
  Menu -->|START GAME| Game
  Menu -->|TRAIN AI 🌱| Train

  Game -.->|Closed| Menu
  Train -.->|Back / ESC 🌱| Menu

  classDef luigi fill:#3a7,stroke:#9f9,color:#fff
  class Train luigi
```

## Folder Layout — Final State

Both branches share this layout; the luigi branch adds `supermario/ML/` and `supermario/UI/TrainingForm.cs`.

```mermaid
flowchart LR
  subgraph Repo["Repository Root"]
    direction LR
    ROOT[".gitignore<br/>.gitattributes<br/>README.md<br/>CLAUDE_PROJECT_GUIDE.md"]
    ASSETS["assets/textures/sprite_sheets/<br/>(player, enemies, items, blocks, world_bg)"]
    GEN["generate_pixelart.py<br/>generate_spritesheets.py"]
    SLN["supermario.sln"]
    ML_REF["ml/c#/ (reference NN classes,<br/>NOT wired into project)"]

    subgraph PROJ["supermario/ (C# WinForms project)"]
      direction TB
      CORE["Core/<br/>GameData<br/>GameManager<br/>Player<br/>TextureLoader"]
      ENEMIES["Enemies/<br/>Goomba<br/>Koopa<br/>FastEnemy<br/>JumpingEnemy<br/>PlatformPatrolEnemy<br/>FlyingEnemy"]
      WORLD["World/<br/>GameObjectS<br/>(platform/pipe wrapper)"]
      UI["UI/<br/>MainMenuForm<br/>mainWin (7 partials)<br/>TrainingForm 🌱"]
      ML["ML/  🌱<br/>NetParams · Neuron · Layer<br/>NeuralNetwork<br/>NeuralNetworkControl<br/>MarioAgent · Population"]
      RES["Resources/<br/>Properties/<br/>App.config<br/>supermario.csproj"]
    end
  end

  classDef luigi fill:#1f6f3a,stroke:#9f9,color:#fff
  classDef refonly fill:#444,stroke:#888,color:#bbb,stroke-dasharray:5 5
  class UI,ML luigi
  class ML_REF refonly
```

## The `mainWin` Partial-Class Fan-Out

`mainWin` was split into 7 partial-class files in commit `4ccef7e` so that each subsystem lives in its own file but compiles into a single class.

```mermaid
classDiagram
  class mainWin {
    Form
    +PhysicsStep()
    +GameLoop()
    +OnKeyDown / OnKeyUp
  }
  class mainWin_LevelData {
    static LEVEL_1[]<br/>LEVEL_2[]<br/>LEVEL_3[]<br/>Q-block defs<br/>Enemy defs<br/>Section templates
  }
  class mainWin_LevelBuilder {
    CreateLongLevel()<br/>ClearPlatforms()<br/>DrawGroundBrick()<br/>DrawPlatformTile()<br/>AddPipe()
  }
  class mainWin_Physics {
    CheckPlatformCollisions()<br/>ResolveSmallestOverlap()<br/>ResolveQBlockOverlap()<br/>ActivateQuestionBlock()<br/>UpdateCamera()<br/>HitByEnemy()
  }
  class mainWin_EnemyUpdates {
    UpdateGoombas()<br/>UpdateKoopas()<br/>UpdateFastEnemies()<br/>UpdateJumping()<br/>UpdatePatrol()<br/>UpdateFlying()
  }
  class mainWin_Collectibles {
    UpdateCoins()<br/>UpdateMushrooms()
  }
  class mainWin_HUD {
    InitHud()<br/>UpdateHud()<br/>DrawPlayerSprite()
  }
  class mainWin_Designer {
    Designer.cs<br/>Dispose()
  }

  mainWin <|.. mainWin_LevelData : partial
  mainWin <|.. mainWin_LevelBuilder : partial
  mainWin <|.. mainWin_Physics : partial
  mainWin <|.. mainWin_EnemyUpdates : partial
  mainWin <|.. mainWin_Collectibles : partial
  mainWin <|.. mainWin_HUD : partial
  mainWin <|.. mainWin_Designer : partial
```

## Game-Loop Tick

```mermaid
sequenceDiagram
  autonumber
  participant T as gameTimer (16 ms)
  participant ML as mainWin.GameLoop
  participant PS as PhysicsStep
  participant EN as EnemyUpdates
  participant CL as Collectibles
  participant CA as UpdateCamera
  participant HD as HUD

  T->>ML: Tick
  ML->>PS: PhysicsStep()
  PS->>PS: SuspendLayout outer
  PS->>PS: player.Move(dir, jumpEdge, jumpHeld)
  PS->>PS: CheckPlatformCollisions
  PS->>EN: UpdateGoombas / Koopas / FastEnemy / Jumping / Patrol / Flying
  PS->>CL: UpdateCoins / UpdateMushrooms
  PS->>PS: ResumeLayout outer
  ML->>CA: UpdateCamera() ⇒ bool moved
  alt moved
    ML->>ML: Invalidate()
  end
  ML->>HD: UpdateHud()
```

Performance hot-spots that were optimised over time:

| Optimization | Commit | Effect |
|---|---|---|
| Single `SuspendLayout` outer pair instead of 7 inner pairs | `8122b3f` | 7×→1× layout passes/tick |
| `UpdateCamera` returns `bool`, skip `Invalidate` when still | `2695fbe` | Removes redundant full-screen repaints |
| `ScrollObjects` early-return on `scroll==0` | `2695fbe` | Skips `SuspendLayout` overhead |
| One 3000 px ground strip vs 75 brick `PictureBox`es | `2695fbe` | Major layout bottleneck removed |
| Opaque `BackColor` on tiles | `5a8c95c` | Eliminates ~75 transparent parent repaints / scroll |
| `globalTick % 168` wrap | `1e82bb3` | Prevents int overflow on long sessions (168 = LCM of animation divisors) |

## Z-Stack (Render Order)

WinForms paints by `Controls` z-order. After commit `be1f398` the order is:

```mermaid
flowchart TB
  H["HUD labels (score, coins, lives)"]
  P["Player picboxplayer"]
  E["Enemies (6 types)"]
  C["Coins"]
  B["Question blocks"]
  PL["Platforms"]
  BR["Bricks / ground"]
  G["Game form background"]

  H --> P --> E --> C --> B --> PL --> BR --> G

  classDef visible fill:#fff,stroke:#333,color:#000
  class H,P,E,C,B,PL,BR,G visible
```

Before that commit, `SendToBack()` was being called by enemy spawn functions *after* every other control had also been `SendToBack`ed, pushing enemies invisibly behind the bricks. Removing those `SendToBack` calls restored visibility; `BringToFront()` on the player keeps it on top.

## Layering / Dependencies

```mermaid
flowchart LR
  subgraph L1["Layer 1 — Engine entry"]
    Prog["Program"]
  end
  subgraph L2["Layer 2 — UI Forms"]
    Menu["MainMenuForm"]
    Game["mainWin"]
    Train["TrainingForm 🌱"]
  end
  subgraph L3["Layer 3 — Core gameplay"]
    Player["Core/Player"]
    GM["Core/GameManager"]
    GD["Core/GameData (Mushroom, Coin, QuestionBlock, PowerUpType)"]
  end
  subgraph L4["Layer 4 — Entities"]
    EN["Enemies/*"]
    GO["World/GameObjectS"]
  end
  subgraph L5["Layer 5 — Assets"]
    TL["Core/TextureLoader"]
    RES["Resources / asset PNGs"]
  end
  subgraph L6["Layer 6 — ML 🌱"]
    NP["NetParams"]
    NN["Neuron · Layer · NeuralNetwork"]
    AG["MarioAgent"]
    POP["Population"]
    NV["NeuralNetworkControl"]
  end

  Prog --> Menu
  Menu --> Game
  Menu -.-> Train
  Game --> Player & GM & GD & EN & GO & TL
  TL --> RES
  Train --> NP & NN & AG & POP & NV
  AG --> NN
  POP --> NN & AG
  NV --> NN

  classDef luigi fill:#1f6f3a,stroke:#9f9,color:#fff
  class Train,L6,NP,NN,AG,POP,NV luigi
```

Key property: the ML layer depends only on its own primitives plus a `Point`/`Rectangle` view of the world. It does **not** touch `Player`, `mainWin`, or any of the gameplay enemy classes — the agent re-implements the physics rather than driving the existing `Player` instance.

## GDI+ Drawing Pipeline

The game uses **two** rendering paths:

```mermaid
flowchart LR
  subgraph PathA["A. Picture-box sprites (most of the game)"]
    direction TB
    PB1["PictureBox + Image<br/>(player, enemies, coins,<br/>mushrooms, Q-blocks)"]
    PB2["Visual.Invalidate() in tick"]
    PB1 --> PB2
  end

  subgraph PathB["B. Procedural GDI+ (menu, fallbacks, enemy sprites)"]
    direction TB
    G1["Form.OnPaint / Control.OnPaint"]
    G2["Graphics.FillRectangle / DrawEllipse / LinearGradientBrush etc."]
    G3["using { SolidBrush / Pen / Path }<br/>(GDI handle hygiene)"]
    G1 --> G2 --> G3
  end

  PathA -.->|fallback when texture missing| PathB
```

The fallback is wired by `TextureLoader` (commit `95a0a36`): if `assets/textures/sprite_sheets/*.png` are absent or fail to load, the game silently degrades to GDI+ procedural rendering instead of crashing.

## How the Luigi Branch Plugs In

```mermaid
flowchart LR
  subgraph MainBranch["master code"]
    direction TB
    MM["MainMenuForm<br/>4 buttons after luigi 🌱"]
    MW["mainWin (gameplay form)"]
    CORE["Core / Enemies / World"]
  end

  subgraph LuigiAdditions["luigi-ml additions"]
    direction TB
    TF["TrainingForm<br/>(arena + dashboard)"]
    POP["Population<br/>(generation manager)"]
    AG["MarioAgent[]<br/>(60 Luigis)"]
    NN["NeuralNetwork<br/>(Brain per agent)"]
    NV["NeuralNetworkControl<br/>(live visualiser)"]
    NP["NetParams<br/>(config + RNG)"]
  end

  MM -.->|TRAIN AI button| TF
  TF --> POP --> AG --> NN
  TF --> NV
  AG --> NP
  NN --> NP
  NV --> NN

  classDef luigi fill:#1f6f3a,stroke:#9f9,color:#fff
  class TF,POP,AG,NN,NV,NP,LuigiAdditions luigi
```

The integration surface is intentionally tiny: one new button in `MainMenuForm`, the new `TrainingForm`, and the `supermario/ML/` namespace. Nothing in the existing gameplay loop is altered.

## Project File (`supermario.csproj`)

The csproj is a hand-edited classic-style `.csproj`. Each new `.cs` file gets a `<Compile Include="…" />` entry. Big changes:
- `4ccef7e` rewires all paths after the Core/Enemies/World/UI reorganisation.
- `b0bb8dc` adds `UI/MainMenuForm.cs`.
- `4c1bc24` 🌱 adds 12 entries (the 7 ML files + `TrainingForm.cs` + any associated designers).

## Where to Look Next

| You want to understand… | Read |
|---|---|
| The physics constants and how the agent mirrors `Player` | [PHYSICS.md](./PHYSICS.md) |
| All six enemy types | [ENEMIES.md](./ENEMIES.md) |
| Level structure, pipes, Q-blocks, procedural templates | [LEVELS.md](./LEVELS.md) |
| The full master commit history grouped into phases | [master.md](./master.md) |
| Just the Luigi-AI commit and its files | [feature-luigi-ml-training.md](./feature-luigi-ml-training.md) |
| The neuroevolution algorithm in detail | [ml/NEUROEVOLUTION.md](./ml/NEUROEVOLUTION.md) |
