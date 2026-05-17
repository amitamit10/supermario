# Feature: World

The non-character pieces of the world: platforms, the ground, pipes, the flagpole, and the parallax background.

## Platforms

```mermaid
classDiagram
  class GameObjectS {
    +PictureBox PictureBox
    +Point Position
    +Rectangle Bounds (world-space, fixed in 9f36fb4)
    +string Type ("ground", "platform", "pipe")
  }
```

A `GameObjectS` wraps a `PictureBox` together with a logical world-space position and a `Type` discriminator. Collision code branches on `Type`:

- `"ground"` — single 3000 px-wide strip (commit `2695fbe`). Fast-path: always calls `LandOn` directly when overlapping (no need to compute four overlaps).
- `"platform"` — standard solid platform tile.
- `"pipe"` — solid platform with custom side-collision draw rules (commit `0dc6869`).

`Bounds` returns **world-space** rectangle (commit `9f36fb4`); previously returned `PictureBox.Bounds` which was screen-space and broke camera-aware collision checks.

## Ground

```mermaid
flowchart LR
  Before["Before 2695fbe<br/>75 PictureBoxes,<br/>each 40 px wide,<br/>laid end-to-end"] -- replaced --> After["After 2695fbe<br/>ONE PictureBox,<br/>3000 px wide,<br/>type='ground'"]
```

- Eliminates the dominant layout bottleneck on level load.
- Brick texture is drawn into the PictureBox bitmap once.
- Opaque `BackColor` (commit `5a8c95c`) skips transparent parent repaints.

## Pipes (Authentic Green Mario Pipes)

Added in commit `0dc6869` with `AddPipe()` and `DrawPipeTile()`.

```mermaid
flowchart TB
  PR["Pipe rim — wide, square cap (top)"]
  PB["Pipe body — narrower, inset"]
  HL["Highlight stripe — vertical line<br/>1/4 from left edge for shading"]
  PR --> PB
  PB --> HL
```

Side-collision via `CheckPlatformCollisions()` blocks the player horizontally when approaching from the side; pipes are real obstacles.

Per-level pipe arrays: `LEVEL_1_PIPES`, `LEVEL_2_PIPES`, `LEVEL_3_PIPES`. Procedural levels (L4-L5) have **no pipes**.

## Flagpole / Level Complete

```mermaid
sequenceDiagram
  participant P as Player
  participant W as mainWin
  participant LC as LevelComplete

  P->>W: Position.X ≥ FLAGPOLE_X
  W->>W: _levelComplete = true (6f06d18 — no double trigger)
  W->>LC: show level-complete screen
  LC->>LC: next level OR all-levels win
  LC->>W: DoLevelSetup(nextLevelIndex)
```

`FLAGPOLE_X`, `CAMERA_MAX`, and `LEVEL_PIXEL_WIDTH` are level-wide constants introduced in commit `6f06d18`.

## All-Levels Win

After completing all 5 levels the game now restarts from **Level 1** (commit `8122b3f`). Previously it tried to restart the current level (which was Level 5), so the player got stuck on the last level forever.

## Backgrounds

`MainMenuForm` and the game form both draw a parallax-style background:

```mermaid
flowchart TB
  Sky["Sky gradient — top to bottom<br/>LinearGradientBrush"]
  Mt["Parallax mountains — distant"]
  Cl["Animated clouds — drift with _cloudOffset"]
  Hi["Green hills — DrawHills()"]
  GR["Brick ground (game) or<br/>menu brick row (menu)"]

  Sky --> Mt --> Cl --> Hi --> GR
```

`DrawHills` and `DrawClouds` use plain field access (no C# 7 tuple deconstruction) so the code compiles under Mono/xbuild as well as .NET Framework (commit `305e957`).

## World Bounds

| Bound | Value | Notes |
|---|---|---|
| World X min | `0` | clamp on `preciseX` |
| World X max | `2950` | clamp on `preciseX` (player and agent) |
| `LEVEL_PIXEL_WIDTH` | per-level | logical level length |
| `CAMERA_MAX` | derived | prevents scrolling past level boundary |
| Ground Y | `513` | top surface of ground strip |
| Pit-fall death Y | `580` | player Y > 580 ⇒ instant death |
| Enemy off-world Y | `600` (95a0a36), then `620` (c8edfbb) | enemy is killed and removed |

## Level Data

```mermaid
flowchart LR
  LD["mainWin.LevelData.cs<br/>(static arrays)"]
  LD --> L1[LEVEL_1 platforms]
  LD --> L2[LEVEL_2 platforms]
  LD --> L3[LEVEL_3 platforms]
  LD --> QB[LEVEL_n_QBLOCKS arrays]
  LD --> EN[LEVEL_n_ENEMIES arrays]
  LD --> PI[LEVEL_n_PIPES arrays]
  LD --> CO[LEVEL_n_COINS arrays]
  LD --> ST[SECTION_* templates]
```

Section templates (25 total) for procedural levels L4-L5 live here too; see [LEVELS.md](../LEVELS.md) for the full catalogue.

## See Also

- [LEVELS.md](../LEVELS.md) — the three hand-designed levels, section templates, Q-block math.
- [RENDERING.md](./RENDERING.md) — how all of this is drawn.
- [ARCHITECTURE.md](../ARCHITECTURE.md#zstack-render-order) — the control z-stack.
