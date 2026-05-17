# Feature: Game Flow

The overall lifecycle of a game session — every state the application can be in and every transition between them.

## Top-Level State Machine

```mermaid
stateDiagram-v2
  [*] --> ProcessStart
  ProcessStart --> MainMenu: Program.Main → Application.Run(new MainMenuForm())

  MainMenu --> Playing: click START GAME
  MainMenu --> HowTo: click HOW TO PLAY
  MainMenu --> Training: click TRAIN AI 🌱
  MainMenu --> [*]: click EXIT

  HowTo --> MainMenu: toggle off

  Playing --> Paused: ESC (not during death)
  Paused --> Playing: Enter

  Playing --> Dying: HP==0 OR Y > 580
  Dying --> Playing: respawn (DoLevelSetup, lives > 0)
  Dying --> GameOver: lives == 0

  Playing --> LevelComplete: Position.X ≥ FLAGPOLE_X
  LevelComplete --> Playing: next level
  LevelComplete --> AllLevelsWin: completed level 5

  AllLevelsWin --> Playing: restart from level 1 (8122b3f)

  GameOver --> MainMenu: form close → MainMenuForm.Show()
  Playing --> MainMenu: form close

  Training --> MainMenu: GoBack() / ESC 🌱
```

## Entry Point

```mermaid
flowchart TB
  Main["Program.Main()<br/>Application.EnableVisualStyles()"]
  Main --> Menu["Application.Run(new MainMenuForm())"]
```

Commit `b0bb8dc` changed `Application.Run` to launch the menu instead of `mainWin` directly.

## START GAME Click Path

```mermaid
sequenceDiagram
  participant U as User click
  participant M as MainMenuForm
  participant G as mainWin

  U->>M: click START GAME
  M->>G: var game = new mainWin();
  M->>G: game.Show();  // commit b1dbdcd ordering
  M->>M: this.Hide();
  G->>G: mainWin_Load — auto-start, no MessageBox (b0bb8dc)
  G->>G: DoLevelSetup(1)
  G->>G: gameTimer.Start()
```

`mainWin_Load`'s blocking `MessageBox` was removed in commit `b0bb8dc` so the game auto-starts the moment the form opens.

## Per-Level Setup

```mermaid
flowchart TB
  S[DoLevelSetup(levelIndex)] --> Stop[gameTimer.Stop  (1e82bb3 safety)]
  Stop --> Clear[ClearPlatforms — also clears coins, mushrooms, animatedBlocks]
  Clear --> Build[CreateLongLevel(levelIndex)]
  Build --> Spawn["player.Position = GetPlayerStartPosition()<br/>facingRight = true<br/>wasGroundedLastFrame = true"]
  Spawn --> ResetTimer[gameTimer.Start]
  ResetTimer --> Flag[_levelComplete = false]
```

Important properties:
- `_levelComplete` prevents the flagpole code from triggering twice in one level.
- Score and `coinCount` are **not** reset here — they carry forward on level advance. They're only reset in `RestartLevel` (commit `ab0eaeb`).
- `gameTimer.Stop()` before `Start` (commit `1e82bb3`) ensures level transitions are always in a clean timer state.

## Death Flow

```mermaid
sequenceDiagram
  participant P as Player
  participant W as mainWin.Physics
  participant A as Death animation

  P->>W: HP-- (HitByEnemy / fall damage)
  W->>W: if HP==0 → isDying=true
  alt pit fall
    P->>W: Position.Y > 580
    W->>W: isDying = true (ab0eaeb)
  end

  loop while isDying
    W->>A: phase 1 — bounce up
    W->>A: phase 2 — fall off-screen (deathTopY = deathY - 100)
    W->>W: isWalking forced false
    W->>W: picboxplayer.Invalidate() (1e82bb3)
  end

  W->>W: lives--
  alt lives > 0
    W->>W: DoLevelSetup(currentLevel) — RestartLevel resets score
  else
    W->>W: show GAME OVER → close
  end
```

Pause is disabled during death (commit `3cdb3fe`).

## Pause Flow

```mermaid
flowchart TB
  ESC[ESC pressed]
  ESC --> Check{isDying?}
  Check -->|yes| Ignore[ignore — death continues]
  Check -->|no| Pause[gameTimer.Stop]
  Pause --> Title["Form.Title = 'PAUSED [Enter to Resume]'"]
  Title --> WaitEnter[wait for Enter]
  WaitEnter --> Resume[gameTimer.Start]
  Resume --> Reset[moveRight = moveLeft = jump = _prevJump = false]
  Reset --> Continue[continue gameplay]
```

The reset of stale key state (commit `1e82bb3`) prevents a phantom jump or step on the first frame after resume.

## Level Complete Flow

```mermaid
flowchart TB
  T[Per-tick check] --> C{Position.X ≥ FLAGPOLE_X<br/>AND NOT _levelComplete?}
  C -->|no| Continue
  C -->|yes| F[_levelComplete = true]
  F --> Show[Show "LEVEL n COMPLETE" + Score + Coins]
  Show --> Wait[wait for Enter / timer]
  Wait --> Next{levelIndex == 5?}
  Next -->|no| Advance[DoLevelSetup(levelIndex+1)]
  Next -->|yes| Win
  Win[All levels complete!]
  Win --> Restart[DoLevelSetup(1) — restart from L1]
```

## TRAIN AI Flow (luigi branch 🌱)

```mermaid
sequenceDiagram
  participant U as User
  participant M as MainMenuForm
  participant T as TrainingForm

  U->>M: click TRAIN AI
  M->>T: new TrainingForm()
  T->>T: BuildPlatforms()
  T->>T: BuildUI()
  T->>T: Shown → ResetTraining()
  T->>T: Population created, _simTimer paused
  U->>T: click ▶ START
  T->>T: _simTimer.Start (16 ms)
  loop SimTick
    T->>T: every alive agent: ComputeInputs → Think → Step → ApplyPlatformCollisions
    alt AllDead
      T->>T: CreateNewGeneration → Generation++
    end
  end
  U->>T: ESC or BACK
  T->>M: GoBack — new MainMenuForm().Show + Close
```

## Application Exit

There are three exit paths:

1. **EXIT button** on the main menu → `Application.Exit()`.
2. **Close the game window** (Alt+F4 or system close) → `FormClosing` handler stops timers cleanly (commit `6f06d18`), then the menu re-shows.
3. **GAME OVER → close** → menu re-shows.

`mainWin.Designer.Dispose()` is called on every game-window close and now correctly disposes `gameTimer` and the five HUD `Font` instances (commit `3cdb3fe`).

## See Also

- [HUD_AND_MENU.md](./HUD_AND_MENU.md) — the menu UI.
- [PLAYER.md](./PLAYER.md) — death-animation specifics.
- [LUIGI_AI.md](./LUIGI_AI.md) — the TRAIN flow in detail.
