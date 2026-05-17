# Branch Documentation

This folder documents the active branches of the `supermario` repository.

Per the documentation request, only **`master`** and **`feature/luigi-ml-training`** (the "luigi ml" branch) are covered here. Other branches (`testing`, `texture-pack-final`, `claude/intelligent-tesla-HiZB5`, `claude/document-luigi-branches-35rZm`) are intentionally excluded.

## Files

| File | Branch | Description |
|------|--------|-------------|
| [master.md](./master.md) | `master` | Mainline branch — the stable, integrated build of the game. 82 commits. |
| [feature-luigi-ml-training.md](./feature-luigi-ml-training.md) | `feature/luigi-ml-training` | "Luigi ML" branch — adds neuroevolution-driven Luigi AI agent training on top of master. 83 commits (82 inherited from master + 1 new). |

## Branch Relationship

```
master ──●──●──●──●──●── … ──●─────────────  (82 commits, tip = d69b573)
                              \
feature/luigi-ml-training ─────●  (+1 commit, tip = 4c1bc24)
                               │
                               └── Adds supermario/ML/ neuroevolution engine
                                   and the TRAIN AI tab in the main menu.
```

The luigi-ml-training branch is a strict superset of master: every master commit is present, with one extra commit on top adding the Luigi AI training feature.

## How To Read These Docs

Each branch file contains:

1. **Overview** — what the branch is for, current tip, total commits.
2. **File layout** — what files/folders exist on the branch.
3. **Themes / phases** — commits grouped by feature area in chronological order so the evolution of the codebase is visible.
4. **Full commit log** — every commit with hash, date, author, subject, and the relevant body text. Nothing is dropped.

Hashes are abbreviated to the first 7 characters in narrative sections; the full hash is shown in the commit log.
