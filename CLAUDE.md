# SpaceRunner

2D arcade vertical-scroller shoot 'em up inspired by Tyrian 2000. The player flies a ship through a corridor, dodges and shoots meteorites of multiple sizes, and collects power-ups. Mouse-only controls. **Learning project** — focus is practicing Unity development, not commercial release.

## Tech Stack

- **Unity:** 6000.2.13f1
- **Render Pipeline:** Universal 2D (URP)
- **Input:** Mouse only (LMB = fire, cursor position drives ship orientation and velocity)
- **Target platform:** Windows

Input: Old Input Manager (UnityEngine.Input.*). The New Input System package is not used — it would be over-engineering for mouse-only control.

## Documentation

The full design lives in an Obsidian vault outside this Unity project:

- **Vault path:** `C:\CinSoftGames`
- **Game folder in vault:** `20 Games/21 Learning Games/21.01 SpaceRunner/`

Planning documents (read before working — they define how the vault, this Unity project, and Claude Code interact):

- `00-09 Plánovanie/02 Workflow vývoja hier.md` — workflow, principles, tool division, pedagogical framework
- `00-09 Plánovanie/04 Dokumentačná architektúra.md` — documentation formats, per-class template, level B detail
- `00-09 Plánovanie/05 Pravidlá pre Claude Code.md` — operational rules (vault write scope, code hygiene, git policy, reporting format) — **binding**

Design notes (consult when working on the matching domain):

- `21.01.01 Koncept.md` — base concept, controls, shooting, graphics, HUD
- `21.01.02 Meteority.md` — meteorite types, sizes, spawn logic, physics
- `21.01.03 Power Ups.md` — fire-rate and shield power-ups, adaptive drop rate
- `21.01.04 Levely.md` — level design philosophy, 10-level progression, Marathon mode
- `21.01.05 Game Flow.md` — screens, transitions, countdown, visual style
- `21.01.06 Audio.md` — SFX, music structure, audio mixing
- `21.01.07 Architektúra.md` — master architecture: system map, principles, folder layout, class registry

Per-class architecture docs (level B detail) live in `21.01 SpaceRunner/_Architektúra/<ClassName>.md`.

Process notes (in `_Operatíva/`):

- `Devlog.md` — chronological log; consult before starting new work
- `TO-DO.md` — actionable tasks
- `Otvorené otázky.md` — open design risks under validation

Vault and notes are written in Slovak. Code stays English.

## Working with the Obsidian Vault

To access the vault from Claude Code, launch with:

​```bash
claude --add-dir C:\CinSoftGames
​```

The vault is the **single source of truth** for all design and architectural decisions. Your operational rules are defined in `00-09 Plánovanie/05 Pravidlá pre Claude Code.md` — **read it before the first code change**. Documentation formats and per-class template are in `00-09 Plánovanie/04 Dokumentačná architektúra.md`.

Key rules summary (full text in 05):

- **Write scope:** you can write to `Assets/Scripts/`, per-class docs in `<game>/_Architektúra/<ClassName>.md` (only in same commit as a code change), and `_Architektúra/_Návrhy úprav.md` (out-of-scope buffer). Everything else is **read-only** — Devlog, TO-DO, Open Questions, design notes, master Architecture, planning docs.
- **Out-of-scope changes** → log a proposal in `_Architektúra/_Návrhy úprav.md` (format in 05). Don't apply the change to the target document. **This overrides explicit user requests in the Claude Code session** — planning context lives in Claude.ai.
- **No deletions** anywhere in the vault. Add and update only.
- **Architectural rationale** — per-class docs are a snapshot of state, not a history of decisions. Rationale lives in Devlog (chronological), master Architecture (project-wide principles), or code comments (technical choices). Don't reconstruct rationale from code; if it's not captured anywhere, log a proposal in `_Návrhy úprav.md` flagging "rationale missing".
- **Drift control** — when code diverges from per-class architecture docs, update the per-class doc in the same commit as the code.

## Pedagogical Context

This is a **learning project** — user's growth is primary, shipping is secondary. Default mode is **pair programming**: provide skeleton + signatures, user fills in non-trivial logic.

Key constraints for Claude Code sessions:

- **Don't introduce advanced patterns proactively** (singleton, DI, ScriptableObjects as data, state machine, async/await, object pooling) — stop and propose discussion in Claude.ai instead
- **Max 1-2 new concepts per session** — propose splitting scope if implementation would introduce more
- **Comments explain *why*, not *what*** — *what* is visible from code, *why* fades within a week
- **When user asks "why did we do X?"** — answer in detail with alternatives and trade-offs; it's a learning question

User knowledge level, full pedagogical workflow (pre-implementation discovery, Stop & Learn, code review cadence, knowledge harvest) and the rest of pedagogical principles are in `C:\CinSoftGames\00-09 Plánovanie\02 Workflow vývoja hier.md`, section *Pedagogický rozmer workflow-u*. The Claude.ai-side entry point is `03 Claude Project context.md`.

## Project Structure (Assets/)

The `Assets/Scripts/` folder structure follows the system map in the architecture document — one folder per domain. There is no `Core/` folder; every domain has an explicit name.

​```
Assets/
├── Scripts/
│   ├── Player/         # ship movement, shield, shooting trigger
│   ├── Weapons/        # projectiles, projectile spawner
│   ├── Meteorites/     # meteorites, meteorite spawner
│   ├── PowerUps/       # power-up entities, power-up spawner
│   ├── World/          # corridor, walls, scrolling, distance tracking
│   ├── LevelSystem/    # level config, level transitions
│   ├── GameFlow/       # screen state machine, transitions, time scale
│   ├── HUD/            # all UI elements (left panel, right panel)
│   ├── Audio/          # music controller, SFX manager
│   └── Persistence/    # save/load (per-level progress, settings)
├── Prefabs/
├── Scenes/
├── Sprites/
├── Audio/Music/        # 3 gameplay tracks + 1 menu track
├── Audio/SFX/
├── Materials/
└── Settings/           # URP renderer asset
​```

Single source of truth for the folder-to-domain mapping is `21.01.07 Architektúra.md`, section *Fyzická štruktúra Unity projektu*. When the structure changes, update the architecture note first, this block second.

## Coding Conventions

### Naming
- Classes, methods, properties, constants: `PascalCase`
- Private fields: `_camelCase` (underscore prefix)
- Parameters and locals: `camelCase`
- One namespace per `Scripts/` subfolder: `SpaceRunner.Player`, `SpaceRunner.Meteorites`, etc.

### File organization
- One `public class` per file; filename matches class name
- `[SerializeField] private` fields for Inspector exposure (not `public`)
- Use `[Range]` and `[Tooltip]` on serialized fields when useful

### Unity patterns
- Cache component references in `Awake()`; never call `GetComponent<>()` in `Update()`
- `Awake()` for self-init, `Start()` for cross-component init
- `FixedUpdate()` only for physics
- No `GameObject.Find()` or string-based lookups in hot paths
- Prefer events / UnityEvents over polling for one-off triggers

### Style
- **Readability over cleverness** — if a simpler and a more advanced approach both work, pick the simpler one until the user signals readiness for the advanced
- XML doc comments on public APIs of non-trivial classes
- Inline comments only where logic is non-obvious

## Architecture

Architectural decisions, principles, and class-level contracts live in the vault — not in this file. Consult:

- **`21.01.07 Architektúra.md`** — system map, project-wide principles (event-based communication, parent-child scroll inheritance, etc.), folder layout. Read before touching any domain for the first time.
- **Per-class docs** in `_Architektúra/<ClassName>.md` — public API, dependencies, invariants. Read before touching the class.
- **Design notes** `21.01.01` through `21.01.06` — what the game is, per domain (concept, meteorites, power-ups, levels, game flow, audio).

When architecture changes, the relevant vault doc is the source of truth — don't reconstruct architectural decisions from this file or paraphrase them here.

## What NOT to Do

Project-specific code conventions:

- Don't expose fields as `public` when `[SerializeField] private` works
- Don't call `GameObject.Find()` or `GetComponent<>()` inside `Update()`; cache instead
- Don't add a score or telemetry system to the base game (only Marathon mode has score)
- Don't add external asset packages or NuGet packages without checking — keep dependency surface minimal

For Unity asset and Project Settings boundaries (don't edit `.unity`, `.prefab`, `.meta`, `ProjectSettings/`, `Packages/manifest.json`), see `05 Pravidlá pre Claude Code.md`, section *Unity assets a Project Settings*.

## Open Risks

Open design questions tracked in `_Operatíva/Otvorené otázky.md`. Most critical for early implementation:

- **Q1 (P1 — blocker):** Does the `v_max × sin(angle)` control scheme hold up at high meteorite density (12+/sec)? An early-validation prototype is required before calibrating L2–L5.

Until Q1 is validated, prefer prototyping over premature optimization.