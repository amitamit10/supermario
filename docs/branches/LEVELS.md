# Level Design

The game ships with **three hand-designed levels** (L1–L3) and **two procedural levels** (L4–L5) generated from a library of 25 section templates. Both branches share the same level system; the luigi branch adds its own separate flat training arena inside `TrainingForm`.

## The Three Hand-Designed Levels

```mermaid
flowchart LR
  subgraph L1["LEVEL 1 — SMB 1-1 Overworld"]
    direction TB
    L1A["Intro run<br/>(wide safe zone)"]
    L1B["3-QB row"]
    L1C["3-pipe gauntlet"]
    L1D["Raised enemy<br/>combat platform"]
    L1E["Elevated run<br/>+ high QB trio"]
    L1F["Challenge zone"]
    L1G["8-step staircase"]
    L1H["Flagpole"]
    L1A-->L1B-->L1C-->L1D-->L1E-->L1F-->L1G-->L1H
  end
```

```mermaid
flowchart LR
  subgraph L2["LEVEL 2 — Underground Cavern"]
    direction TB
    L2A["Descending entry"]
    L2B["Cavern traverse"]
    L2C["3-pipe sequence<br/>(forced height changes)"]
    L2D["Deep traverse<br/>+ enemy platform"]
    L2E["6-step exit staircase"]
    L2F["Flagpole"]
    L2A-->L2B-->L2C-->L2D-->L2E-->L2F
  end
```

```mermaid
flowchart LR
  subgraph L3["LEVEL 3 — Sky Fortress"]
    direction TB
    L3A["High-start platforms"]
    L3B["2-pipe stepping stones"]
    L3C["Sky-high traverse<br/>(Y=273)"]
    L3D["Precision narrow ledges"]
    L3E["High-reward platform"]
    L3F["8-step staircase"]
    L3G["Flagpole"]
    L3A-->L3B-->L3C-->L3D-->L3E-->L3F-->L3G
  end
```

## Coordinates & World Bounds

```mermaid
flowchart LR
  subgraph WorldStrip["World coordinate space"]
    direction LR
    Z0["X = 0<br/>spawn"]
    Z1["X = 2950<br/>X clamp"]
    Z2["X = LEVEL_PIXEL_WIDTH<br/>level end"]
    Z3["X = FLAGPOLE_X<br/>victory"]
  end
  subgraph Heights["Vertical reference (Y, top-down)"]
    direction TB
    H0["Y = 0  (top of screen)"]
    H1["Y = 115 — sky-fortress Q-blocks"]
    H2["Y = 215–275 — L1 elevated QBs"]
    H3["Y = 273 — L3 sky traverse"]
    H4["Y = 405 — old buggy spawn (fixed)"]
    H5["Y = 445 — correct spawn (513-68)"]
    H6["Y = 513 — ground surface"]
    H7["Y = 580 — pit-fall death line"]
    H8["Y = 600/620 — enemy off-world cleanup"]
  end
```

Camera: `CAMERA_MAX` prevents scrolling past the level boundary (`6f06d18`).

## Win / Lose / Restart Flow

```mermaid
stateDiagram-v2
  [*] --> Title: launch
  Title --> Playing: click START GAME
  Playing --> Playing: collect coin / stomp enemy / hit Q-block
  Playing --> LevelComplete: touch flagpole at FLAGPOLE_X
  LevelComplete --> Playing: next level
  LevelComplete --> Won: completed all 5 levels
  Won --> Playing: restart from level 1 (fixed in 8122b3f)
  Playing --> Dying: HP=0 OR pit fall (Y>580)
  Dying --> Restart: death animation finishes
  Restart --> Playing: score & coin reset (RestartLevel only)
  Restart --> Title: lives exhausted
  Playing --> Paused: ESC
  Paused --> Playing: Enter ([Enter to Resume] hint)

  note right of Dying
    isWalking forced false; picboxplayer
    invalidated each death step (1e82bb3)
  end note
```

Key fixes:
- `_levelComplete` flag prevents double-trigger (`6f06d18`).
- All 5 levels completed ⇒ restart from L1, not L5 (`8122b3f`).
- `gameTimer.Stop()` before `Start` in `DoLevelSetup` (`1e82bb3`).
- Score / coinCount only reset in `RestartLevel`, not `DoLevelSetup` (`ab0eaeb`).

## Question-Block Math

Vertical placement formula used everywhere after `e20b055`:

```
block_Y = platform_Y − player_height(68) − clearance(40) − block_height(50)
```

This positions the Q-block so its bottom edge floats **40 px** above the standing player's head — the player walks freely beneath, but a jump brings them up into it.

Applied recalculations:
| Level | Old Y | New Y | Reason |
|---|---|---|---|
| L1 row above Y=433 | 353 | **275** | Stand-clearance |
| L1 above Y=393 platform | 313 | **235** | Same |
| L1 above Y=393 run | 273 | **235** | Same |
| L1 above Y=373 challenge | 333 | **215** | Same |
| L2 | various | per-platform | Same formula applied to all 8 |
| L3 sky | various | **Y=115** above Y=273 | Multi-step climb retained |

## Procedural Sections (L4–L5)

`GenerateRandomLevel` composes the level out of section templates picked from three pools:

```mermaid
flowchart TB
  subgraph Generator["GenerateRandomLevel"]
    direction TB
    OPEN["openingPool — gentle starts"]
    MID["midPool — variety"]
    HARD["hardPool — late-game challenge"]
    Step1["Always open with a gentle section"]
    Step2["Pick 6–8 more sections (5 → 7 → 9 across L4-L5)"]
    Step3["Anti-repeat: skip if previous == picked"]
    Step4["120 px between sections (was 100)"]
  end

  OPEN --> Step1 --> Step2
  MID --> Step2
  HARD --> Step2
  Step2 --> Step3 --> Step4
```

The 25 templates (final state on master HEAD):

| # | Section | Pool |
|---|---|---|
| 1 | STAIRS | opening |
| 2 | GENTLE_HOP | opening |
| 3 | LEDGE_HOP | opening |
| 4 | WIDE_GAPS | opening |
| 5 | WAVE | opening |
| 6 | DESCEND | mid |
| 7 | BRIDGE | mid |
| 8 | ZIGZAG | mid |
| 9 | ARCH | mid |
| 10 | SUSPENDED | mid |
| 11 | VALLEY | mid |
| 12 | MULTI_LEVEL | mid |
| 13 | CASTLE | mid |
| 14 | STAIR_UP | mid |
| 15 | DOUBLE_GAP | mid |
| 16 | TRIPLE_JUMP | mid |
| 17 | PYRAMID | mid |
| 18 | LONG_RUN | mid |
| 19 | DESCENT_STAIRS | mid (a647f89) |
| 20 | BIG_GAP | mid (a647f89) |
| 21 | BATTLEMENTS | mid (a647f89) |
| 22 | STAGGER_NARROW | replaced 0dc6869 |
| 23 | SKYSCRAPER | replaced 0dc6869 |
| 24 | BOUNCY | replaced 0dc6869 |
| 25 | CLOUD_WALK | hard (a647f89) |

(Replaced sections are still defined in code but moved out of active pools.)

### Section evolution timeline

```mermaid
timeline
  title Section-template library size over time
  9bfba3d : 5 → 15 (Apr 30)
  96aa547 : 15 → 21 (May 01) — TRIPLE_JUMP, PYRAMID, LONG_RUN, STAGGER_NARROW, SKYSCRAPER, BOUNCY
  0dc6869 : 21 → 21 (May 02) — replace 3 weak with STAIR_UP, DOUBLE_GAP, GENTLE_HOP
  a647f89 : 21 → 25 (May 07) — add DESCENT_STAIRS, BIG_GAP, CLOUD_WALK, BATTLEMENTS
```

## Pipes

Added in commit `0dc6869`:
- `AddPipe()` + `DrawPipeTile()` render authentic green Mario pipes with a wide rim head and inset body with a highlight stripe.
- Side-collision in `CheckPlatformCollisions()` blocks the player horizontally when approaching from the side — pipes are real obstacles, not decoration.
- Per-level arrays (`LEVEL_1_PIPES`, `LEVEL_2_PIPES`, `LEVEL_3_PIPES`).
- Procedural levels (L4-L5) have **no pipes** by design.

## Coins

```mermaid
flowchart LR
  A["Auto-coin row<br/>(skips staircase steps W=40, H≥40)"] -->|adds rows above platforms| B[Per-level coin grid]
  C["Per-level floating coin arrays"] -->|guides player through key jumps| B
  B --> D["Coin animation timer<br/>(was questionAnimTimer)<br/>integrated into GameLoop @ 110 ms (5a8c95c)"]
```

A coin gives **+10 score** and **+1 coinCount**. Coin Q-blocks give **+50 score** and **+1 coin** on first hit.

## Training Arena (luigi branch only)

`TrainingForm` builds a *separate* flat-ish strip of 10 platforms used only for AI training:

```csharp
private static readonly (int x, int y, int w, int h)[] TRAIN_PLATFORMS = {
    (0,    450, 500, 40),  (520,  430, 240, 40), (800,  390, 180, 40),
    (1010, 450, 300, 40),  (1350, 410, 220, 40), (1610, 370, 200, 40),
    (1850, 450, 250, 40),  (2140, 430, 200, 40), (2380, 390, 200, 40),
    (2620, 450, 380, 40),
};
private static readonly Point SPAWN = new Point(30, 350);
```

```mermaid
flowchart LR
  subgraph Arena["Luigi Training Arena — flat → climb → flat"]
    P1["(0, 450)<br/>500×40"]
    P2["(520, 430)<br/>240"]
    P3["(800, 390)<br/>180"]
    P4["(1010, 450)<br/>300"]
    P5["(1350, 410)<br/>220"]
    P6["(1610, 370)<br/>200"]
    P7["(1850, 450)<br/>250"]
    P8["(2140, 430)<br/>200"]
    P9["(2380, 390)<br/>200"]
    P10["(2620, 450)<br/>380"]

    P1 --> P2 --> P3 --> P4 --> P5 --> P6 --> P7 --> P8 --> P9 --> P10
  end
```

This is an intentionally simple level — no enemies, no Q-blocks, no pipes, no coins. Just gaps to jump and platform-height differences to climb so the four agent inputs (`gapDist`, `enemyDist`, `heightDiff`, `isGrounded`) have meaningful signal.

Spawn `Y = 350` puts the agent in the air, so it falls onto the first platform on the very first tick — every Luigi starts with the same air-frame.
