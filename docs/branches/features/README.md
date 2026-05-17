# Feature Catalogue

Every feature in the repository, documented one-by-one. Scope: `master` and `feature/luigi-ml-training` only.

A feature here means "a coherent piece of player-facing or developer-facing behaviour" — so power-ups, the camera, the procedural sprite generator, and the neuroevolution loop are all features.

## Feature Map

```mermaid
mindmap
  root((SuperMario Features))
    Player
      Movement
      Jumping
      Super state
      Death
      Spawn
    Combat
      Stomp
      Enemy damage
      Shell kick
      Fall damage
    Collectibles
      Coins
      Mushrooms
      Question blocks
    World
      Platforms
      Ground strip
      Pipes
      Flagpole
      Backgrounds
    Enemies
      Goomba
      Koopa
      FastEnemy
      JumpingEnemy
      PlatformPatrolEnemy
      FlyingEnemy
    HUD & Menu
      Main menu
      How to play
      Score/Coins/Lives
      Pause hint
    Rendering
      Sprite sheets
      TextureLoader
      Procedural GDI+
      Z-stack
    Asset Pipeline
      Pixel-art generator
      Sprite-sheet generator
    Game Flow
      Title → Play
      Restart on death
      Level complete
      All-levels win
      Pause/Resume
    Performance
      Single ground strip
      One SuspendLayout
      Tick wrap
      Opaque BackColor
    Luigi AI 🌱
      TRAIN AI button
      Training form
      Population
      Neural network
      MarioAgent
      Network visualiser
      Tunable settings
```

## Pages

| Page | What it covers |
|------|----------------|
| [PLAYER.md](./PLAYER.md) | Movement, jumping, variable jump height, spawn position, death animation, super state. |
| [COMBAT.md](./COMBAT.md) | Stomp mechanics, enemy damage, super-absorbs-hit, shell-kick, fall damage. |
| [COLLECTIBLES.md](./COLLECTIBLES.md) | Coins, mushrooms, question blocks (both mushroom and coin variants), Q-block solid-physics. |
| [WORLD.md](./WORLD.md) | Platforms, the 3000 px ground strip, pipes, flagpole, hills/clouds backgrounds. |
| [HUD_AND_MENU.md](./HUD_AND_MENU.md) | Animated main menu, HOW-TO-PLAY overlay, in-game HUD (score, coins, lives), pause hint. |
| [RENDERING.md](./RENDERING.md) | Sprite-sheet integration, TextureLoader, procedural GDI+ fallback, control z-stack. |
| [ASSET_PIPELINE.md](./ASSET_PIPELINE.md) | The two Python scripts: `generate_pixelart.py` and `generate_spritesheets.py`. |
| [GAME_FLOW.md](./GAME_FLOW.md) | Title → Play → Die → Restart → Level Complete → Win → Title state machine. |
| [PERFORMANCE.md](./PERFORMANCE.md) | All the perf optimisations: SuspendLayout batching, single-strip ground, opaque BackColor, etc. |
| [LUIGI_AI.md](./LUIGI_AI.md) 🌱 | The full Luigi AI feature surface — top-level page; deep-dives in `../ml/`. |

Six enemy types each have their own profile in [../ENEMIES.md](../ENEMIES.md).

## Master-only vs Luigi-only

```mermaid
flowchart LR
  subgraph Both["✔ On both branches"]
    direction TB
    P["Player"]
    Co["Combat"]
    Cl["Collectibles"]
    W["World"]
    E["Enemies"]
    H["HUD & Menu (3 buttons)"]
    R["Rendering"]
    A["Asset Pipeline"]
    F["Game Flow"]
    Pe["Performance"]
  end
  subgraph Luigi["🌱 luigi-ml only"]
    L["TRAIN AI button + Training Form +<br/>Population + NeuralNetwork +<br/>MarioAgent + NN Visualiser"]
  end
  Both -.adds.-> Luigi
```

The luigi branch is a strict superset — everything in master still works exactly as it did.
