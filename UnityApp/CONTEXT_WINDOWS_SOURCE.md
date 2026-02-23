# Unity Mirror Context (Windows Source)

## Core Rule For AI Tasks

When working from this repository:

1. Analyze `UnityApp/` as the mirrored reference.
2. Treat the Windows Unity project as the execution source of truth:
   - `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity`
3. If implementation must be applied in Unity runtime/editor, target Windows source paths.
4. Use mirror sync script to refresh Linux context before major AI-assisted edits.

## Key Runtime Files

- `UnityApp/Assets/Scripts/Session/PracticeSessionController.cs`
- `UnityApp/Assets/Scripts/Fretboard/FretboardRenderer.cs`
- `UnityApp/Assets/Scripts/Fretboard/FretMarker.cs`
- `UnityApp/Assets/Scripts/UI/ChordProgressionPanel.cs`
- `UnityApp/Assets/Scripts/UI/ChordSelector.cs`
- `UnityApp/Assets/Scripts/UI/TransportControls.cs`
- `UnityApp/Assets/Scripts/Timing/TimingEngine.cs`
- `UnityApp/Assets/Data/ChordDatabase.asset`

## Key Editor Automation Files

- `UnityApp/Assets/Scripts/Editor/SetupChordSelector.cs`
- `UnityApp/Assets/Scripts/Editor/SetupUIControls.cs`
- `UnityApp/Assets/Scripts/Editor/SetupSceneReferences.cs`
- `UnityApp/Assets/Scripts/Editor/BuildWebGL.cs`

## Mirror Maintenance

- Policy: `UnityApp/SYNC_POLICY.md`
- Script: `scripts/sync_unity_from_windows.sh`
