# UnityApp Sync Policy

## Purpose

`UnityApp/` in this repository is a Linux-side mirror for collaboration and AI context.
The real Unity source of truth remains the Windows project:

- `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity`

Windows files must not be edited by this sync workflow.

## Direction

One-way sync only:

- `Windows Unity -> Linux repo UnityApp`

Default behavior is add/update only (no delete).

## Sync Command

Dry-run:

```bash
./scripts/sync_unity_from_windows.sh --dry-run
```

Apply:

```bash
./scripts/sync_unity_from_windows.sh
```

Override source path on another machine:

```bash
UNITY_WIN_SRC=/path/to/GuitarPracticeUnity ./scripts/sync_unity_from_windows.sh --dry-run
```

## Allowlist (Synced)

- `Assets/Scripts/**` (including `Editor/**` and `.meta`)
- `Assets/Data/**` (including `.asset` and `.meta`)
- `Assets/Prefabs/**` (including `.meta`)
- `Assets/Plugins/WebGL/**` (including `.meta`)
- `Assets/Scenes/**` (including `.meta`)
- `Assets/MainScene.unity`
- `Assets/MainScene.unity.meta`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/McpUnitySettings.json`

## Exclusions (Not Synced)

- `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`
- `Builds/` outputs
- `.vs/`, `.plastic/`
- Other generated/cache artifacts

## Recommended Flow

1. Run dry-run and inspect summary.
2. Run apply sync.
3. Review `git status` and staged changes.
4. Commit only intended mirror updates.
