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

- **Do NOT create new notes in the vault without explicit user approval.** Editing existing notes (Devlog, Architecture, TO-DO, Open Questions) is fine.
- **Do NOT delete existing content.** Add and update; never overwrite entire sections or remove prior entries.
- **Devlog (`_Operatíva/Devlog.md`):** newest entries on top. Use the existing entry structure: date heading + *Čo som robil* / *Na čo som narazil* / *Ako som to vyriešil* / `#do-knowledge` (optional). Write in first person from the user's perspective — the devlog is theirs, not yours. When unsure, draft briefly and let the user refine on review.
- **Architecture note (`21.01.07 Architektúra.md`):** level B detail only — public APIs, responsibilities, invariants. No private methods or implementation details.
- **When implementation diverges from architecture, update the architecture note in the same logical commit as the code.** Drift between doc and code is the failure mode to avoid.
- **TO-DO (`_Operatíva/TO-DO.md`):** do not move tasks from *Otvorené* to *Hotové* without explicit user approval. Suggest the move; don't perform it.
- **Open Questions (`_Operatíva/Otvorené otázky.md`):** when closing a question, move it to the *Uzavreté* section with the resolution and date. Do not delete open questions.

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