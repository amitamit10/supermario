# `feature/luigi-ml-training` branch (the "Luigi ML" branch)

A feature branch that adds **neuroevolution-driven Luigi AI training** on top of `master`. The player can open a new **TRAIN AI** tab from the main menu and watch up to 60 Luigi agents learn to run a training level through generational selection, crossover, and mutation.

- **Remote ref:** `refs/remotes/origin/feature/luigi-ml-training`
- **Tip commit:** `4c1bc24` — *"Add Luigi AI TRAIN tab with neuroevolution ML engine"* (2026-05-17)
- **Base / fork point:** `d69b573` (current `master` tip — see [master.md](./master.md))
- **Total commits:** 83 — every commit in `master` plus exactly one new commit on top.
- **Commits unique to this branch (vs `master`):** `4c1bc24` only.

## Relationship to `master`

```
… ─●─ 3cdb3fe ─ d69b573                     ←  master tip
                       \
                        4c1bc24             ←  feature/luigi-ml-training tip
                        (Luigi AI TRAIN tab)
```

Diff vs `master` is **+1,105 / -1** across 10 files. No master files are removed; the only modification to an existing file outside the new ML folder is `MainMenuForm.cs` (adds the 4th TRAIN AI button) and `supermario.csproj` (registers the new ML and TrainingForm sources).

```
 supermario/ML/Layer.cs                |  40 +++
 supermario/ML/MarioAgent.cs           | 195 +++++++++++++
 supermario/ML/NetParams.cs            |  20 ++
 supermario/ML/NeuralNetwork.cs        |  45 +++
 supermario/ML/NeuralNetworkControl.cs | 106 +++++++
 supermario/ML/Neuron.cs               |  60 ++++
 supermario/ML/Population.cs           |  75 +++++
 supermario/UI/MainMenuForm.cs         |  25 +-
 supermario/UI/TrainingForm.cs         | 528 ++++++++++++++++++++++++++++++++++
 supermario/supermario.csproj          |  12 +
 10 files changed, 1105 insertions(+), 1 deletion(-)
```

## File Layout (HEAD)

Identical to `master` except for the additions under `supermario/ML/` and `supermario/UI/TrainingForm.cs`. The existing standalone reference classes in `ml/c#/` (from master commit `bebc788`) remain unchanged and are still not wired into the project; the active integration is the new files in `supermario/ML/`.

```
supermario/
├── ML/                                  ← NEW namespace: supermario.ML
│   ├── NetParams.cs                     ← mutable config + shared Random + Tanh
│   ├── Neuron.cs                        ← single neuron (weights, bias, forward, mutate, crossover, clone)
│   ├── Layer.cs                         ← layer of neurons (forward, crossover, clone)
│   ├── NeuralNetwork.cs                 ← multi-layer forward, crossover, clone
│   ├── NeuralNetworkControl.cs          ← GDI+ live visualiser
│   ├── MarioAgent.cs                    ← one Luigi agent with full Mario physics + 4-input inference
│   └── Population.cs                    ← top-30 % selection + elitism + crossover + mutation
└── UI/
    ├── MainMenuForm.cs                  ← gains 4th TRAIN AI button (4 buttons total)
    └── TrainingForm.cs                  ← NEW – split-screen game canvas + dashboard
```

## The Single New Commit — `4c1bc24`

> **Subject:** Add Luigi AI TRAIN tab with neuroevolution ML engine
> **Author:** Claude `<noreply@anthropic.com>`
> **Date:** 2026-05-17 15:15:58 +0000
> **Session:** `https://claude.ai/code/session_01Umuh9sUC2XpELRSaz1Kbed`

### Body (verbatim)

```
- Add TRAIN AI button to MainMenuForm (4th button, opens TrainingForm)
- Create TrainingForm: split-screen game canvas + right dashboard panel
  - Live simulation of up to 60 Luigi agents running a training level
  - Dashboard shows generation, alive count, best score, all-time best
  - NeuralNetworkControl visualises the best agent's network live
  - Configurable settings: population size, mutation %, survive %, network shape
  - Apply settings resets and restarts training with new parameters
  - Start/Pause, Reset, and Back to Menu controls
- Add supermario/ML/ neural network and neuroevolution engine:
  - NetParams.cs  — mutable config (PopulationSize, MutationRate, SurviveRate, NetworkShape)
  - Neuron.cs     — fixed 3 bugs (bias now added in Forward, shared RNG, weights in [-1,1])
  - Layer.cs      — forward pass + crossover + clone
  - NeuralNetwork.cs  — multi-layer forward, crossover, clone
  - NeuralNetworkControl.cs — GDI+ live visualiser (activation colours + weight colours)
  - MarioAgent.cs — Luigi agent with full Mario physics, 4-input inference, stuck detection
  - Population.cs — neuroevolution: top-30% selection, crossover, elitism, mutation
```

## How the ML Engine is Wired Up

### `supermario.ML.NetParams` (configuration)

```csharp
public static readonly Random randomNum = new Random();   // shared RNG
public static int    PopulationSize = 60;
public static double MutationRate   = 0.05;
public static double SurviveRate    = 0.30;

// 4 inputs: gap distance, enemy distance, platform-height-diff, is-grounded
// 2 outputs: horizontal-dir (-1/0/+1 via tanh), jump (>0 = jump)
public static int[] NetworkShape = { 4, 6, 4, 2 };

public static double Tanh(double x) => Math.Tanh(x);
```

All four fields are mutable so `TrainingForm` can change them before pressing **Apply settings**, which then resets the population.

### `supermario.ML.Neuron`

- `Weights[]` initialised in `[-1, 1]`.
- `Bias` initialised in `[-1, 1]`.
- `Forward(inputs)`: `sum = Bias; sum += Σ inputs[i]*Weights[i]; Output = Tanh(sum)`. The commit message specifically notes three bugs that were fixed vs the reference `ml/c#/` classes from earlier on master: bias is now actually added in `Forward`, the shared `NetParams.randomNum` is used (no identical seeding in tight loops), and weights are clamped to `[-1, 1]`.
- `Mutate()`: for each weight and the bias, replace with a fresh uniform `[-1, 1]` value with probability `MutationRate`.
- `CrossOver(a, b, tilt)`: per-weight pick from `a` with probability `tilt`, otherwise from `b`. Same coin flip for bias.
- `Clone()`: copy weights and bias into a new neuron.

### `supermario.ML.Layer`

Holds `Neurons[numNeurons]`, each with `numInputs` weights. `Forward` returns the activations of all its neurons. `CrossOver` and `Clone` delegate per-neuron.

### `supermario.ML.NeuralNetwork`

- `Shape` is the integer array, e.g. `{ 4, 6, 4, 2 }`.
- `layers[0]` is intentionally `null` because the input layer has no weights — values pass straight through.
- `Forward(inputs)` walks layers 1 … N, threading activations through.
- `CrossOver(a, b, tilt)` and `Clone()` delegate per-layer.

### `supermario.ML.MarioAgent`

One Luigi agent. Holds its own `NeuralNetwork Brain` and full physics state mirroring `Player.Move`:

| Constant | Value |
|---|---|
| `Gravity` | `0.58f` |
| `JumpPower` | `-13.8f` |
| `MaxFallSpeed` | `15.5f` |
| `MoveSpeed` | `4.4f` |
| `MaxMoveSpeed` | `5.6f` |
| `GroundAccel` | `0.75f` |
| `AirAccel` | `0.42f` |
| `GroundDecel` | `0.55f` |
| `AirDecel` | `0.16f` |

The agent tracks:
- `IsAlive`, `IsGrounded`, `Position`, `HorizontalVelocity`, `VerticalVelocity`
- `Fitness` — the rightmost world-X ever reached
- `stuckTimer` / `lastX` — frames with negligible X progress (death-by-stuck)
- `preciseX, preciseY` — float positions clamped to world bounds `[0, 2950]`
- `jumpHeld` — to apply 2.4 × gravity multiplier on release (variable jump height, matches `Player`)

`Step(directionInput, jumpInput)` does the full physics tick; the 4 inputs are inferred from the world and fed to the network whose 2 outputs drive `directionInput` (sign of `tanh` → -1 / 0 / +1) and `jumpInput` (`> 0`).

### `supermario.ML.Population`

Generational neuroevolution:

```csharp
public List<MarioAgent> Agents { get; private set; }
public int Generation { get; private set; }
public int AliveCount => Agents.Count(a => a.IsAlive);
public bool AllDead  => Agents.All(a => !a.IsAlive);

public List<MarioAgent> GetBestAgents() {
    int keep = Math.Max(2, (int)(PopulationSize * SurviveRate));   // top 30 %, ≥ 2
    return Agents.OrderByDescending(a => a.Fitness).Take(keep).ToList();
}

public void CreateNewGeneration() {
    var survivors = GetBestAgents();
    var next = new List<MarioAgent>(PopulationSize);

    // Elitism: keep single best agent unchanged
    next.Add(new MarioAgent(survivors[0].Brain.Clone(), _startPos));

    while (next.Count < PopulationSize) {
        int ai = randomNum.Next(survivors.Count);
        int bi; do { bi = randomNum.Next(survivors.Count); } while (bi == ai && survivors.Count > 1);
        double tilt = randomNum.NextDouble() * 0.6 + 0.2;          // 0.2 – 0.8
        var brain = NeuralNetwork.CrossOver(survivors[ai].Brain, survivors[bi].Brain, tilt);
        MutateNetwork(brain);                                       // per-weight Mutate()
        next.Add(new MarioAgent(brain, _startPos));
    }
    Agents = next;
    Generation++;
}
```

Key properties:
- **Survivor count:** `max(2, PopulationSize * SurviveRate)` — never collapses below 2.
- **Elitism:** the best agent's brain is cloned into the next generation unchanged.
- **Two-parent crossover** with a random "tilt" toward parent A in `[0.2, 0.8]` per pair — keeps neither parent dominant.
- **Mutation:** every weight/bias of every neuron in every non-input layer rolls against `MutationRate` and is resampled `[-1, 1]` on hit.

### `supermario.ML.NeuralNetworkControl`

A GDI+ user control that visualises the best agent's network live: activation colour per neuron, weight colour per connection. Embedded in the right-side dashboard panel of `TrainingForm`.

### `supermario.UI.TrainingForm` (528 lines)

Hosts the training UI:

- **Left half:** the game canvas with the training level and up to 60 Luigi agents running it simultaneously.
- **Right half:** dashboard panel showing:
  - `Generation` counter
  - `Alive` count
  - Current generation `Best score` (best fitness)
  - `All-time best`
  - `NeuralNetworkControl` rendering the best agent's brain
  - Settings: population size, mutation %, survive %, network shape (e.g. `4,6,4,2`)
  - **Apply settings** — writes back to `NetParams`, resets the population, restarts training
  - **Start / Pause**
  - **Reset**
  - **Back to Menu**

When all agents die (`Population.AllDead`), the form calls `CreateNewGeneration()` and respawns the population at the start of the training level.

### `supermario.UI.MainMenuForm` change

The main menu gains a 4th button — **TRAIN AI** — alongside the existing **START GAME**, **HOW TO PLAY**, and **EXIT** buttons. Clicking it opens `TrainingForm`. Layout math in `MainMenuForm` is updated to lay out 4 styled flat buttons instead of 3.

### `supermario.csproj` change

12 new `<Compile>` entries: one for each of the 7 ML files, one for `TrainingForm.cs`, plus its `.Designer.cs` if applicable. No removals.

## Full Commit Log (newest first)

Only the very first commit is unique to this branch. The remaining 82 are inherited verbatim from `master` — see [master.md § Full Commit Log](./master.md#full-commit-log-newest-first).

| # | Hash | Date | Author | Subject | Source |
|---|------|------|--------|---------|--------|
| 1 | `4c1bc24` | 2026-05-17 | Claude | **Add Luigi AI TRAIN tab with neuroevolution ML engine** | **New** |
| 2 | `d69b573` | 2026-05-17 | amitamit10 | Merge pull request #20 from amitamit10/claude/intelligent-tesla-HiZB5 | from master |
| 3 | `3cdb3fe` | 2026-05-17 | Claude | fix: stability, gameplay and resource leaks across game systems | from master |
| 4 | `19ad223` | 2026-05-16 | amitamit10 | Merge pull request #18 from amitamit10/codex/refactor-movement-functionality-n412m5 | from master |
| 5 | `1ebf262` | 2026-05-16 | Claude | Resolve merge conflict with master for codex/refactor-movement-functionality-n412m5 | from master |
| 6 | `f11d533` | 2026-05-16 | amitamit10 | Merge pull request #15 from amitamit10/claude/awesome-wright-ZyJdv | from master |
| 7 | `c62e6f6` | 2026-05-16 | Claude | Resolve merge conflicts with master for claude/awesome-wright-ZyJdv | from master |
| 8 | `cc6d413` | 2026-05-16 | amitamit10 | Merge pull request #19 from amitamit10/testing | from master |
| 9 | `d0b124b` | 2026-05-16 | amitamit10 | Merge pull request #17 from amitamit10/claude/repo-recovery-stabilization-A1iDX | from master |
| 10 | `ee04ec3` | 2026-05-16 | amitamit10 | Merge pull request #16 from amitamit10/claude/awesome-wright-oEUy8 | from master |
| 11 | `34841b8` | 2026-05-16 | amitamit10 | Merge pull request #14 from amitamit10/claude/awesome-wright-BtTFz | from master |
| 12 | `1e82bb3` | 2026-05-16 | Claude | fix: stability, gameplay, and performance improvements | from master |
| 13 | `9f36fb4` | 2026-05-15 | Claude | Merge branch 'claude/awesome-wright-YJC6W': stability, UX, performance fixes | from master |
| 14 | `8122b3f` | 2026-05-15 | Claude | fix: stability, UX, and performance improvements | from master |
| 15 | `a673ae3` | 2026-05-14 | amitamit10 | fix: enemy gravity, animation and pit-fall correctness | from master |
| 16 | `c8edfbb` | 2026-05-14 | Claude | fix: enemy gravity, animation and pit-fall correctness | from master |
| 17 | `bebc788` | 2026-05-13 | amitamit10 | Add files via upload | from master |
| 18 | `cdd0ba5` | 2026-05-13 | amitamit10 | chore: add ml folder for NPC ML classes | from master |
| 19 | `b1dbdcd` | 2026-05-13 | amitamit10 | fix: match game window style to menu, eliminate desktop flash on launch | from master |
| 20 | `02849c0` | 2026-05-13 | Claude | fix: match game window style to menu and eliminate desktop flash on launch | from master |
| 21 | `7b39976` | 2026-05-13 | Claude | Merge remote-tracking branch 'origin/master' into claude/repo-recovery-stabilization-A1iDX | from master |
| 22 | `e20b055` | 2026-05-13 | amitamit10 | feat: authentic Mario-style question block physics and level design | from master |
| 23 | `f5614d3` | 2026-05-13 | Claude | feat: authentic Mario-style question block physics and level design | from master |
| 24 | `2f461f1` | 2026-05-13 | amitamit10 | fix: remove dead walk-frame code, mushroom memory leak, and facing reset | from master |
| 25 | `1686ab3` | 2026-05-13 | Claude | fix: remove dead walk-frame code, fix mushroom memory leak and facing reset | from master |
| 26 | `56866cb` | 2026-05-13 | Claude | Merge remote-tracking branch 'origin/master' into claude/repo-recovery-stabilization-A1iDX | from master |
| 27 | `2695fbe` | 2026-05-13 | amitamit10 | fix: consolidate best improvements from all branches into stable build | from master |
| 28 | `63bb7b1` | 2026-05-13 | Claude | fix: consolidate best improvements from all branches into stable build | from master |
| 29 | `95a0a36` | 2026-05-13 | Claude | Fix stability, gameplay, and collision bugs | from master |
| 30 | `6a3a336` | 2026-05-10 | amitamit10 | Tighten camera movement updates | from master |
| 31 | `67bf653` | 2026-05-10 | amitamit10 | Fix movement floor and scrolling performance | from master |
| 32 | `7f0b8d4` | 2026-05-10 | amitamit10 | Merge pull request #7 from amitamit10/codex/refactor-movement-functionality | from master |
| 33 | `ee5d8b3` | 2026-05-10 | amitamit10 | Rework player movement physics | from master |
| 34 | `2faf474` | 2026-05-10 | amit | Merge remote-tracking branch 'origin/master' into feature/codex-run | from master |
| 35 | `fa809ae` | 2026-05-10 | amitamit10 | feat: merge feature/codex-run — sprite sheets, texture integration, Mono fixes, performance | from master |
| 36 | `acaf8a3` | 2026-05-10 | amit | Codex run: apply integration based on project guide | from master |
| 37 | `d500ae6` | 2026-05-10 | amit | Start animation refinement phase | from master |
| 38 | `2e9b06e` | 2026-05-10 | amit | Generate multi-frame sprite sheets, delete old fragmented sprites, update Claude animation guide | from master |
| 39 | `ddcf56c` | 2026-05-10 | amit | Generate and integrate authentic procedural 2D pixel-art assets and update Claude integration guide | from master |
| 40 | `912e343` | 2026-05-10 | amit | Setup texture expansion branch, asset folders, and project guide | from master |
| 41 | `53171da` | 2026-05-10 | amitamit10 | Merge pull request #6 from amitamit10/claude/practical-cannon-Ei05y | from master |
| 42 | `b67a336` | 2026-05-10 | Claude | fix: 6 stability/gameplay/perf bugs | from master |
| 43 | `305e957` | 2026-05-10 | amitamit10 | fix: restore Mono compatibility for DrawHills and DrawClouds | from master |
| 44 | `8d32679` | 2026-05-10 | amitamit10 | perf: apply remaining performance and bug fixes to master | from master |
| 45 | `5a8c95c` | 2026-05-10 | amitamit10 | Port performance/bug fixes from feature branch to master | from master |
| 46 | `a647f89` | 2026-05-07 | Claude | Add 4 new deliberate section templates; widen random-level pools | from master |
| 47 | `5eded6a` | 2026-05-07 | Claude | docs: replace ASCII diagrams with Mermaid flowcharts in README | from master |
| 48 | `4ccef7e` | 2026-05-07 | Claude | Reorganize project into Core/, Enemies/, World/, UI/ folders | from master |
| 49 | `e56b5b4` | 2026-05-07 | Claude | Fix enemy direction-reversal bugs in platform collision loops | from master |
| 50 | `57bd99c` | 2026-05-07 | Claude | Merge branch 'claude/add-enemy-variety-A2sSc' | from master |
| 51 | `e03c6e1` | 2026-05-07 | Claude | Remove dead code: slim GameManager, delete orphaned gameObject.cs | from master |
| 52 | `0e5c2cd` | 2026-05-07 | amitamit10 | Merge pull request #5 from amitamit10/claude/add-enemy-variety-A2sSc | from master |
| 53 | `5ec77bf` | 2026-05-07 | Claude | Pre-playtest polish: fix spawn positions, GDI leak, reduce fly amplitude | from master |
| 54 | `1e6f566` | 2026-05-06 | amitamit10 | Merge pull request #4 from amitamit10/claude/add-enemy-variety-A2sSc | from master |
| 55 | `852af7d` | 2026-05-05 | Claude | Add three new enemy types with distinct behaviors and level placements | from master |
| 56 | `d6ac9d3` | 2026-05-03 | amitamit10 | Merge pull request #3 from amitamit10/claude/mario-level-design-upgrade-Yy0yI | from master |
| 57 | `0dc6869` | 2026-05-02 | Claude | Upgrade level design to authentic Mario-style architecture | from master |
| 58 | `b0bb8dc` | 2026-05-02 | Claude | Add polished main menu with animated GDI+ visuals and clean game flow | from master |
| 59 | `be1f398` | 2026-05-01 | Claude | Fix enemy invisibility: remove SendToBack from all spawn functions | from master |
| 60 | `8941a81` | 2026-05-01 | Claude | Merge regression fixes from claude/add-mushroom-powerup-bWlNf | from master |
| 61 | `ab0eaeb` | 2026-05-01 | Claude | Fix 8 regression bugs: crash, softlock, animation, and physics issues | from master |
| 62 | `1f9cc95` | 2026-05-01 | amitamit10 | Merge pull request #2 from amitamit10/claude/add-mushroom-powerup-bWlNf | from master |
| 63 | `96aa547` | 2026-05-01 | Claude | Add Phases 1-4: mushroom collectibles, coins, new enemies, level content | from master |
| 64 | `6f06d18` | 2026-04-30 | Claude | Merge bug fixes from cc branch + remove duplicate supermario-master folder | from master |
| 65 | `6506174` | 2026-04-30 | amitamit10 | Merge pull request #1 from amitamit10/claude/mario-level-design-upgrade-caQVR | from master |
| 66 | `9bfba3d` | 2026-04-30 | Claude | Upgrade level design with 15 section templates and redesigned levels | from master |
| 67 | `d5b87c1` | 2026-04-29 | amitamit10 | Add files via upload | from master |
| 68 | `a9dc802` | 2026-04-29 | amitamit10 | Add files via upload | from master |
| 69 | `93981fb` | 2026-04-13 | amit elgabsi | dvv | from master |
| 70 | `482715d` | 2026-04-13 | amit elgabsi | dfs | from master |
| 71 | `0c4c3a2` | 2026-02-23 | amit elgabsi | fvs | from master |
| 72 | `7782ed2` | 2026-02-23 | amit elgabsi | sss | from master |
| 73 | `29ca404` | 2026-02-16 | amit elgabsi | ghdnbgd | from master |
| 74 | `3531fc0` | 2026-02-16 | amit elgabsi | dwadawd | from master |
| 75 | `f79c509` | 2026-02-16 | amit elgabsi | ffssdf | from master |
| 76 | `ebf9fcd` | 2026-02-02 | amit elgabsi | גש''גשג | from master |
| 77 | `9a1ddcb` | 2026-02-02 | amit elgabsi | dc | from master |
| 78 | `6c36f28` | 2026-01-05 | amit elgabsi | update ident | from master |
| 79 | `2eebbde` | 2026-01-05 | amit elgabsi | fixed some stuff | from master |
| 80 | `130df6c` | 2025-12-01 | amit elgabsi | test | from master |
| 81 | `1de410f` | 2025-12-01 | amit elgabsi | improved from almost scratch# | from master |
| 82 | `ad084c9` | 2025-11-24 | User | Add project files. | from master |
| 83 | `9953c10` | 2025-11-24 | User | Add .gitattributes and .gitignore. | from master |

## At-a-Glance Summary

> **What this branch is for:** running a *visible*, in-game neuroevolution loop where 60 randomly-initialised Luigi brains compete on a training level, the best 30 % survive, the population is repopulated via two-parent crossover + per-weight mutation, and the cycle repeats with the best brain shown live in a GDI+ visualiser — all without leaving the SuperMario WinForms app.
