# SpaceRunner

2D arcade vertical-scroller shoot 'em up inspired by Tyrian 2000. The player flies a ship through a corridor, dodges and shoots meteorites of multiple sizes, and collects power-ups. Mouse-only controls. **Learning project** — focus is practicing Unity development, not commercial release.

## Tech Stack

- **Unity:** 6000.2.13f1
- **Render Pipeline:** Universal 2D (URP)
- **Input:** Mouse only (LMB = fire, cursor position drives ship orientation and velocity)
- **Target platform:** Windows

## Documentation

The full design lives in an Obsidian vault outside this Unity project:

- **Vault path:** `C:\CinSoftGames`
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

## Project Structure (Assets/)

```
Assets/
├── Scripts/
│   ├── Core/         # GameManager, LevelLoader, GameState
│   ├── Player/       # Ship movement, input, rotation
│   ├── Weapons/      # Projectile, fire rate, weapon system
│   ├── Meteorites/   # Meteor base, sizes, spawner
│   ├── PowerUps/     # PU base, FireRatePU, ShieldPU
│   ├── HUD/          # Level progress, shield/fire counters
│   └── Audio/        # MusicController, SFX manager
├── Prefabs/
├── Scenes/
├── Sprites/
├── Audio/Music/      # 3 gameplay tracks + 1 menu track
├── Audio/SFX/
├── Materials/
└── Settings/         # URP renderer asset, Input actions
```

Each `Scripts/` subfolder maps 1:1 to a design note in the vault.

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