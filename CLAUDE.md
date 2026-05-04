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
- **Workflow document:** `00-09 Plánovanie/02 Workflow vývoja hier.md` — read this first; it defines how the vault, this Unity project, and Claude Code work together
- **Game folder in vault:** `20 Games/21 Learning Games/21.01 SpaceRunner/`

Design notes (consult when working on the matching domain):

- `21.01.01 Koncept.md` — base concept, controls, shooting, graphics, HUD
- `21.01.02 Meteority.md` — meteorite types, sizes, spawn logic, physics
- `21.01.03 Power Ups.md` — fire-rate and shield power-ups, adaptive drop rate
- `21.01.04 Levely.md` — level design philosophy, 10-level progression, Marathon mode
- `21.01.05 Game Flow.md` — screens, transitions, countdown, visual style
- `21.01.06 Audio.md` — SFX, music structure, audio mixing

Process notes (in `_Operatíva/`):

- `Devlog.md` — chronological log; consult before starting new work
- `TO-DO.md` — actionable tasks
- `Otvorené otázky.md` — open design risks under validation

Vault and notes are written in Slovak. Code stays English.

## Working with the Obsidian Vault

This project is documented in an Obsidian vault outside this Unity project (see *Documentation* above). To access it, launch Claude Code with:

```bash
claude --add-dir C:\CinSoftGames
```

The vault is the **single source of truth** for all design and architectural decisions. The full workflow contract is in `00-09 Plánovanie/02 Workflow vývoja hier.md` — consult it for the complete set of rules. Key rules summarized here:

### Vault write scope

Your write access is **limited to the `_Architektúra/` subfolder** of the current game (per-class `.md` docs, Canvas `.canvas` files, the operational `_Návrhy úprav.md`). Outside this subfolder you have **read access only** — this includes design notes, master architecture, Devlog, TO-DO, Open Questions, and all planning/knowledge folders.

If a task requires changes outside this scope, log a proposal entry in `_Architektúra/_Návrhy úprav.md`. **This applies even when the user explicitly asks for the edit elsewhere** — respond with "I'm logging this as a proposal for review in Claude.ai." Planning, design, and learning discussions happen in Claude.ai; you don't have that context.

### Proposal buffer (`_Návrhy úprav.md`)

When you identify a change outside `_Architektúra/` (Devlog entry to draft, TO-DO item to add, design note inconsistency, code issue outside refactoring scope, etc.), log it as an entry in `_Architektúra/_Návrhy úprav.md`. Don't apply the change to the target document.

Entry format (newest at top of *Otvorené návrhy*):
```
### YYYY-MM-DD — Short title
**Cieľový dokument:** relative path (or "kód: <path>")
**Návrh:** description
(optional) **Kontext:** why this is needed
```

The user reviews proposals in Claude.ai sessions and applies sensible ones manually.

### Architectural rationale — only actual design decisions

In per-class documents, the *Architektonické rozhodnutia (prečo)* section is for **decisions that were actually deliberated** during design (typically captured in master Architektúra, design notes, Devlog, or sibling per-class docs). Cite or paraphrase that source.

If rationale is **not captured anywhere**, write a neutral description (e.g. "class uses pattern X" with no "why"), and log a proposal in `_Návrhy úprav.md` flagging "rationale missing." **Do not reconstruct rationale from code.** Technical or forced choices (Unity API calls, language idioms, math conventions) get neutral descriptions, not fabricated "why" justifications.

### Other vault rules

- **Do NOT delete existing content** anywhere in the vault. Add and update only.
- **Architecture note (`21.01.07 Architektúra.md`):** level B detail only — public APIs, responsibilities, invariants. No private methods or implementation details. (Read-only for you; propose master-level changes via `_Návrhy úprav.md`.)
- **When code diverges from per-class architecture docs, update the per-class doc in the same commit as the code.** Drift is the failure mode.

Note: the vault and notes are written in Slovak. Code, identifiers, and technical comments stay in English.
## Pedagogical Context

This is a **learning project**. The user's goal is growth, not just shipping the game. Without explicit pedagogical structure, the default risk is "Claude Code writes everything, user accepts, learns nothing." These rules prevent that.

### User's starting knowledge

- **Languages:** intermediate in C# and Python; prior background in Java
- **OOP fundamentals:** comfortable with classes, inheritance, polymorphism, interfaces
- **OOP advanced:** lacks depth in singletons, observer pattern (custom events / listeners), dependency injection, state machines
- **Unity Editor:** comfortable navigating
- **Unity best practices:** basic ("separate logic from graphics"); no systematic exposure

### Rules for Claude Code

1. **Do not introduce advanced patterns proactively.** Singleton, observer/custom events, dependency injection, ScriptableObjects as data containers, coroutines, async/await, state machine, object pooling — none of these gets implemented without prior discussion in Claude.ai. If a pattern feels needed, **stop and tell the user** "this would be a good place for pattern X — discuss in Claude.ai first?" rather than implementing it.

2. **Prefer the simpler approach over the more elegant one** when it doesn't break the design. Object pooling can wait until performance actually demands it. The advanced concept gets introduced *after* the user recognizes its value, not before.

3. **Comments explain *why*, not *what*.** *What* is visible from the code. *Why* fades within a week. For non-obvious choices (event vs. direct call, ScriptableObject vs. plain class, etc.) leave a short rationale comment.

4. **For architectural decisions, write the rationale in the relevant per-class document** in `_Architektúra/` (section *Architektonické rozhodnutia*). Without this, the logic of the choice is lost on review a month later.

5. **Code style: readability over cleverness.** If a more advanced and a simpler approach both work, pick the simpler one — until the user is comfortable with the more advanced.

6. **When the user asks "why did we do X this way?"** — answer in detail, give alternatives, give trade-offs. This is a learning question, not a request for confirmation.

7. **Implementation modes per task:**
   - **Fully Claude Code** — only for routine boilerplate the user asked to be done that way
   - **Pair programming (default)** — Claude Code provides skeleton + signatures; user fills in method bodies. The user decides which methods they want to write themselves
   - **Fully user** — for learning-critical code (first observer pattern, first state machine, first use of a newly Stop & Learned concept). User asks for review afterward

   Default if not specified: pair programming with the user filling in non-trivial logic.

8. **One or two new concepts per session, max.** When implementation would introduce 3+ new concepts at once, stop and propose splitting the work — either across sessions, or by simplifying scope.

The full pedagogical workflow (pre-implementation discovery, Stop & Learn ritual, code review cadence, knowledge harvest) is documented in `C:\CinSoftGames\00-09 Plánovanie\02 Workflow vývoja hier.md`, section *Pedagogický rozmer workflow-u*.

## Project Structure (Assets/)

The `Assets/Scripts/` folder structure follows the system map in the architecture document — one folder per domain. There is no `Core/` folder; every domain has an explicit name.

```
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
```

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

### Language
- Code, identifiers, comments: English
- XML doc comments on public APIs of non-trivial classes
- Inline comments only where logic is non-obvious

## Key Architectural Decisions

These are deliberate decisions from the design phase. Respect them — don't "fix" them.

### Player movement
- Ship anchored at fixed Y; only X position changes
- `horizontal_velocity = v_max × sin(angle_from_vertical)`, where angle is determined by cursor relative to ship
- **Immediate rotation, no lerp, no inertia**
- Cursor below ship Y is clamped for orientation (ship cannot aim or move downward)

### Shooting
- Cooldown-based fire rate (not per-click)
- Projectile direction = ship orientation (single source: cursor angle)

### Meteorites
- 3 sizes (Big/Medium/Small) × 2 colors (Black = standard, Red = drops a power-up)
- On destroy: Big → 2 Medium, Medium → 2 Small, Small → explode
- Physics collisions (meteor-meteor, meteor-wall) via Rigidbody2D
- Spawn velocity: random direction within ±30° cone from vertical
- Hit count rendered as text in meteor center; decrements on each hit

### Power-ups
- Two types: FireRate (stackable, +1 shot/sec per pickup) and Shield (binary, max 1)
- Reset on every level start; no carry-over
- FireRate adaptive drop: `P = max(P_min, P_base × (1 − k × picked_count))`

### HUD
- Two side panels outside the gameplay corridor
- Color rule: **white = static/reference, red = active/dynamic** — applies project-wide
- Right panel = stack of icons (white outline + optional red fill + optional `×N`)
- New power-ups extend the stack; no HUD refactor

### Levels
- 10 sequential levels, pass/fail only (no score in base game)
- Marathon Challenge unlocked after L10 (only mode with a numeric score)
- Each level has 8 per-level parameters tuned via testing

## What NOT to Do

- Don't edit `.unity` scenes or `.prefab` files outside the Unity Editor
- Don't manually edit `.meta` files — Unity manages them
- Don't change `Packages/manifest.json` without explicit approval
- Don't expose fields as `public` when `[SerializeField] private` works
- Don't call `GameObject.Find()` or `GetComponent<>()` inside `Update()`; cache instead
- Don't add a score or telemetry system to the base game (only Marathon mode has score)
- Don't add external asset packages or NuGet packages without checking — keep dependency surface minimal

## Open Risks

Five open design questions tracked in `_Operatíva/Otvorené otázky.md`. Most critical for early implementation:

- **Q1 (P1 — blocker):** Does the `v_max × sin(angle)` control scheme hold up at high meteorite density (12+/sec)? An early-validation prototype is required before calibrating L2–L5.

Until Q1 is validated, prefer prototyping over premature optimization.