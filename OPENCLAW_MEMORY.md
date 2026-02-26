# OPENCLAW_MEMORY.md

Scope: This file stores OpenClaw-specific operating preferences for this project only.

## Document Resolution Order (for tasks in this project)
1. `codex/AGENTS.md` (primary, highest priority)
2. Domain-specific `*.md` related to the request context (UI/ML/hardware/etc.)
3. `CLAUDE.md`, `gemini/GEMINI.md` as supporting context

Conflict rule: Always follow `codex/AGENTS.md`.

## Execution Reporting Preference
For longer tasks, report in this flow:
1. Execution started
2. Mid-run streaming summary (key progress/errors)
3. Final completion report (result + next action)
