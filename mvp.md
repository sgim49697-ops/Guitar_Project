# Guitar Practice MVP — 구현 기록

> 작성일: 2026-02-21
> 목표: 코드 진행을 선택하면 운지법이 타이밍에 맞춰 지판에 점등되는 것

---

## 1. MVP 목표

```
[코드 진행 선택] → [BPM 설정] → [Play] → [마디마다 지판 점등 + 코드 슬롯 하이라이트] → [Stop]
```

기본 진행: **C → G → Am → F** (4마디 루프)

---

## 2. 프로젝트 구조

```
Guitar_Project/
├── Tunerapp/               # 기타 튜너 웹앱 (Express + WebAudio)
├── api/                    # FastAPI 백엔드 (AI 코드 설명)
├── docker-compose.yml      # 서비스 오케스트레이션
└── UnityApp/               # (구버전, 미사용)

# Unity 실제 경로 (Windows / WSL 마운트)
C:\Projects\Project_Guitar\GuitarPracticeUnity\
→ WSL: /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/
```

### Unity 스크립트 구조

```
Assets/Scripts/
├── Data/
│   ├── ChordData.cs              # 코드 운지 데이터 (FretPosition[])
│   ├── ChordDatabase.cs          # ScriptableObject — 코드 목록 관리
│   └── TimelineEvent.cs          # 타임라인 이벤트 (코드명 + 박자)
├── Fretboard/
│   ├── FretboardRenderer.cs      # 지판 렌더링 + HighlightChord()
│   ├── FretMarker.cs             # 2D Sprite 마커 (시안↔노란색 펄스)
│   └── FretboardConfig.cs        # ScriptableObject — 색상 설정
├── Timing/
│   └── TimingEngine.cs           # DSP 기반 비트/마디 타이밍 엔진
├── Session/
│   ├── PracticeSessionController.cs  # 세션 시작/중지, 마디 전환
│   └── LocalStorageManager.cs        # WebGL localStorage / PlayerPrefs
├── UI/
│   ├── UIManager.cs              # 코드명 텍스트 + 비트 인디케이터 업데이트
│   ├── TransportControls.cs      # Play/Stop 버튼 컨트롤러
│   ├── BPMControl.cs             # BPM 슬라이더 + 텍스트
│   ├── ChordProgressionPanel.cs  # 코드 슬롯 행 (현재 코드 하이라이트)
│   ├── ChordSelector.cs          # 드롭다운 4개 — 커스텀 코드 진행 선택
│   └── ChordInfoPanel.cs         # AI 코드 설명 패널 (Phase 3)
├── API/
│   └── AIClient.cs               # FastAPI HTTP 통신
└── Editor/
    ├── ChordDataCreator.cs        # ChordDatabase 에셋 생성 도구
    ├── UpdateFretboardConfig.cs   # FretboardConfig 색상 업데이트
    ├── ChordSlotCreator.cs        # ChordSlot 프리팹 자동 생성
    ├── SetupSceneReferences.cs    # Inspector 참조 자동 연결
    └── SetupUIControls.cs         # Canvas UI 전체 자동 생성
```

---

## 3. 세션 진행 순서 (처음부터 끝까지)

### Step 1 — Sub-agent 구성

`.claude/agents/` 에 3개 에이전트 파일 작성:

| 에이전트 | 역할 | 상태 |
|---|---|---|
| `fingering-coach-agent` | Unity UI 패널, DTO, 코칭 화면 | ✅ MVP 활성 |
| `chord-recognizer-agent` | ML 모델, 코드 인식, 피처 추출 | ⏸ Phase 4 예정 |
| `hardware-bridge-agent` | MQTT/BLE/Serial 브릿지, LED 제어 | ⏸ Phase 5 예정 |

각 에이전트 파일 위치: `/home/user/projects/Guitar_Project/.claude/agents/`

---

### Step 2 — mcp-unity 설치 및 WSL2 연결

**설치:**
- Unity Package Manager에서 `com.gamelovers.mcp-unity@2b22ab96c905` 설치
- MCP 서버: Node.js stdio 방식, WebSocket 포트 **8090**

**문제: WSL2 → Windows WebSocket ECONNREFUSED**

Unity가 IPv6 loopback `[::1]:8090`에만 바인딩 → WSL2에서 접근 불가

**해결:** `ProjectSettings/McpUnitySettings.json`에서 `AllowRemoteConnections: true` 설정
→ Unity가 `0.0.0.0:8090`으로 바인딩하여 WSL2에서 접근 가능

```json
// /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/ProjectSettings/McpUnitySettings.json
{
  "Port": 8090,
  "AllowRemoteConnections": true,
  "AutoStartServer": true
}
```

**Claude Code MCP 설정:**
```bash
claude mcp add mcp-unity -- node \
  /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/Library/PackageCache/\
com.gamelovers.mcp-unity@2b22ab96c905/Server~/build/index.js
```

---

### Step 3 — 씬 구조 파악 (mcp-unity로 Inspector 읽기)

`mcp__mcp-unity__get_scene_info` + `get_gameobject`로 씬 분석:

**MainScene 루트 오브젝트 (7개):**
```
MainScene
├── Main Camera
├── EventSystem
├── TimingEngine          ← TimingEngine.cs
├── GameManager           ← PracticeSessionController.cs
├── Fretboard             ← FretboardRenderer.cs
├── Canvas                ← UIManager.cs (비활성), CanvasScaler, GraphicRaycaster
└── DirectionalLight
```

**확인된 연결 상태:**
- `PracticeSessionController`: timingEngine, fretboardRenderer, chordDatabase, progressionPanel 모두 연결됨
- `ChordProgressionPanel`: chordSlotPrefab=null, slotsParent=null (미연결)
- `UIManager`: 전체 null (비활성)

---

### Step 4 — ChordSlot 프리팹 생성

**문제 발생:** `ChordSlotCreator.cs` 작성 후 `[MenuItem("Guitar/Create ChordSlot Prefab")]`가 Unity 메뉴에 나타나지 않음

**원인:** 스크립트를 WSL에서 직접 생성했을 때 `.meta` 파일이 없어 Unity가 스크립트를 인식하지 못함

**해결:**
```
Assets/Refresh 메뉴 실행 → .meta 파일 자동 생성 → 컴파일 → 메뉴 등록
```

**생성 결과:**
- `Assets/Prefabs/ChordSlot.prefab` — Image (배경) + TextMeshProUGUI 자식

---

### Step 5 — SlotsParent 생성 및 참조 연결

**SlotsParent 생성** (Canvas/ChordProgressionPanel/SlotsParent):
- `RectTransform`: stretch-fill, offset 10px
- `HorizontalLayoutGroup`: spacing=10, MiddleCenter

**참조 연결 문제:** `update_component`로 Unity Object 참조(프리팹, Transform)를 직접 설정 불가

**해결:** `SetupSceneReferences.cs` Editor 스크립트 작성 — `SerializedObject` API 사용

```csharp
// SerializedObject로 private [SerializeField] 필드 접근
SerializedObject so = new SerializedObject(panel);
so.FindProperty("chordSlotPrefab").objectReferenceValue = prefab;
so.FindProperty("slotsParent").objectReferenceValue = slotsParent;
so.ApplyModifiedProperties();
```

**실행:** `Guitar/Setup Scene References` 메뉴 → 참조 연결 완료

---

### Step 6 — TMP Essential Resources 임포트

**문제:** `TextMesh Pro Essential Resources are missing` 에러

**해결:**
1. Unity TmpImporter 알림에서 "Import" 클릭 (TMP Examples & Extras)
2. `Window/TextMeshPro/Import TMP Essential Resources` 실행

---

### Step 7 — Canvas UI 전체 생성 (SetupUIControls.cs)

`SetupUIControls.cs` Editor 스크립트로 전체 UI를 코드로 자동 생성:

**생성된 UI 구조:**
```
Canvas
├── ChordProgressionPanel (기존, full-canvas 반투명 배경)
│   └── SlotsParent (HorizontalLayoutGroup — 코드 슬롯 행)
│
├── ControlBar (하단 고정, anchor y:0→0 height:120px, 반투명 어두운 배경)
│   ├── BPMControl  [BPMControl.cs]
│   │   ├── BPMSlider (range 40~240, default 80, 시안색 fill/handle)
│   │   └── BPMText  (TextMeshPro "80 BPM")
│   └── TransportControls  [TransportControls.cs]
│       ├── PlayButton  (초록 #267B27, "▶  Play")
│       └── StopButton  (빨강 #8C2626, "■  Stop")
│
└── HUD (우측 상단, anchor 0.72~1.0 x 0.82~1.0)
    ├── ChordNameText  (TextMeshPro "--", 시안 36pt Bold)
    └── BeatRow  (HorizontalLayoutGroup)
        ├── Beat1 ─┐
        ├── Beat2  │  22×22px 회색 원형 Image
        ├── Beat3  │  (비트 박자마다 스케일 펄스)
        └── Beat4 ─┘
```

**연결된 참조 전체 목록:**

| 컴포넌트 | 필드 | 연결 대상 |
|---|---|---|
| `ChordProgressionPanel` | chordSlotPrefab | Assets/Prefabs/ChordSlot.prefab |
| `ChordProgressionPanel` | slotsParent | Canvas/ChordProgressionPanel/SlotsParent |
| `TransportControls` | sessionController | GameManager (PracticeSessionController) |
| `TransportControls` | bpmControl | ControlBar/BPMControl |
| `TransportControls` | uiManager | Canvas (UIManager) |
| `TransportControls` | playButton | ControlBar/TransportControls/PlayButton |
| `TransportControls` | stopButton | ControlBar/TransportControls/StopButton |
| `BPMControl` | bpmSlider | ControlBar/BPMControl/BPMSlider |
| `BPMControl` | bpmText | ControlBar/BPMControl/BPMText |
| `BPMControl` | timingEngine | TimingEngine |
| `UIManager` | sessionController | GameManager |
| `UIManager` | timingEngine | TimingEngine |
| `UIManager` | chordNameText | HUD/ChordNameText |
| `UIManager` | bpmValueText | ControlBar/BPMControl/BPMText |
| `UIManager` | beatIndicators[0~3] | HUD/BeatRow/Beat1~4 |

**메뉴 실행:** `Guitar/Setup UI Controls`

---

### Step 8 — 씬 저장

`mcp__mcp-unity__save_scene` → `Assets/MainScene.unity` 저장 완료

---

## 4. 현재 완성 상태

### Unity 씬 (MainScene)

| 항목 | 상태 |
|---|---|
| TimingEngine DSP 타이밍 | ✅ 완성 |
| FretboardRenderer 지판 렌더링 | ✅ 완성 |
| FretMarker 시안↔노란 펄스 애니메이션 | ✅ 완성 |
| ChordDatabase (C, G, Am, F, D 5개) | ✅ 완성 |
| PracticeSessionController | ✅ 완성 + 참조 연결됨 |
| ChordProgressionPanel | ✅ 완성 + prefab/slotsParent 연결됨 |
| ChordSlot 프리팹 | ✅ 생성 완료 |
| ControlBar (BPMControl + TransportControls) | ✅ 생성 + 참조 연결됨 |
| HUD (ChordNameText + BeatIndicators) | ✅ 생성 + 연결됨 |
| UIManager | ✅ 활성화 + 모든 참조 연결됨 |
| TMP Essential Resources | ✅ 임포트 완료 |
| 씬 저장 | ✅ MainScene.unity |

### 사용 가능한 Guitar 메뉴

| 메뉴 항목 | 역할 |
|---|---|
| `Guitar/Create ChordSlot Prefab` | ChordSlot 프리팹 재생성 |
| `Guitar/Setup Scene References` | ChordProgressionPanel 참조 재연결 |
| `Guitar/Setup UI Controls` | Canvas UI 전체 재생성 + 참조 연결 |
| `Guitar/Update FretboardConfig Colors` | 지판 색상 업데이트 |

---

## 5. 실행 흐름 (Play Mode)

```
① Play 버튼 클릭
   └─ TransportControls.OnPlayClicked()
      └─ bpmControl.GetCurrentBPM()  →  int bpm
      └─ sessionController.StartDefaultSession(bpm)
         └─ TimingEngine.StartPlayback(4마디)
         └─ 첫 코드(C) 즉시 표시

② 마디 시작마다 (TimingEngine.OnMeasureStart 이벤트)
   └─ PracticeSessionController.OnMeasureChanged(measureIndex)
      └─ ChordDatabase.GetChord(chordName)
      └─ FretboardRenderer.HighlightChord(chordData)  → 지판 점등
      └─ ChordProgressionPanel.SetActiveChord(index)  → 슬롯 하이라이트

③ UIManager.Update() 매 프레임
   └─ TimingEngine.CurrentBeat 읽기
   └─ beatIndicators[currentBeat] 스케일 펄스

④ Stop 버튼 클릭
   └─ sessionController.StopSession()
      └─ TimingEngine.StopPlayback()
      └─ FretboardRenderer.ClearAllHighlights()
   └─ uiManager.ResetIndicators()
```

---

## 6. 다음 단계

### 즉시 가능 (Unity에서 Play Mode 테스트)
- Play Mode 진입 → Play 버튼 클릭 → C→G→Am→F 루프 + 지판 점등 확인

### 단기 (MVP 완성)
- **ChordSelector UI 추가**: 드롭다운 4개 + Apply 버튼으로 커스텀 코드 진행 선택
  - `ChordSelector.cs`는 이미 구현됨 — UI 오브젝트 생성 및 ChordDatabase 연결 필요
- **WebGL 빌드**: File → Build Settings → WebGL → Build
- **Docker 서빙**: `docker compose restart` → `http://localhost:8000/practice`

### 중기 (Phase 3 — fingering-coach-agent)
- SongData ScriptableObject (곡 이름 + 코드 진행 프리셋)
- 튜너 통합 (iPhone 마이크 → 현재 코드 일치 여부 표시)
- `/api/recommend-fingering` 엔드포인트 + 코칭 메시지 UI

### 장기
- **Phase 4**: ML 코드 인식 (`chord-recognizer-agent`)
- **Phase 5**: LED 하드웨어 연동 (`hardware-bridge-agent`)

---

## 7. 발생 이슈 및 해결 기록

| 이슈 | 원인 | 해결 |
|---|---|---|
| WSL2 → Unity WebSocket ECONNREFUSED | Unity가 IPv6 loopback만 바인딩 | `AllowRemoteConnections: true` 설정 |
| `[MenuItem]` 메뉴가 등록 안 됨 | `.meta` 파일 없어 Unity가 스크립트 미인식 | `Assets/Refresh` 실행으로 `.meta` 생성 |
| `update_component`로 Object 참조 설정 불가 | MCP가 Unity Object 타입 직렬화 미지원 | `SerializedObject` API 사용하는 Editor 스크립트 작성 |
| TMP 텍스트 빈칸 표시 | TMP Essential Resources 미임포트 | `Window/TextMeshPro/Import TMP Essential Resources` |
| `▶` 문자 폰트 경고 | LiberationSans SDF에 해당 유니코드 없음 | 기능 무관, 무시 가능 (또는 "Play"만 사용) |

---

## 8. 환경 정보

| 항목 | 값 |
|---|---|
| OS | WSL2 (Ubuntu) + Windows |
| Unity | 6.3 LTS |
| Unity 프로젝트 경로 | `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/` |
| mcp-unity 패키지 | `com.gamelovers.mcp-unity@2b22ab96c905` |
| MCP WebSocket 포트 | 8090 |
| Python 패키지 관리 | uv (`uv pip install`, bare pip 금지) |
| Docker | `docker compose restart` |
| 서비스 포트 | 8000 (Express), 8080 (FastAPI) |
