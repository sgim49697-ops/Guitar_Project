# Guitar Practice App — Codex Agent Instructions

This file defines project conventions, agent roles, and coding rules for OpenAI Codex CLI working on the Guitar Practice project.

---

## Project Overview

A guitar practice tool consisting of three components:

| Component | Path | Description |
|-----------|------|-------------|
| Unity WebGL App | `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/` | Main interactive fretboard UI (C#) |
| FastAPI Backend | `./api/main.py` | AI chord recommendation endpoints |
| Tunerapp | `./Tunerapp/` | Web-based guitar tuner (Express + WebAudio) |

**Deployment:** Docker Compose, port 8000. Unity WebGL build served as static files.

---

## Critical Path Rules

### Unity Scripts
- **ALWAYS use Windows/WSL path:** `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/Assets/Scripts/`
- **NEVER use:** `/home/user/projects/Guitar_Project/UnityApp/` (deprecated, unused)
- After editing C# files, Unity must recompile before testing

### Python
- **ALWAYS use `uv pip`** — never bare `pip`
- Install: `uv pip install <package>`
- PyTorch requires CUDA index: `uv pip install torch --index-url https://download.pytorch.org/whl/cu128`

### Docker
- Restart after `server.js` changes: `docker compose restart`
- Tunerapp `index.html` edits: no restart needed (static file)

---

## Repository Structure

```
Guitar_Project/
├── Tunerapp/
│   ├── index.html          # Tuner UI (WebAudio pitch detection)
│   └── server.js           # Express server (port 8000)
├── api/
│   └── main.py             # FastAPI AI endpoints
├── docker-compose.yml
├── CLAUDE.md               # Project context (canonical reference)
├── codex/
│   └── AGENTS.md           # This file
└── gemini/
    └── GEMINI.md           # Gemini CLI instructions

Unity Project: /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/
└── Assets/Scripts/
    ├── Fretboard/
    │   ├── FretboardRenderer.cs    # Builds 6-string fretboard, chord animations
    │   ├── FretMarker.cs           # Per-fret sprite marker with state machine
    │   ├── FretboardConfig.cs      # ScriptableObject (colors, spacing)
    │   └── FretMarkerPulse.cs      # Pulse animation for active markers
    ├── Session/
    │   ├── PracticeSessionController.cs  # Session lifecycle, chord progression loop
    │   ├── TimingEngine.cs               # DSP-based beat timing
    │   ├── ChordDatabase.cs              # ScriptableObject list of ChordData
    │   └── ChordData.cs                  # Chord name + fret positions + finger numbers
    ├── UI/
    │   ├── ChordSelector.cs        # Chord progression selector (1–8 slots, cycling buttons)
    │   ├── ChordProgressionPanel.cs # Visual display of current progression
    │   ├── BPMControl.cs           # BPM slider UI
    │   ├── TransportControls.cs    # Play / Stop buttons
    │   └── UIManager.cs            # Indicator reset, general UI state
    ├── API/
    │   └── AIClient.cs             # HTTP client for FastAPI backend
    └── Editor/
        ├── ChordDataCreator.cs         # Guitar menu: auto-populate ChordDatabase
        ├── SetupChordSelector.cs       # Guitar menu: create SelectorBar UI
        └── SetupUIControls.cs          # Guitar menu: create ControlBar UI
```

---

## Current Implementation State (as of 2026-02-22)

### Unity WebGL App
- **FretMarker state machine:** `Inactive → Preview → Transitioning → Settling → Active`
  - Inactive: near-transparent gray `(0.15, 0.15, 0.15, 0.3)`
  - Active: bright cyan `(0, 1, 1, 1)` with pulse animation
  - Preview: dim cyan (next chord preview, shown on beat 3)
  - Transitions: crossfade animations
- **ChordDatabase:** 9 chords — C, G, Am, F, D, Em, A, E, Dm
- **ChordSelector UI:** SelectorBar (y=122–170px, left 65% of screen)
  - Chord slots: cycling buttons (click = next chord in list)
  - Add/Remove buttons: 1–8 slots
  - Apply button: stops session → applies new progression → restarts
- **Canvas layout (1151×552):**
  - ControlBar: y = 0–120 (Play/Stop/BPM)
  - SelectorBar: y = 122–170 (chord selection)
  - Safe UI zones: top bar y=470–552, HUD top-right x=828–1151

### FastAPI Backend (`api/main.py`)
- Chord recommendation endpoint planned: `POST /api/recommend-fingering`
- Currently minimal — no ML model deployed yet

### Tunerapp
- Autocorrelation pitch detection, optimized for guitar (70–1400 Hz)
- `fftSize = 8192`, RMS threshold = 0.001
- Known: thin strings (1–2) have lower detection accuracy

---

## Agent Roles

Three specialized sub-agents handle different parts of the system. The main Codex session acts as orchestrator.

---

### Agent: fingering-coach-agent
**Status:** ACTIVE — MVP Phase 1

**Trigger keywords:** 화면 바꿔줘, UI 수정, API 응답 형태 바꿔줘, DTO 정의, 운지법 힌트 표시, 코칭 메시지, 다음 코드 표시, 텔레메트리 추가

**Scope:**
- Unity UI panels: ChordProgressionPanel, coaching overlay, hint display
- DTO definitions for backend responses:
  - `suggested_fingering` (string + fret positions + finger numbers)
  - `coaching_messages` (prioritized list)
  - `next_exercise` (optional)
- Lightweight telemetry (no PII): `chord_completed`, `retry_count`, `common_error_type`
- C# script changes in `Assets/Scripts/UI/`

**Out of scope for this agent:**
- ML models, feature extraction → chord-recognizer-agent
- Hardware/LED/MQTT → hardware-bridge-agent

**UI Validation Checklist (mandatory before completing UI tasks):**
1. Compile errors: 0
2. Verify RectTransform anchor/offset values match intent
3. Layout overlap check — no UI panel should cover the fretboard (center of screen)
4. All serialized fields connected (not null)
5. Report results in format:
   ```
   ## 검증 결과
   - [ ] 컴파일 오류: 0건
   - [ ] 레이아웃 겹침: 없음
   - [ ] 참조 연결: 전체 연결됨
   - [ ] 동작 확인: (description)
   ```

**Rules:**
- Responses must be backward compatible
- UI text: concise, friendly, actionable
- Never hardcode secrets
- No Bash — file I/O and edits only

---

### Agent: chord-recognizer-agent
**Status:** INACTIVE — Phase 4 (post-MVP)

**Trigger keywords:** 코드 인식해줘, 음 감지, ML 붙여줘, 추천 기능, 피처 뽑아줘, /api/recommend-fingering 구현, 평가 스크립트, 정확도 측정

**Scope:**
- FastAPI endpoint: `POST /api/recommend-fingering`
- Feature extraction utilities (audio → chord features)
- Offline evaluation script for model accuracy
- Deterministic heuristics first; ML only when training data exists

**Out of scope for this agent:**
- Unity UI → fingering-coach-agent
- Hardware → hardware-bridge-agent

**Rules:**
- Never hardcode secrets
- Bash must be non-destructive and repo-scoped
- Validate Python type errors after changes
- When uncertain, ask rather than guess

---

### Agent: hardware-bridge-agent
**Status:** INACTIVE — Phase 5 (post-MVP)

**Trigger keywords:** LED 켜줘, 하드웨어 연결, MQTT 붙여줘, BLE 통신, 디바이스 제어, /api/hardware 엔드포인트, 프레임 전송, 보안 검토

**Scope:**
- Bridge module for one transport: MQTT (preferred) OR BLE OR Serial
- Backend endpoints: `POST /api/hardware/push-frame`, `POST /api/hardware/clear`
- Security: device ID allowlist, rate limiting, auth token for remote calls
- FretMarker state → LED frame mirroring

**Out of scope for this agent:**
- Unity UI → fingering-coach-agent
- ML/chord recognition → chord-recognizer-agent

**Rules:**
- Never hardcode credentials — use environment variables
- Keep bridge isolated from ML logic
- Security review required before any remote connection feature is enabled
- Non-destructive Bash only

**Note:** Use the most capable model for this agent (security review and protocol design decisions are involved).

---

## Orchestrator Rules (Main Session)

### Before delegating to a sub-agent
1. Read relevant files directly — never delegate without knowing the current file content
2. Include actual code in the delegation prompt — agents should not guess
3. Include extension scenarios in the prompt:
   - Adding songs → same animation API (HighlightChord/TransitionToChord) reused
   - Mic chord recognition → recognized chord displayed via HighlightChord
   - Hardware LED → FretMarker state mirrored to LED bridge

### After receiving sub-agent results (mandatory)
1. Read ALL files the agent modified — never trust completion message alone
2. For Unity C# changes: recompile and verify 0 errors
3. Fix any issues found directly — avoid re-delegation loops
4. Check that existing public APIs are not broken:
   - `HighlightChord(ChordData)`, `TransitionToChord(ChordData, float)`
   - `PreviewNextChord(ChordData)`, `ClearAllHighlights()`
   - `SetActive(bool)`, `SetPreview()`, `CrossfadeToActive(float)`, `CrossfadeToInactive(float)`

### Completion conditions
A task is complete ONLY after:
1. Modified files read and logic verified
2. Unity scripts: 0 compile errors confirmed
3. Existing public APIs not broken
4. Code is reusable for future extension scenarios

---

## Architecture Principles

- **Unity** = UI + interaction layer
- **FastAPI** = logic layer (recommendations, session logging, future LED bridge + ML inference)
- Network I/O isolated behind stable interfaces (currently HTTP)
- No strong coupling between UI and AI network calls
- Use DTOs for JSON communication; maintain backward-compatible fields for hardware expansion

---

## Coding Standards (Unity C#)

- New C# files: one-line description comment at top
  ```csharp
  // ChordSelector.cs - 코드 진행 선택 UI (개수 동적 조절 1~8)
  ```
- Store chord names as strings, not indices (index-based approaches break when DB order changes)
- Lambda closures in loops: always capture loop variable
  ```csharp
  int captured = i;
  btn.onClick.AddListener(() => DoSomething(captured));
  ```
- `Awake()` for initialization that other components depend on; `Start()` for logic that needs other components
- HorizontalLayoutGroup with dynamic children: set `childForceExpandHeight = true` to ensure click areas

## Coding Standards (Python)

- New files: one-line description comment at top
  ```python
  # main.py - FastAPI backend: chord recommendation and session logging
  ```
- Input validation at system boundaries (API endpoints)
- Meaningful error messages with HTTP status codes
- No raw audio storage without explicit privacy filter

---

## Data & Privacy

- Session logs: store only what is necessary
- No raw audio storage without explicit consent and privacy filter
- No PII in telemetry events

---

## Known Issues / Caveats

- Unity Editor scripts (Assets/Scripts/Editor/) require Unity to recompile before their menu items appear
- FretboardConfig color changes don't auto-apply to existing assets — use `Guitar > Update FretboardConfig Colors` menu or Inspector
- SelectorBar slots store chord names (not indices) to prevent ordering bugs
- ScrollRect in Unity Canvas can cause invisible buttons at runtime — use simple HorizontalLayoutGroup instead
- `childForceExpandHeight = false` with no explicit LayoutElement height → button height = 0 → unclickable
