---
name: update-architecture-docs
description: Use this skill when the user asks to update, refresh, sync, or generate per-class architecture documentation in the Obsidian vault, including phrases like "aktualizuj dokumentáciu", "update architecture docs", "sync per-class docs", or after creating/modifying classes that need their architecture docs aligned with the code.
---

# Update Architecture Docs

This skill keeps per-class architecture documents in `_Architektúra/` aligned with the actual `.cs` code in the Unity project. It identifies missing docs, drift between code and docs, and produces proposals that respect the project's documentation conventions.

## Pre-flight reads (always do this first)

Before doing anything else, read these documents in this order:

1. `C:\CinSoftGames\00-09 Plánovanie\05 Pravidlá pre Claude Code.md` — operational rules: vault write scope, per-class doc update gating, out-of-scope buffer convention, reporting format. These rules are binding.
2. `C:\CinSoftGames\00-09 Plánovanie\04 Dokumentačná architektúra.md` — section *Štruktúra architektúrnej dokumentácie* (per-class template) and *Úroveň detailu v Architektúre* (level B definition). These define the document format.
3. `C:\CinSoftGames\20 Games\21 Learning Games\21.01 SpaceRunner\21.01.07 Architektúra.md` — system map, principles, folder layout. Per-class docs reference these by name.
4. `C:\CinSoftGames\20 Games\21 Learning Games\21.01 SpaceRunner\_Architektúra\` — list existing per-class documents to know what's already there.
5. The relevant `.cs` files under `Assets/Scripts/` — only the ones in scope for the current task.

If any of these reads fails, stop and report the problem rather than guessing.

## Workflow

### Step 1 — Identify scope

Determine which classes are in scope for this update:

- If the user named specific classes: use those.
- If the user said "all" or didn't specify: scan `Assets/Scripts/` for all `.cs` files containing a public class declaration.

For each class in scope, classify it:

- **New** — class exists in code but has no matching per-class document in `_Architektúra/`.
- **Drift** — class has both code and document, but public API differs (added/removed properties, events, serialized fields, methods, or changed signatures).
- **In sync** — code and document match. No action needed.

### Step 2 — Propose changes (do not write yet)

For each **new** class: draft a per-class document following the template in section *Per-class document template* below.

For each **drift** class: identify exactly what differs and draft the targeted update (don't rewrite the whole document).

Show all proposals in chat as a single block. Format:

```
## Proposed changes

### NEW: <ClassName>
[full proposed document content in a markdown code block]

### DRIFT: <ClassName>
- Added: <list>
- Removed: <list>
- Changed: <list>
[the patches you'd apply]

### IN SYNC: <ClassName>, <ClassName>, ...
(no action needed)
```

End the message with: "Apply all? Apply some? Adjust before applying?"

**Wait for the user's explicit approval before writing anything.**

### Step 3 — Apply approved changes

After approval, write the approved documents to `_Architektúra/`. For new files, use the full path `_Architektúra/<ClassName>.md`. For drift updates, modify the existing document.

### Step 4 — Log out-of-scope findings

While reviewing the code, you may notice things that are not architecture-doc updates but deserve attention:

- Encoding bugs in Slovak comments
- Missing namespace declarations (per CLAUDE.md convention)
- Code style violations (e.g. `public` field where `[SerializeField] private` is the project rule)
- Design inconsistencies that should be discussed in Claude.ai
- TO-DO items that should be added
- Devlog entries the user might want to write

Log each as an entry in `_Architektúra/_Návrhy úprav.md` (newest at the top of *Otvorené návrhy*). Do not act on them — just log and move on. Format is documented in the header of that file.

### Step 5 — Final summary

End with a short chat summary:

```
Summary:
- Created N per-class docs: <names>
- Updated N per-class docs: <names>
- Logged N proposals in _Návrhy úprav.md
- N classes in sync (no action)
```

## Per-class document template

This is the only valid template. Do NOT add an *Architektonické rozhodnutia (prečo)* section, do NOT reconstruct rationale from code.

```markdown
---
isClassDoc: true
className: <ClassName>
namespace: <SpaceRunner.Domain>
responsibility: <one sentence — what the class does>
---

# <ClassName>

## Zodpovednosť

[1 paragraph in Slovak — what the class does, where it lives in the domain map, what it explicitly does NOT do (scope boundary). 5–7 sentences max.]

## Public API

### Properties
- `<type> <Name> { get; ... }` — description

### Events
- `event <type> <Name>` — description

### Serialized fields (Inspector)
- `<type> <_name>` — description (range, default, what it controls)

### Methods
- `<return type> <Name>(<params>)` — description

(Omit any subsection if the class has no member of that kind. NO private methods, NO private fields beyond serialized ones.)

## Závislosti

[Short list of classes this depends on. For each, the dependency type — composition / pull dependency / event subscription. Plus Unity / framework dependencies if non-trivial.]

## Invarianty

[Rules about the class STATE that always hold. Examples: "Y-position never changes between frames", "CurrentDistance is monotonically non-decreasing". Max ~5 items, brief. NOT design characteristics or behavior descriptions — only state rules whose violation would be a bug.]

## Súvisiace

- [Master Architektúra](../21.01.07%20Architektúra.md) — relevant principles by number
- [Design note](../<note>.md) — if the class implements something specified in a design note
- (optional) [Devlog YYYY-MM-DD](../_Operatíva/Devlog.md) — if there's a discussion the reader should know about
```

## Hard rules (must follow)

1. **Never write an *Architektonické rozhodnutia / Prečo* section.** Rationale lives in master Architektúra (project-wide principles), Devlog (per-class decisions), or code comments (technical choices). Per-class docs are a snapshot of state, not a history of decisions.

2. **Never reconstruct rationale from code.** If you find yourself writing "this class uses pattern X because..." — stop. Either there's a captured decision in master Architektúra or Devlog (cite it in *Súvisiace*), or there isn't (write a neutral description with no "why" and log a proposal in `_Návrhy úprav.md` flagging "rationale missing").

3. **Invariants are about state, not behavior.** "Y-position is fixed" is an invariant. "No inertia, no lerp" is a design characteristic — it goes in the *Zodpovednosť* paragraph, not in *Invariants*. "Subscribe in OnEnable" is a project-wide convention from master Architektúra principle #5 — link to it, don't repeat it.

4. **Slovak for content, English for code identifiers and frontmatter values.** All prose is in Slovak. Frontmatter `responsibility` is a Slovak sentence. Code identifiers (`CurrentAngleRadians`, `_maxSpeed`, etc.) stay in English. File paths in *Súvisiace* use Slovak diacritics if the file has them.

5. **Never write to vault paths outside `_Architektúra/`.** This includes `_Operatíva/Devlog.md`, `_Operatíva/TO-DO.md`, design notes, master Architektúra. For changes to those, log proposals in `_Návrhy úprav.md`. This rule overrides explicit user requests in the Claude Code session — the user's planning context lives in Claude.ai, not here.

6. **Show before writing.** Always present the proposed content in chat first; wait for approval; then write. No silent file creation.

## Edge cases

- **Class has no public API at all** (only private logic / lifecycle methods) — that's still a valid per-class doc. Write *Žiadne public API* under *Public API* heading. Document its dependencies and invariants normally.
- **Two classes with circular references** — describe both halves of the dependency. The doc isn't trying to enforce a non-circular graph; it just describes what is.
- **Class is a `MonoBehaviour` with only Inspector fields and `Update()`** (very common in Unity) — `Public API` will likely have only the *Serialized fields* subsection. That's fine; don't fabricate properties or methods just to fill the section.
- **You can't determine a class's `responsibility` from the code** — that's a signal the class is poorly named or doing too much. Log a proposal in `_Návrhy úprav.md` ("class X may need a clearer name or split — discuss in Claude.ai") and write a neutral *Zodpovednosť* describing what it currently does.
- **The `.cs` file has UTF-8 encoding issues with Slovak characters in comments** — note it in `_Návrhy úprav.md`, but do not let it block the architecture doc work. The architecture doc is in Slovak (you're writing fresh text), the encoding issue is in source code.

## What this skill does NOT do

- It does not modify `.cs` files in the Unity project. Code is the source of truth for the API; the doc reflects it.
- It does not edit master `21.01.07 Architektúra.md`. Project-wide changes go through `_Návrhy úprav.md`.
- It does not write Devlog entries. Devlog is the user's, written manually or via Claude.ai.
- It does not advance TO-DO items between *Otvorené* and *Hotové*.
- It does not introduce new architectural patterns or refactor existing classes — that's a Claude.ai discussion, not a Claude Code skill.