# UnityApp Sync Policy

## Purpose

`UnityApp/` in this repository is a Linux-side mirror for collaboration and AI context.
The Windows project is the execution runtime (Unity Editor), but both sides are editable.

- Windows runtime path: `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity`
- Linux mirror path: `UnityApp/`

## Direction

**Bidirectional sync** — choose the direction based on where you made edits:

| 수정 위치 | 실행할 명령 |
|----------|------------|
| Windows에서 수정 후 Linux 반영 | `bash scripts/sync_unity_from_windows.sh` |
| Linux에서 수정 후 Windows 반영 | `bash scripts/sync_unity_to_windows.sh` |

Both scripts are **add/update only (no delete)** by default.

## Sync Commands

### Windows → Linux
```bash
# dry-run
bash scripts/sync_unity_from_windows.sh --dry-run

# apply
bash scripts/sync_unity_from_windows.sh
```

### Linux → Windows
```bash
# dry-run
bash scripts/sync_unity_to_windows.sh --dry-run

# apply
bash scripts/sync_unity_to_windows.sh
```

### 커스텀 경로 사용 시
```bash
UNITY_WIN_SRC=/path/to/GuitarPracticeUnity bash scripts/sync_unity_from_windows.sh
UNITY_WIN_SRC=/path/to/GuitarPracticeUnity bash scripts/sync_unity_to_windows.sh
```

## Allowlist (Synced in both directions)

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

### AI(Claude/Codex)가 Linux에서 C# 수정 후
1. `bash scripts/sync_unity_to_windows.sh --dry-run` 으로 변경 내용 확인
2. `bash scripts/sync_unity_to_windows.sh` 적용
3. Unity Editor에서 재컴파일 확인 (에러 0건)

### Windows Unity Editor에서 수정 후
1. `bash scripts/sync_unity_from_windows.sh --dry-run` 으로 변경 내용 확인
2. `bash scripts/sync_unity_from_windows.sh` 적용
3. `git status` 확인 후 커밋
