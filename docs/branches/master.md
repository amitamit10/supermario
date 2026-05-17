# `master` branch

The mainline branch of the SuperMario WinForms game. This is where every feature, fix, and refactor is integrated. The HEAD of this branch is the canonical "stable build".

- **Remote ref:** `refs/remotes/origin/master`
- **Tip commit:** `d69b573` — *"Merge pull request #20 from amitamit10/claude/intelligent-tesla-HiZB5"* (2026-05-17)
- **Root commit:** `9953c10` — *"Add .gitattributes and .gitignore."* (2025-11-24)
- **Total commits:** 82
- **Primary authors:** `amitamit10`, `amit elgabsi` (early scaffolding), `Claude` / `Anthropic` (feature work and fixes)

## File Layout (HEAD)

```
.
├── .gitattributes
├── .gitignore
├── CLAUDE_PROJECT_GUIDE.md
├── README.md
├── assets/
│   └── textures/
│       └── sprite_sheets/
│           ├── blocks_sheet.png
│           ├── enemies_sheet.png
│           ├── items_sheet.png
│           ├── player_sheet.png
│           └── world_bg.png
├── generate_pixelart.py
├── generate_spritesheets.py
├── ml/
│   ├── .gitkeep
│   └── c#/                       # Standalone NN reference classes
│       ├── Layer.cs
│       ├── NetParams.cs
│       ├── NeuralNetwork.cs
│       ├── NeuralNetworkControl.cs
│       ├── Neuron.cs
│       └── Population.cs
├── supermario.sln
└── supermario/                   # Main C# WinForms project
    ├── App.config
    ├── Core/
    │   ├── GameData.cs           # Shared types: Mushroom, Coin, QuestionBlock, PowerUpType
    │   ├── GameManager.cs        # IsGameRunning / StartGame / EndGame / ResetGame
    │   ├── Player.cs             # Player physics, Bounce(), Move(), super/normal
    │   └── TextureLoader.cs      # Resilient PNG → Bitmap loader (MemoryStream)
    ├── Enemies/                  # One file per enemy type
    │   ├── FastEnemy.cs
    │   ├── FlyingEnemy.cs
    │   ├── Goomba.cs
    │   ├── JumpingEnemy.cs
    │   ├── Koopa.cs
    │   └── PlatformPatrolEnemy.cs
    ├── Program.cs                # Launches MainMenuForm
    ├── Properties/               # AssemblyInfo, Resources, Settings
    ├── Resources/                # PNGs imported via Resources.resx
    ├── UI/
    │   ├── MainMenuForm.cs       # Animated main menu, three buttons
    │   ├── mainWin.cs            # Game form: fields, ctor, game loop, input
    │   ├── mainWin.Designer.cs
    │   ├── mainWin.Collectibles.cs   # Coins + mushrooms
    │   ├── mainWin.EnemyUpdates.cs   # Spawn + update for all 6 enemy types
    │   ├── mainWin.HUD.cs            # Player sprite, HUD init/update
    │   ├── mainWin.LevelBuilder.cs   # Build/clear world, tile draw handlers
    │   ├── mainWin.LevelData.cs      # All static level/spawn arrays
    │   ├── mainWin.Physics.cs        # Collision, camera, death, level reset
    │   └── mainWin.resx
    ├── World/
    │   └── GameObjectS.cs        # Platform/pipe PictureBox wrapper
    └── supermario.csproj
```

Note: there is an `ml/c#/` reference folder with standalone NN classes, but they are **not** wired into the main project on `master`. The actively-integrated ML engine lives only on `feature/luigi-ml-training` under `supermario/ML/`.

## Themes by Phase

### 1. Initial scaffolding (Nov 2025 – Apr 2026)
Very early commits with terse messages — initial project upload, identity tweaks, experimental rewrites.

- `9953c10` Add `.gitattributes` / `.gitignore`
- `ad084c9` Add project files
- `1de410f` "improved from almost scratch#"
- `130df6c` test
- `2eebbde` fixed some stuff
- `6c36f28` update ident
- `9a1ddcb`, `ebf9fcd`, `f79c509`, `3531fc0`, `29ca404`, `7782ed2`, `0c4c3a2`, `482715d`, `93981fb` — commits with garbled / placeholder messages from the prototyping phase
- `a9dc802`, `d5b87c1` Add files via upload

### 2. First playable level pass (Apr 30 2026)
- `9bfba3d` **Upgrade level design with 15 section templates and redesigned levels** — expanded library from 5 to 15 (DESCEND, BRIDGE, ZIGZAG, ARCH, WIDE_GAPS, LEDGE_HOP, SUSPENDED, VALLEY, MULTI_LEVEL, CASTLE + 5 improved). Redesigned LEVEL_1 (17 platforms, Mario 1-1 feel) and LEVEL_2 (20 platforms with variety). Improved `GenerateRandomLevel` (opens with gentle section, 120 px spacing). Fixed spawn fall-damage bug by initialising `wasGroundedLastFrame=true`.
- `6506174` Merge PR #1 (`mario-level-design-upgrade-caQVR`)
- `6f06d18` **Merge bug fixes from cc branch + remove duplicate `supermario-master` folder** — persistent HUD via `InitHud`/`UpdateHud`, `_levelComplete` flag, `FLAGPOLE_X`/`CAMERA_MAX`/`LEVEL_PIXEL_WIDTH` constants, `player.Bounce()` on stomp, `FormClosing` handler, GDI `using` wrappers, `DoLevelSetup()` shared method, `GenerateRandomLevel` uses `do-while` instead of recursion, `wasGroundedLastFrame=true` on init/reset.

### 3. Mushroom power-up, coins, enemy variety, level upgrade (May 1 2026)
- `96aa547` **Add Phases 1-4: mushroom collectibles, coins, new enemies, level content** — question blocks eject mushrooms; mushroom has gravity, directional movement, full platform collision; `BecomeSuper` (grow to 82 px) / `BecomeNormal` shrink-on-damage. Coins scattered above every platform, spin animation via shared `questionAnimTimer`, +10 score / +1 coinCount. Coin Q-blocks +50 score / +1 coin on first hit. HUD shows SCORE (6 digit) and COINS (3 digit). Added Koopa (green turtle, shell state) and FastEnemy (red 3.2 px/frame patrol). Stomp scores: Goomba 100, Koopa 150, FastEnemy 200. 6 new section templates (TRIPLE_JUMP, PYRAMID, LONG_RUN, STAGGER_NARROW, SKYSCRAPER, BOUNCY), pool 15→21.
- `1f9cc95` Merge PR #2 (`add-mushroom-powerup-bWlNf`)
- `ab0eaeb` **Fix 8 regression bugs**: crash on coin dispose (remove from `animatedBlocks` first), pit-fall softlock detection (Y > 580 ⇒ death), `UpdateCamera` overwriting death animation, squished goomba floating 34 px (use `Visual.Height`), mushroom double-resolve jitter at platform corners (add `break`), score/coin reset only in `RestartLevel`, mushroom `VelocityX 1.8f→2f`, HUD null guard `_scoreLabel != null`.
- `8941a81` Merge regression fixes from `claude/add-mushroom-powerup-bWlNf`
- `be1f398` **Fix enemy invisibility**: remove `SendToBack` from Goomba/Koopa/FastEnemy spawn functions — previously they were pushed behind ground bricks because every preceding control had already called `SendToBack`. Render order is now: HUD > player > enemies > coins/blocks/platforms/bricks.
- `b0bb8dc` **Add polished main menu with animated GDI+ visuals and clean game flow** — `MainMenuForm` with sky-gradient, parallax mountains, drifting clouds, brick ground, animated title (drop shadow, outline, gold stars, sine bob), decorative Mario/Goomba/Q-block/coin figures, three styled flat buttons, HOW-TO-PLAY overlay, keyboard shortcuts, returns to menu when game closed. `Program.cs` launches `MainMenuForm` instead of `mainWin` directly; `mainWin_Load` blocking `MessageBox` removed.

### 4. Authentic Mario-style level architecture (May 2-3 2026)
- `0dc6869` **Upgrade level design to authentic Mario-style architecture** — LEVEL_1 SMB 1-1 overworld (intro run, 3-QB row, 3-pipe gauntlet, raised combat platform, ascending staircase to flag). LEVEL_2 underground cavern (descending entry, 3-pipe forced height changes, 6-step exit staircase). LEVEL_3 sky fortress (high-start platforms, 2-pipe stepping stones, sky-high Y=273 traverse, narrow ledges, 8-step staircase). `AddPipe` + `DrawPipeTile` for authentic green pipes with side-collision blocking. `QBlockDef`/`EnemyDef` structs with per-level arrays. Auto-coin row skips staircase steps (W=40, H≥40). 21 deliberate templates (added STAIR_UP, DOUBLE_GAP, GENTLE_HOP; replaced SKYSCRAPER/STAGGER_NARROW/BOUNCY). `GenerateRandomLevel` uses opening/mid/hard pools with anti-repeat logic; procedural levels 4-5 get 7 and 9 sections.
- `d6ac9d3` Merge PR #3 (`mario-level-design-upgrade-Yy0yI`)

### 5. Enemy variety expansion (May 5-7 2026)
- `852af7d` **Add three new enemy types with distinct behaviors and level placements** — `JumpingEnemy` (blue, leaps with -9f every 90 ticks, spring-coil feet, 150 pts). `PlatformPatrolEnemy` (orange, edge-detection AI turns at ledge ends, antenna + determined-eye, 175 pts). `FlyingEnemy` (green Parakoopa, sine-wave flight amp 28 px / freq 0.055 rad/tick; first stomp strips wings, second squishes, 200/300 pts). All have GDI+ procedural sprites, AABB platform collision, invincibility-aware damage. Hand-placed `EnemyDef` arrays for Levels 1-3; wired into `PhysicsStep`, `CreateLongLevel`, `ClearPlatforms`.
- `1e6f566` Merge PR #4 (`add-enemy-variety-A2sSc`)
- `5ec77bf` **Pre-playtest polish**: fix 6 out-of-bounds spawn positions across levels 1-3, move L1 patrol enemies to bridge/rest platforms, reduce `FLY_AMPLITUDE 28→22` to prevent clipping through platform undersides, fix GDI leak in `PlatformPatrolEnemy.DrawSprite` (antenna-tip `SolidBrush` now in `using`).
- `0e5c2cd` Merge PR #5
- `e03c6e1` **Remove dead code**: slim `GameManager` (`Score`, `CurrentLevel`, `UpdateGame`, `RestartLevel`, `LoadNextLevel` never called); delete orphaned `gameObject.cs` (referenced non-existent `Form1`).
- `57bd99c` Merge `claude/add-enemy-variety-A2sSc`
- `e56b5b4` **Fix enemy direction-reversal bugs in platform collision loops** — all 6 enemy `Update` methods were missing a `break` after `ReverseDirection()`, causing double-reversal in corners. `PlatformPatrolEnemy` also had edge-detection cancelling wall-reversal — fixed via `peWallHit` flag.
- `4ccef7e` **Reorganize project into `Core/`, `Enemies/`, `World/`, `UI/`** — split flat layout into four folders. `Enemies.cs` exploded into one file per enemy. `mainWin` split into 7 partial-class files (`.cs`, `.LevelData`, `.LevelBuilder`, `.Physics`, `.EnemyUpdates`, `.Collectibles`, `.HUD`). Deleted dead files: `Enemies.cs`, `level.cs`, `Utils.cs`. Added `README.md` with folder tree and architecture diagram. No logic changes.
- `5eded6a` **docs: replace ASCII diagrams with Mermaid flowcharts in README** — color-coded subgraph diagrams.
- `a647f89` **Add 4 new deliberate section templates** (25 total): `SECTION_DESCENT_STAIRS` (5-step mirror of `STAIR_UP`), `SECTION_BIG_GAP` (155 px pit), `SECTION_CLOUD_WALK` (5 narrow 60 px platforms), `SECTION_BATTLEMENTS` (4 equal platforms). `midPool` gets BATTLEMENTS, BIG_GAP, DESCENT_STAIRS; `hardPool` gets CLOUD_WALK.

### 6. Texture / sprite-sheet integration (May 10 2026)
- `912e343` Setup texture expansion branch, asset folders, and project guide
- `ddcf56c` Generate and integrate authentic procedural 2D pixel-art assets and update Claude integration guide
- `2e9b06e` Generate multi-frame sprite sheets, delete old fragmented sprites, update Claude animation guide
- `d500ae6` Start animation refinement phase
- `acaf8a3` Codex run: apply integration based on project guide
- `fa809ae` feat: merge feature/codex-run — sprite sheets, texture integration, Mono fixes, performance
- `2faf474` Merge origin/master into feature/codex-run (conflicts: `generate_spritesheets.py`)
- `5a8c95c` **Port performance/bug fixes from feature branch to master** — remove `questionAnimTimer`, integrate animation stepping into `GameLoop` at ~110 ms via `_animStepCount`. Raise `FALL_DAMAGE_THRESHOLD 60f→120f`. Timer `Interval 8→16 ms`; `SmoothingMode.None` in `OnPaintBackground`. Opaque `BackColor` on ground bricks / platform tiles (eliminates ~75 transparent parent repaints per scroll). `SuspendLayout`/`ResumeLayout` in `ScrollObjects`. All 6 enemy types use world-coords (`plat.Position.X`) instead of screen-coords. `break` after ground collision per enemy. Removed redundant `Visual.Invalidate()` from enemy `Update()`.
- `8d32679` perf: apply remaining performance and bug fixes to master
- `305e957` **fix: restore Mono compatibility for `DrawHills` and `DrawClouds`** — replace C# 7 tuple deconstruction syntax with explicit field access.
- `b67a336` **fix: 6 stability/gameplay/perf bugs** — Q-block won't trigger on equal-height walk-by (require upward `VerticalVelocity < 0`). Death-animation phase-2 starts from top of phase-1 arc (no 100 px snap). Edge-triggered `jumpEdge` so holding jump through landing doesn't auto-fire. Squish/shell early-out moved before gravity (no 1 px jitter). `SuspendLayout`/`ResumeLayout` in all 6 enemy updates + `UpdateCoins`. Enemy walk-speed `(int)Math.Round` instead of truncation.
- `53171da` Merge PR #6 (`claude/practical-cannon-Ei05y`)

### 7. Movement-refactor / floor / camera (May 10 2026)
- `ee5d8b3` Rework player movement physics
- `7f0b8d4` Merge PR #7 (`codex/refactor-movement-functionality`)
- `67bf653` Fix movement floor and scrolling performance
- `6a3a336` Tighten camera movement updates

### 8. Stabilization sweep (May 13 2026)
- `95a0a36` **Fix stability, gameplay, and collision bugs** — `TextureLoader` `try/catch` so missing dir falls back to procedural GDI+. Enemy stomp detection adds `VerticalVelocity >= 0` guard for all 6 enemy types. Enemy off-world cleanup at Y > 600. Dead-code removal (`walkFrame`/`walkFrameTimer`). `ResolveSmallestOverlap` ceiling-bounce no longer gated on velocity; explicit fallback for top-overlap-smallest while moving up.
- `63bb7b1`, `2695fbe` **Consolidate best improvements from all branches into stable build** — `UpdateCamera()` returns `bool`, `GameLoop` only invalidates on movement. `UpdatePlayerScreenLocation()` helper guarded against death animation. `ScrollObjects()` early-return on `scroll==0`. `CreateBrickGround` 75 `PictureBox`es → one 3000 px strip. `CheckPlatformCollisions` fast-path for ground strip via `LandOn`. `ResolveSmallestOverlap` upward-velocity-corner + unconditional ceiling bounce. `PLAYER_START_X`/`GROUND_TOP_Y` constants. `GetPlayerStartPosition()` (fixes Y=405 spawn bug, now 445 = 513-68). `TextureLoader.LoadAll()` resilient.
- `56866cb` Merge origin/master into `claude/repo-recovery-stabilization-A1iDX`
- `1686ab3`, `2f461f1` **fix: remove dead walk-frame code, mushroom memory leak, facing reset** — drop unused `walkFrame`/`walkFrameTimer` from `PhysicsStep` (`DrawPlayerSprite` already uses `globalTick`). Pit-fall cleanup for mushrooms (Y > 580). Clamp mushroom X after reversal. Reset `facingRight=true` in `DoLevelSetup`.
- `f5614d3`, `e20b055` **feat: authentic Mario-style question block physics and level design** — Q-blocks now fully solid (stand on top, jump on, blocked by sides). Activation fires ONLY when hit from below with upward velocity. Removed `headRect`-only `CheckQuestionBlockCollisions`. Added `ResolveQBlockOverlap`/`ActivateQuestionBlock`. All Q-block Y positions recalculated using `block_Y = platform_Y - player_height(68) - clearance(40) - block_height(50)` so blocks float 40 px above standing player. Level 1 row 353→275, 313→235, 273→235, 333→215; Level 2 matching per-platform; Level 3 sky-fortress 115.
- `7b39976` Merge origin/master into `claude/repo-recovery-stabilization-A1iDX`
- `02849c0`, `b1dbdcd` **fix: match game window style to menu, eliminate desktop flash on launch** — `FormBorderStyle = None`. Remove `Min`/`MaximumSize` and `WindowState=Maximized`. `StartPosition = CenterScreen`. `MainMenuForm.LaunchGame` calls `game.Show()` before `Hide()` so no desktop flash.
- `cdd0ba5` chore: add `ml/` folder for NPC ML classes (just the `.gitkeep`/scaffolding)
- `bebc788` Add files via upload (drop the `ml/c#/` reference NN classes)

### 9. Enemy gravity, animation & pit-fall correctness (May 14 2026)
- `c8edfbb`, `a673ae3` **fix: enemy gravity, animation and pit-fall correctness** — `(int)Math.Round(velocity)` instead of `(int)velocity` for enemy/mushroom gravity in all 6 update loops (truncation caused gravity < 1.0 to produce 0 movement). `Visual.Invalidate()` on walk-frame timer for FastEnemy/JumpingEnemy/FlyingEnemy/PlatformPatrolEnemy (paint never triggered while stationary). Kill enemies whose world-Y > 620 (no more infinite falls burning CPU). Move `isWalking` assignment after `CheckPlatformCollisions()` so it reads post-collision `IsGrounded`.

### 10. Stability / UX / performance pass (May 15 2026)
- `8122b3f`, `9f36fb4` **fix: stability, UX, and performance improvements** — completing all 5 levels now restarts at level 1 (was restarting level 5). PAUSED title shows `[Enter to Resume]`. Collapse 7 `SuspendLayout`/`ResumeLayout` pairs (one per enemy update + coins) into a single outer pair in `PhysicsStep` (7→1 layout passes per tick). Remove redundant `ClearPowerUps()` from `CreateLongLevel`. `GameObjectS.Bounds` corrected to world-space (was screen-space).

### 11. Tunneling / animation / pause / overflow fixes (May 16 2026)
- `1e82bb3` **fix: stability, gameplay, and performance improvements** — raise landing-overlap threshold 25→30 px for all 6 enemy types (covers 15 px/step max-fall velocity, no tunneling). Enemies reverse direction on side-hit with Q-blocks. `JumpingEnemy` ceiling detection while airborne. `animatedBlocks.Clear()` after `ClearCoins`/`ClearPowerUps` in `ClearPlatforms`. `gameTimer.Stop()` before `Start` in `DoLevelSetup`. `isWalking` forced false when `isDying`, `picboxplayer.Invalidate()` each death step. Phantom-jump-on-resume fixed: ESC resets `moveRight`/`moveLeft`/`jump`/`_prevJump`. `globalTick` wrapped at 168 (LCM of all animation divisors).
- `34841b8` Merge PR #14 (`claude/awesome-wright-BtTFz`)
- `ee04ec3` Merge PR #16 (`claude/awesome-wright-oEUy8`)
- `d0b124b` Merge PR #17 (`claude/repo-recovery-stabilization-A1iDX`)
- `cc6d413` Merge PR #19 (`testing`)
- `c62e6f6` **Resolve merge conflicts for `claude/awesome-wright-ZyJdv`** — `TextureLoader.cs` keep improved `File.Exists` check; `mainWin.Physics.cs` keep more descriptive ceiling-hit comments; `mainWin.cs` keep `isWalking` tied to `IsGrounded`.
- `f11d533` Merge PR #15 (`claude/awesome-wright-ZyJdv`)
- `1ebf262` **Resolve merge conflict for `codex/refactor-movement-functionality-n412m5`** — keep master's complete inline Q-block collision (`crossedBottom`, `ResolveQBlockOverlap`, `ActivateQuestionBlock`) over branch's incomplete `CheckQuestionBlockCollisions`. Remove duplicate `GetPlayerStartPosition`.
- `19ad223` Merge PR #18 (`codex/refactor-movement-functionality-n412m5`)

### 12. Stability / gameplay / resource leaks (May 17 2026 — current tip)
- `3cdb3fe` **fix: stability, gameplay and resource leaks across game systems** —
  - Super power-up now *absorbs* one enemy hit instead of subtracting HP and removing super in a single touch. Centralised in `HitByEnemy()` used by all 6 enemy-collision damage branches and the fall-damage path. Mushroom no longer auto-adds HP, so the absorb is the actual benefit at full health.
  - `BecomeSuper`/`BecomeNormal` use real 14 px height delta instead of hard-coded 16 — no more 2 px ground embed when shrinking / float when growing.
  - Walking sideways into a dormant Koopa shell now kicks the shell (50 pts) instead of damaging the player.
  - Fall-damage threshold 120→220 px so intended platform drops stop costing a heart.
  - Escape no longer triggers pause during death animation.
  - `mainWin.Designer.Dispose` now disposes `gameTimer` and the five HUD `Font`s — fixes GDI-handle leak per restart.
  - `TextureLoader` reads PNGs through a `MemoryStream` so assets are no longer locked for process lifetime.
  - `DrawPlayerSprite` caches fallback bitmap in a static field instead of hitting `Properties.Resources` every paint event.
- `d69b573` Merge PR #20 (`claude/intelligent-tesla-HiZB5`) — current tip.

## Full Commit Log (newest first)

| # | Hash | Date | Author | Subject |
|---|------|------|--------|---------|
| 1 | `d69b573` | 2026-05-17 | amitamit10 | Merge pull request #20 from amitamit10/claude/intelligent-tesla-HiZB5 |
| 2 | `3cdb3fe` | 2026-05-17 | Claude | fix: stability, gameplay and resource leaks across game systems |
| 3 | `19ad223` | 2026-05-16 | amitamit10 | Merge pull request #18 from amitamit10/codex/refactor-movement-functionality-n412m5 |
| 4 | `1ebf262` | 2026-05-16 | Claude | Resolve merge conflict with master for codex/refactor-movement-functionality-n412m5 |
| 5 | `f11d533` | 2026-05-16 | amitamit10 | Merge pull request #15 from amitamit10/claude/awesome-wright-ZyJdv |
| 6 | `c62e6f6` | 2026-05-16 | Claude | Resolve merge conflicts with master for claude/awesome-wright-ZyJdv |
| 7 | `cc6d413` | 2026-05-16 | amitamit10 | Merge pull request #19 from amitamit10/testing |
| 8 | `d0b124b` | 2026-05-16 | amitamit10 | Merge pull request #17 from amitamit10/claude/repo-recovery-stabilization-A1iDX |
| 9 | `ee04ec3` | 2026-05-16 | amitamit10 | Merge pull request #16 from amitamit10/claude/awesome-wright-oEUy8 |
| 10 | `34841b8` | 2026-05-16 | amitamit10 | Merge pull request #14 from amitamit10/claude/awesome-wright-BtTFz |
| 11 | `1e82bb3` | 2026-05-16 | Claude | fix: stability, gameplay, and performance improvements |
| 12 | `9f36fb4` | 2026-05-15 | Claude | Merge branch 'claude/awesome-wright-YJC6W': stability, UX, performance fixes |
| 13 | `8122b3f` | 2026-05-15 | Claude | fix: stability, UX, and performance improvements |
| 14 | `a673ae3` | 2026-05-14 | amitamit10 | fix: enemy gravity, animation and pit-fall correctness |
| 15 | `c8edfbb` | 2026-05-14 | Claude | fix: enemy gravity, animation and pit-fall correctness |
| 16 | `bebc788` | 2026-05-13 | amitamit10 | Add files via upload |
| 17 | `cdd0ba5` | 2026-05-13 | amitamit10 | chore: add ml folder for NPC ML classes |
| 18 | `b1dbdcd` | 2026-05-13 | amitamit10 | fix: match game window style to menu, eliminate desktop flash on launch |
| 19 | `02849c0` | 2026-05-13 | Claude | fix: match game window style to menu and eliminate desktop flash on launch |
| 20 | `7b39976` | 2026-05-13 | Claude | Merge remote-tracking branch 'origin/master' into claude/repo-recovery-stabilization-A1iDX |
| 21 | `e20b055` | 2026-05-13 | amitamit10 | feat: authentic Mario-style question block physics and level design |
| 22 | `f5614d3` | 2026-05-13 | Claude | feat: authentic Mario-style question block physics and level design |
| 23 | `2f461f1` | 2026-05-13 | amitamit10 | fix: remove dead walk-frame code, mushroom memory leak, and facing reset |
| 24 | `1686ab3` | 2026-05-13 | Claude | fix: remove dead walk-frame code, fix mushroom memory leak and facing reset |
| 25 | `56866cb` | 2026-05-13 | Claude | Merge remote-tracking branch 'origin/master' into claude/repo-recovery-stabilization-A1iDX |
| 26 | `2695fbe` | 2026-05-13 | amitamit10 | fix: consolidate best improvements from all branches into stable build |
| 27 | `63bb7b1` | 2026-05-13 | Claude | fix: consolidate best improvements from all branches into stable build |
| 28 | `95a0a36` | 2026-05-13 | Claude | Fix stability, gameplay, and collision bugs |
| 29 | `6a3a336` | 2026-05-10 | amitamit10 | Tighten camera movement updates |
| 30 | `67bf653` | 2026-05-10 | amitamit10 | Fix movement floor and scrolling performance |
| 31 | `7f0b8d4` | 2026-05-10 | amitamit10 | Merge pull request #7 from amitamit10/codex/refactor-movement-functionality |
| 32 | `ee5d8b3` | 2026-05-10 | amitamit10 | Rework player movement physics |
| 33 | `2faf474` | 2026-05-10 | amit | Merge remote-tracking branch 'origin/master' into feature/codex-run |
| 34 | `fa809ae` | 2026-05-10 | amitamit10 | feat: merge feature/codex-run — sprite sheets, texture integration, Mono fixes, performance |
| 35 | `acaf8a3` | 2026-05-10 | amit | Codex run: apply integration based on project guide |
| 36 | `d500ae6` | 2026-05-10 | amit | Start animation refinement phase |
| 37 | `2e9b06e` | 2026-05-10 | amit | Generate multi-frame sprite sheets, delete old fragmented sprites, update Claude animation guide |
| 38 | `ddcf56c` | 2026-05-10 | amit | Generate and integrate authentic procedural 2D pixel-art assets and update Claude integration guide |
| 39 | `912e343` | 2026-05-10 | amit | Setup texture expansion branch, asset folders, and project guide |
| 40 | `53171da` | 2026-05-10 | amitamit10 | Merge pull request #6 from amitamit10/claude/practical-cannon-Ei05y |
| 41 | `b67a336` | 2026-05-10 | Claude | fix: 6 stability/gameplay/perf bugs |
| 42 | `305e957` | 2026-05-10 | amitamit10 | fix: restore Mono compatibility for DrawHills and DrawClouds |
| 43 | `8d32679` | 2026-05-10 | amitamit10 | perf: apply remaining performance and bug fixes to master |
| 44 | `5a8c95c` | 2026-05-10 | amitamit10 | Port performance/bug fixes from feature branch to master |
| 45 | `a647f89` | 2026-05-07 | Claude | Add 4 new deliberate section templates; widen random-level pools |
| 46 | `5eded6a` | 2026-05-07 | Claude | docs: replace ASCII diagrams with Mermaid flowcharts in README |
| 47 | `4ccef7e` | 2026-05-07 | Claude | Reorganize project into Core/, Enemies/, World/, UI/ folders |
| 48 | `e56b5b4` | 2026-05-07 | Claude | Fix enemy direction-reversal bugs in platform collision loops |
| 49 | `57bd99c` | 2026-05-07 | Claude | Merge branch 'claude/add-enemy-variety-A2sSc' |
| 50 | `e03c6e1` | 2026-05-07 | Claude | Remove dead code: slim GameManager, delete orphaned gameObject.cs |
| 51 | `0e5c2cd` | 2026-05-07 | amitamit10 | Merge pull request #5 from amitamit10/claude/add-enemy-variety-A2sSc |
| 52 | `5ec77bf` | 2026-05-07 | Claude | Pre-playtest polish: fix spawn positions, GDI leak, reduce fly amplitude |
| 53 | `1e6f566` | 2026-05-06 | amitamit10 | Merge pull request #4 from amitamit10/claude/add-enemy-variety-A2sSc |
| 54 | `852af7d` | 2026-05-05 | Claude | Add three new enemy types with distinct behaviors and level placements |
| 55 | `d6ac9d3` | 2026-05-03 | amitamit10 | Merge pull request #3 from amitamit10/claude/mario-level-design-upgrade-Yy0yI |
| 56 | `0dc6869` | 2026-05-02 | Claude | Upgrade level design to authentic Mario-style architecture |
| 57 | `b0bb8dc` | 2026-05-02 | Claude | Add polished main menu with animated GDI+ visuals and clean game flow |
| 58 | `be1f398` | 2026-05-01 | Claude | Fix enemy invisibility: remove SendToBack from all spawn functions |
| 59 | `8941a81` | 2026-05-01 | Claude | Merge regression fixes from claude/add-mushroom-powerup-bWlNf |
| 60 | `ab0eaeb` | 2026-05-01 | Claude | Fix 8 regression bugs: crash, softlock, animation, and physics issues |
| 61 | `1f9cc95` | 2026-05-01 | amitamit10 | Merge pull request #2 from amitamit10/claude/add-mushroom-powerup-bWlNf |
| 62 | `96aa547` | 2026-05-01 | Claude | Add Phases 1-4: mushroom collectibles, coins, new enemies, level content |
| 63 | `6f06d18` | 2026-04-30 | Claude | Merge bug fixes from cc branch + remove duplicate supermario-master folder |
| 64 | `6506174` | 2026-04-30 | amitamit10 | Merge pull request #1 from amitamit10/claude/mario-level-design-upgrade-caQVR |
| 65 | `9bfba3d` | 2026-04-30 | Claude | Upgrade level design with 15 section templates and redesigned levels |
| 66 | `d5b87c1` | 2026-04-29 | amitamit10 | Add files via upload |
| 67 | `a9dc802` | 2026-04-29 | amitamit10 | Add files via upload |
| 68 | `93981fb` | 2026-04-13 | amit elgabsi | dvv |
| 69 | `482715d` | 2026-04-13 | amit elgabsi | dfs |
| 70 | `0c4c3a2` | 2026-02-23 | amit elgabsi | fvs |
| 71 | `7782ed2` | 2026-02-23 | amit elgabsi | sss |
| 72 | `29ca404` | 2026-02-16 | amit elgabsi | ghdnbgd |
| 73 | `3531fc0` | 2026-02-16 | amit elgabsi | dwadawd |
| 74 | `f79c509` | 2026-02-16 | amit elgabsi | ffssdf |
| 75 | `ebf9fcd` | 2026-02-02 | amit elgabsi | גש''גשג |
| 76 | `9a1ddcb` | 2026-02-02 | amit elgabsi | dc |
| 77 | `6c36f28` | 2026-01-05 | amit elgabsi | update ident |
| 78 | `2eebbde` | 2026-01-05 | amit elgabsi | fixed some stuff |
| 79 | `130df6c` | 2025-12-01 | amit elgabsi | test |
| 80 | `1de410f` | 2025-12-01 | amit elgabsi | improved from almost scratch# |
| 81 | `ad084c9` | 2025-11-24 | User | Add project files. |
| 82 | `9953c10` | 2025-11-24 | User | Add .gitattributes and .gitignore. |

## Notable Cross-Cutting Patterns

- **Co-authoring style:** Most of the substantive code commits are authored by `Claude <noreply@anthropic.com>` and squashed by `amitamit10` via PR merge. Squash commits often carry the same body as their original branch commit.
- **Session IDs in messages:** Many commit bodies end in a `https://claude.ai/code/session_…` link tying the commit to the Claude Code session that produced it. The recurring session id `01MLC27ZJx64jPUL8e4nywyP` was used for the multi-PR stabilization period in late April / early May 2026.
- **Performance themes:** A recurring pattern is reducing redundant `Invalidate`, batching `SuspendLayout`/`ResumeLayout`, using opaque `BackColor` to skip transparent parent repaints, and replacing many small `PictureBox`es with a single wider strip (notably ground bricks: 75 → 1).
- **Coordinate-space drift:** Multiple fixes target enemy collisions accidentally using screen-space coords (`PictureBox.Left + cameraX`) vs. world-space (`Position.X`). Final state: collisions are world-space everywhere.
