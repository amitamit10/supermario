# Changelog

Flat one-line-per-commit reference for both branches. Newest first. For richer narrative grouped by theme see [master.md](./master.md), [feature-luigi-ml-training.md](./feature-luigi-ml-training.md), and [TIMELINE.md](./TIMELINE.md).

## Legend

- 🌱 = commit only on `feature/luigi-ml-training`.
- 🔀 = merge commit / squash from a PR.
- ⚙️ = pure refactor / no logic change.
- 🐛 = bug fix.
- ✨ = new feature.
- 🚀 = performance.
- 🧹 = cleanup / dead-code removal.
- 📝 = docs.
- 🎨 = art / assets.
- 🛠 = config / build.

## Commit Log

| Date | Hash | Author | Tags | Subject |
|---|---|---|---|---|
| 2026-05-17 | `4c1bc24` | Claude | 🌱 ✨ | Add Luigi AI TRAIN tab with neuroevolution ML engine |
| 2026-05-17 | `d69b573` | amitamit10 | 🔀 | Merge PR #20 (claude/intelligent-tesla-HiZB5) |
| 2026-05-17 | `3cdb3fe` | Claude | 🐛 🧹 | fix: stability, gameplay and resource leaks across game systems |
| 2026-05-16 | `19ad223` | amitamit10 | 🔀 | Merge PR #18 (codex/refactor-movement-functionality-n412m5) |
| 2026-05-16 | `1ebf262` | Claude | 🔀 | Resolve merge conflict for codex/refactor-movement-functionality-n412m5 |
| 2026-05-16 | `f11d533` | amitamit10 | 🔀 | Merge PR #15 (claude/awesome-wright-ZyJdv) |
| 2026-05-16 | `c62e6f6` | Claude | 🔀 | Resolve merge conflicts for claude/awesome-wright-ZyJdv |
| 2026-05-16 | `cc6d413` | amitamit10 | 🔀 | Merge PR #19 (testing) |
| 2026-05-16 | `d0b124b` | amitamit10 | 🔀 | Merge PR #17 (claude/repo-recovery-stabilization-A1iDX) |
| 2026-05-16 | `ee04ec3` | amitamit10 | 🔀 | Merge PR #16 (claude/awesome-wright-oEUy8) |
| 2026-05-16 | `34841b8` | amitamit10 | 🔀 | Merge PR #14 (claude/awesome-wright-BtTFz) |
| 2026-05-16 | `1e82bb3` | Claude | 🐛 🚀 | fix: stability, gameplay, performance — enemy tunneling fix, Q-block side-collision, JumpingEnemy ceiling, gameTimer Stop-before-Start, phantom-jump-on-resume, globalTick wrap at 168 |
| 2026-05-15 | `9f36fb4` | Claude | 🔀 | Merge claude/awesome-wright-YJC6W |
| 2026-05-15 | `8122b3f` | Claude | 🐛 🚀 | fix: stability, UX, performance — all-levels restart at L1, PAUSED hint, 7→1 SuspendLayout, GameObjectS.Bounds world-space |
| 2026-05-14 | `a673ae3` | amitamit10 | 🐛 | fix: enemy gravity, animation, pit-fall correctness (squash) |
| 2026-05-14 | `c8edfbb` | Claude | 🐛 | fix: enemy gravity (Math.Round), Visual.Invalidate on walk-frame timer, Y>620 cleanup, isWalking after collisions |
| 2026-05-13 | `bebc788` | amitamit10 | 🎨 | Add files via upload |
| 2026-05-13 | `cdd0ba5` | amitamit10 | 🛠 | chore: add ml/ folder for NPC ML classes |
| 2026-05-13 | `b1dbdcd` | amitamit10 | 🐛 | fix: match game window style to menu, eliminate desktop flash on launch (squash) |
| 2026-05-13 | `02849c0` | Claude | 🐛 | fix: borderless game window, MaximumSize removed, CenterScreen, game.Show before menu.Hide |
| 2026-05-13 | `7b39976` | Claude | 🔀 | Merge origin/master into claude/repo-recovery-stabilization-A1iDX |
| 2026-05-13 | `e20b055` | amitamit10 | ✨ | feat: authentic Mario-style Q-block physics and level design (squash) |
| 2026-05-13 | `f5614d3` | Claude | ✨ | feat: solid Q-blocks, ResolveQBlockOverlap, ActivateQuestionBlock, Y formula recalc all 3 levels |
| 2026-05-13 | `2f461f1` | amitamit10 | 🐛 🧹 | fix: remove dead walk-frame code, mushroom memory leak, facing reset |
| 2026-05-13 | `1686ab3` | Claude | 🐛 🧹 | same — Claude origin |
| 2026-05-13 | `56866cb` | Claude | 🔀 | Merge origin/master into claude/repo-recovery-stabilization-A1iDX |
| 2026-05-13 | `2695fbe` | amitamit10 | 🐛 🚀 | fix: consolidate best improvements — UpdateCamera bool, 75→1 ground strip, spawn Y fix |
| 2026-05-13 | `63bb7b1` | Claude | 🐛 🚀 | same — Claude origin |
| 2026-05-13 | `95a0a36` | Claude | 🐛 | TextureLoader try/catch, stomp VY guard, ResolveSmallestOverlap ceiling fixes |
| 2026-05-10 | `6a3a336` | amitamit10 | 🐛 | Tighten camera movement updates |
| 2026-05-10 | `67bf653` | amitamit10 | 🐛 🚀 | Fix movement floor and scrolling performance |
| 2026-05-10 | `7f0b8d4` | amitamit10 | 🔀 | Merge PR #7 (codex/refactor-movement-functionality) |
| 2026-05-10 | `ee5d8b3` | amitamit10 | ⚙️ | Rework player movement physics |
| 2026-05-10 | `2faf474` | amit | 🔀 | Merge origin/master into feature/codex-run (generate_spritesheets.py conflict) |
| 2026-05-10 | `fa809ae` | amitamit10 | ✨ 🎨 | feat: merge feature/codex-run — sprite sheets, texture integration, Mono fixes, perf |
| 2026-05-10 | `acaf8a3` | amit | 🎨 | Codex run: apply integration based on project guide |
| 2026-05-10 | `d500ae6` | amit | 🎨 | Start animation refinement phase |
| 2026-05-10 | `2e9b06e` | amit | 🎨 | Generate multi-frame sprite sheets, delete fragmented sprites |
| 2026-05-10 | `ddcf56c` | amit | 🎨 | Generate procedural 2D pixel-art assets |
| 2026-05-10 | `912e343` | amit | 🛠 | Setup texture-expansion branch, asset folders, project guide |
| 2026-05-10 | `53171da` | amitamit10 | 🔀 | Merge PR #6 (claude/practical-cannon-Ei05y) |
| 2026-05-10 | `b67a336` | Claude | 🐛 🚀 | fix: 6 stability/gameplay/perf bugs — Q-block VY guard, death animation arc, jumpEdge, squish before gravity |
| 2026-05-10 | `305e957` | amitamit10 | 🐛 | fix: restore Mono compat — no C# 7 tuple deconstruction |
| 2026-05-10 | `8d32679` | amitamit10 | 🚀 | perf: apply remaining performance and bug fixes to master |
| 2026-05-10 | `5a8c95c` | amitamit10 | 🚀 | Port perf/bug fixes — Interval 8→16, opaque BackColor, world-coords for enemies |
| 2026-05-07 | `a647f89` | Claude | ✨ | Add 4 deliberate sections (DESCENT_STAIRS, BIG_GAP, CLOUD_WALK, BATTLEMENTS) — 25 total |
| 2026-05-07 | `5eded6a` | Claude | 📝 | docs: replace ASCII diagrams with Mermaid flowcharts in README |
| 2026-05-07 | `4ccef7e` | Claude | ⚙️ | Reorganize project into Core/, Enemies/, World/, UI/ folders |
| 2026-05-07 | `e56b5b4` | Claude | 🐛 | Fix enemy direction-reversal bugs (missing break in 6 enemy types) |
| 2026-05-07 | `57bd99c` | Claude | 🔀 | Merge claude/add-enemy-variety-A2sSc |
| 2026-05-07 | `e03c6e1` | Claude | 🧹 | Remove dead code: slim GameManager, delete gameObject.cs |
| 2026-05-07 | `0e5c2cd` | amitamit10 | 🔀 | Merge PR #5 |
| 2026-05-07 | `5ec77bf` | Claude | 🐛 | Pre-playtest polish — 6 spawn fixes, GDI leak, FLY_AMPLITUDE 28→22 |
| 2026-05-06 | `1e6f566` | amitamit10 | 🔀 | Merge PR #4 |
| 2026-05-05 | `852af7d` | Claude | ✨ | Add 3 new enemy types (JumpingEnemy, PlatformPatrolEnemy, FlyingEnemy) |
| 2026-05-03 | `d6ac9d3` | amitamit10 | 🔀 | Merge PR #3 |
| 2026-05-02 | `0dc6869` | Claude | ✨ | Upgrade level design to authentic Mario architecture — L1 overworld, L2 underground, L3 sky fortress, pipes, per-level defs, 21 templates |
| 2026-05-02 | `b0bb8dc` | Claude | ✨ | Add polished main menu with animated GDI+ visuals |
| 2026-05-01 | `be1f398` | Claude | 🐛 | Fix enemy invisibility — remove SendToBack |
| 2026-05-01 | `8941a81` | Claude | 🔀 | Merge regression fixes from claude/add-mushroom-powerup-bWlNf |
| 2026-05-01 | `ab0eaeb` | Claude | 🐛 | Fix 8 regression bugs — crash, softlock, animation, physics |
| 2026-05-01 | `1f9cc95` | amitamit10 | 🔀 | Merge PR #2 |
| 2026-05-01 | `96aa547` | Claude | ✨ | Add Phases 1-4: mushroom collectibles, coins, new enemies, level content |
| 2026-04-30 | `6f06d18` | Claude | 🐛 🔀 | Merge bug fixes from cc branch + remove duplicate supermario-master folder |
| 2026-04-30 | `6506174` | amitamit10 | 🔀 | Merge PR #1 |
| 2026-04-30 | `9bfba3d` | Claude | ✨ | Upgrade level design with 15 section templates and redesigned levels |
| 2026-04-29 | `d5b87c1` | amitamit10 | 🎨 | Add files via upload |
| 2026-04-29 | `a9dc802` | amitamit10 | 🎨 | Add files via upload |
| 2026-04-13 | `93981fb` | amit elgabsi | | dvv |
| 2026-04-13 | `482715d` | amit elgabsi | | dfs |
| 2026-02-23 | `0c4c3a2` | amit elgabsi | | fvs |
| 2026-02-23 | `7782ed2` | amit elgabsi | | sss |
| 2026-02-16 | `29ca404` | amit elgabsi | | ghdnbgd |
| 2026-02-16 | `3531fc0` | amit elgabsi | | dwadawd |
| 2026-02-16 | `f79c509` | amit elgabsi | | ffssdf |
| 2026-02-02 | `ebf9fcd` | amit elgabsi | | גש''גשג |
| 2026-02-02 | `9a1ddcb` | amit elgabsi | | dc |
| 2026-01-05 | `6c36f28` | amit elgabsi | | update ident |
| 2026-01-05 | `2eebbde` | amit elgabsi | 🐛 | fixed some stuff |
| 2025-12-01 | `130df6c` | amit elgabsi | | test |
| 2025-12-01 | `1de410f` | amit elgabsi | ⚙️ | improved from almost scratch# |
| 2025-11-24 | `ad084c9` | User | 🛠 | Add project files |
| 2025-11-24 | `9953c10` | User | 🛠 | Add .gitattributes and .gitignore |

## Summary

- **Total commits on master:** 82
- **Total commits on feature/luigi-ml-training:** 83 (= 82 master + 1 unique)
- **Unique luigi commit:** `4c1bc24`
- **First commit date:** 2025-11-24
- **Latest commit date:** 2026-05-17

## Where to Read Detail

- For every commit's full body see [master.md § Full Commit Log](./master.md#full-commit-log-newest-first) and [feature-luigi-ml-training.md § Full Commit Log](./feature-luigi-ml-training.md#full-commit-log-newest-first).
- For narrative grouping by theme see [master.md § Themes by Phase](./master.md#themes-by-phase).
- For Gantt-style time view see [TIMELINE.md](./TIMELINE.md).
