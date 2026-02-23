# Guitar Practice App — Gemini CLI Instructions

이 파일은 Gemini CLI가 Guitar Practice 프로젝트를 이해하고 작업하기 위한 프로젝트 컨텍스트, 에이전트 역할, 코딩 규칙을 정의합니다.

---

## 프로젝트 개요

기타 연습을 돕는 3가지 컴포넌트로 구성된 풀스택 앱입니다.

| 컴포넌트 | 경로 | 설명 |
|---------|------|------|
| Unity WebGL App | `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/` | 인터랙티브 기타 지판 UI (C#) |
| FastAPI 백엔드 | `./api/main.py` | AI 코드 추천 엔드포인트 |
| Tunerapp | `./Tunerapp/` | 웹 기반 기타 튜너 (Express + WebAudio) |

**배포:** Docker Compose, 포트 8000. Unity WebGL 빌드는 정적 파일로 서빙.

---

## 필수 경로 규칙

### Unity 스크립트 경로
- **항상 이 경로 사용:** `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/Assets/Scripts/`
- **절대 사용 금지:** `/home/user/projects/Guitar_Project/UnityApp/` (구버전, 미사용)
- C# 파일 수정 후 반드시 Unity 재컴파일 확인

### Python 패키지 관리
- **항상 `uv pip` 사용** — bare `pip` 절대 금지
- 설치: `uv pip install <package>`
- PyTorch (CUDA 12.8): `uv pip install torch --index-url https://download.pytorch.org/whl/cu128`

### 서버 재시작
- `server.js` 수정 시: `docker compose restart`
- `index.html` 수정 시: 재시작 불필요 (정적 파일)

---

## 저장소 구조

```
Guitar_Project/
├── Tunerapp/
│   ├── index.html          # 튜너 UI (WebAudio 피치 감지)
│   └── server.js           # Express 서버 (포트 8000)
├── api/
│   └── main.py             # FastAPI AI 엔드포인트
├── docker-compose.yml
├── CLAUDE.md               # 프로젝트 컨텍스트 (정식 참조 문서)
├── codex/
│   └── AGENTS.md           # OpenAI Codex CLI 지침
└── gemini/
    └── GEMINI.md           # 이 파일 (Gemini CLI 지침)

Unity 프로젝트: /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/
└── Assets/Scripts/
    ├── Fretboard/
    │   ├── FretboardRenderer.cs    # 6현 지판 빌드 + 코드 전환 애니메이션
    │   ├── FretMarker.cs           # 프렛별 스프라이트 마커 (상태 머신)
    │   ├── FretboardConfig.cs      # ScriptableObject (색상, 간격 설정)
    │   └── FretMarkerPulse.cs      # 활성 마커 펄스 애니메이션
    ├── Session/
    │   ├── PracticeSessionController.cs  # 세션 라이프사이클, 코드 진행 루프
    │   ├── TimingEngine.cs               # DSP 기반 비트 타이밍
    │   ├── ChordDatabase.cs              # ScriptableObject (ChordData 목록)
    │   └── ChordData.cs                  # 코드명 + 운지 위치 + 손가락 번호
    ├── UI/
    │   ├── ChordSelector.cs        # 코드 진행 선택기 (1–8 슬롯, 클릭 순환)
    │   ├── ChordProgressionPanel.cs # 현재 진행 시각적 표시
    │   ├── BPMControl.cs           # BPM 슬라이더 UI
    │   ├── TransportControls.cs    # Play / Stop 버튼
    │   └── UIManager.cs            # 인디케이터 리셋, UI 상태 관리
    ├── API/
    │   └── AIClient.cs             # FastAPI 백엔드 HTTP 클라이언트
    └── Editor/
        ├── ChordDataCreator.cs         # Guitar 메뉴: ChordDatabase 자동 생성
        ├── SetupChordSelector.cs       # Guitar 메뉴: SelectorBar UI 자동 생성
        └── SetupUIControls.cs          # Guitar 메뉴: ControlBar UI 자동 생성
```

---

## 현재 구현 상태 (2026-02-22 기준)

### Unity WebGL 앱

**FretMarker 상태 머신:**
```
Inactive → Preview → Transitioning → Settling → Active
```
- `Inactive`: 거의 투명한 회색 `(0.15, 0.15, 0.15, 0.3)`
- `Active`: 밝은 시안 `(0, 1, 1, 1)` + 펄스 애니메이션
- `Preview`: 어두운 시안 (다음 코드 예고, 비트 3에 표시)
- `Transitioning/Settling`: 크로스페이드 전환 중간 상태

**핵심 공개 API (절대 변경 금지):**
```csharp
// FretboardRenderer.cs
void HighlightChord(ChordData chord)            // 즉시 코드 하이라이트
void TransitionToChord(ChordData next, float duration)  // 크로스페이드 전환
void PreviewNextChord(ChordData nextChord)      // 다음 코드 예고 표시
void ClearAllHighlights()                       // 전체 초기화

// FretMarker.cs
void SetActive(bool active)
void SetPreview()
void CrossfadeToActive(float duration)
void CrossfadeToInactive(float duration)
```

**ChordDatabase (ScriptableObject):** 9개 코드 등록
- C, G, Am, F, D, Em, A, E, Dm

**ChordSelector UI (SelectorBar):**
- 위치: y=122–170px, 화면 왼쪽 65%
- 코드 슬롯: 클릭 → 다음 코드로 순환 (코드명 기반, 인덱스 아님)
- Add/Remove 버튼: 슬롯 1–8개 동적 조절
- Apply 버튼: 세션 정지 → 진행 업데이트 → 세션 재시작

**Canvas 레이아웃 (1151×552 기준):**
```
y = 470–552  : 상단 바 (안전 UI 영역)
y = 122–170  : SelectorBar (코드 선택기)
y = 0–120    : ControlBar (Play/Stop/BPM)
중앙 전체     : 지판 영역 (UI 패널이 가리면 안 됨)
```

### FastAPI 백엔드
- 계획된 엔드포인트: `POST /api/recommend-fingering`
- 현재: 최소 구현 (ML 모델 미배포)

### Tunerapp
- 오토코릴레이션 피치 감지, 기타 범위(70–1400Hz) 최적화
- `fftSize = 8192`, RMS 임계값 = 0.001
- 알려진 한계: 1–2번 줄(얇은 줄) 감지율이 낮음

---

## 에이전트 역할 정의

3개의 전문 서브에이전트가 시스템의 각기 다른 부분을 담당합니다. 메인 Gemini 세션은 오케스트레이터 역할을 합니다.

---

### 에이전트 1: fingering-coach-agent
**상태:** 활성 (MVP Phase 1)

**담당 범위:**
- Unity UI 패널 수정 및 신규 생성
- 백엔드 응답 DTO 정의:
  - `suggested_fingering`: 코드명 + 운지 위치 + 손가락 번호
  - `coaching_messages`: 우선순위 기반 코칭 메시지 목록
  - `next_exercise`: 다음 연습 (선택적)
- 경량 텔레메트리 이벤트 (PII 없음): `chord_completed`, `retry_count`, `common_error_type`
- `Assets/Scripts/UI/` 내 C# 스크립트

**이 에이전트를 사용할 상황:**
- 화면 레이아웃 변경, UI 컴포넌트 추가/수정
- API 응답 형태 변경, DTO 구조 정의
- 코칭 메시지, 운지법 힌트, 다음 코드 표시 기능
- 텔레메트리 이벤트 추가

**이 에이전트 범위 밖:**
- ML 모델, 피처 추출 → chord-recognizer-agent
- 하드웨어/LED/MQTT → hardware-bridge-agent

**UI 검증 의무 (작업 완료 전 필수):**
1. 컴파일 오류 0건 확인
2. RectTransform anchor/offset 값이 의도한 대로인지 수치 확인
3. 새 UI 패널이 지판(화면 중앙)을 가리지 않는지 확인
4. 모든 serialized field가 null이 아닌지 확인
5. 완료 보고 형식:
   ```
   ## 검증 결과
   - [ ] 컴파일 오류: 0건
   - [ ] 레이아웃 겹침: 없음
   - [ ] 참조 연결: 전체 연결됨
   - [ ] 동작 확인: (확인 내용 기술)
   ```

---

### 에이전트 2: chord-recognizer-agent
**상태:** 비활성 (Phase 4 / MVP 이후 예정)

**담당 범위:**
- FastAPI 엔드포인트: `POST /api/recommend-fingering`
- 오디오 피처 추출 유틸리티
- 모델 정확도 오프라인 평가 스크립트
- 결정론적 휴리스틱 우선; ML은 학습 데이터 확보 후 적용

**이 에이전트를 사용할 상황:**
- 코드 인식 기능 구현, 음원 분석
- ML 모델 통합, 피처 추출
- 운지법 추천 API 구현, 정확도 측정

**이 에이전트 범위 밖:**
- Unity UI → fingering-coach-agent
- 하드웨어 → hardware-bridge-agent

---

### 에이전트 3: hardware-bridge-agent
**상태:** 비활성 (Phase 5 / MVP 이후 예정)

**담당 범위:**
- 하드웨어 브릿지 모듈 (MQTT 우선, BLE 또는 Serial 선택)
- 엔드포인트: `POST /api/hardware/push-frame`, `POST /api/hardware/clear`
- 보안: 디바이스 ID 허용 목록, 레이트 리밋, 원격 호출 인증 토큰
- FretMarker 상태 → LED 프레임 미러링

**이 에이전트를 사용할 상황:**
- LED 제어, 하드웨어 연동
- MQTT/BLE/Serial 통신 구현
- `/api/hardware` 엔드포인트, 보안 검토

**이 에이전트 범위 밖:**
- Unity UI → fingering-coach-agent
- ML/코드 인식 → chord-recognizer-agent

**주의:** 보안 검토 및 프로토콜 설계 판단이 포함되므로 가장 강력한 모델 사용 권장.

---

## 오케스트레이터 행동 규칙

### 서브에이전트에 위임하기 전
1. 관련 파일을 직접 읽은 후 위임 — 파일 내용을 모르는 채로 에이전트에 넘기지 않음
2. 위임 프롬프트에 실제 코드를 포함 — 에이전트가 추측하지 않도록
3. 확장 시나리오를 프롬프트에 명시:
   - 곡 추가 → 동일한 애니메이션 API(`HighlightChord`/`TransitionToChord`) 재사용
   - 마이크 코드 인식 → 인식된 코드를 `HighlightChord`로 표시
   - 하드웨어 LED → `FretMarker` 상태를 LED 브릿지에 그대로 미러링

### 서브에이전트 결과 수신 후 (필수)
1. 에이전트가 수정한 **모든 파일**을 직접 읽기 — 완료 메시지만 믿지 않음
2. Unity C# 변경 시 반드시 재컴파일 후 에러 0건 확인
3. 문제 발견 시 직접 수정 — 재위임 루프 만들지 않음
4. 기존 공개 API가 깨지지 않았는지 확인

### 작업 완료 조건
아래를 모두 확인한 뒤에만 완료 처리:
1. 수정된 파일 직접 읽고 로직 정확성 검증
2. Unity 스크립트: 컴파일 에러 0개
3. 기존 공개 API 호환성 유지
4. 미래 확장 시나리오에서 코드 재사용 가능 여부 확인

---

## 아키텍처 원칙

- **Unity** = UI + 인터랙션 레이어
- **FastAPI** = 로직 레이어 (추천, 세션 로깅, 향후 LED 브릿지 + ML 추론)
- 네트워크 I/O는 안정적인 인터페이스 뒤에 격리 (현재 HTTP)
- UI와 AI 네트워크 호출 간 강한 결합 금지
- JSON 통신 시 DTO 사용, 하드웨어 확장을 위한 하위 호환 필드 유지

---

## 코딩 규칙

### Unity C#
- 새 파일 최상단에 한 줄 설명 주석:
  ```csharp
  // ChordSelector.cs - 코드 진행 선택 UI (개수 동적 조절 1~8)
  ```
- 코드명은 인덱스 대신 문자열로 저장 (DB 순서 변경 시 인덱스 불일치 버그 방지)
- 루프 내 람다 클로저: 반드시 루프 변수를 캡처:
  ```csharp
  int captured = i;
  btn.onClick.AddListener(() => DoSomething(captured));
  ```
- `Awake()`: 다른 컴포넌트가 의존하는 초기화 로직
- `Start()`: 다른 컴포넌트를 필요로 하는 로직
- HorizontalLayoutGroup + 동적 자식: `childForceExpandHeight = true` 설정 (미설정 시 height=0, 클릭 불가)
- ContentSizeFitter + ScrollRect 조합 지양 (런타임에 버튼 invisible 버그 발생)

### Python
- 새 파일 최상단에 한 줄 설명 주석:
  ```python
  # main.py - FastAPI 백엔드: 코드 추천 및 세션 로깅
  ```
- 시스템 경계(API 엔드포인트)에서만 입력 검증
- HTTP 상태 코드와 함께 의미 있는 에러 메시지
- Raw 오디오 저장 금지 (명시적 privacy filter 없이)

---

## 데이터 & 프라이버시

- 세션 로그: 필요한 것만 저장
- Raw 오디오: 명시적 동의 + privacy filter 없이 저장 금지
- 텔레메트리 이벤트에 PII 포함 금지

---

## 알려진 문제 및 주의사항

| 문제 | 원인 | 해결책 |
|------|------|--------|
| Editor 메뉴 항목이 나타나지 않음 | Unity가 아직 스크립트를 컴파일하지 않음 | Assets > Refresh 또는 Unity 재컴파일 대기 |
| FretboardConfig 색상 변경이 기존 에셋에 미반영 | ScriptableObject 자동 업데이트 안 됨 | `Guitar > Update FretboardConfig Colors` 메뉴 실행 |
| SelectorBar 슬롯이 잘못된 코드 표시 | 인덱스 기반 접근법 → DB 순서 변경 시 불일치 | `selectedNames List<string>` (코드명 직접 저장)으로 해결됨 |
| 버튼 클릭 영역 없음 | `childForceExpandHeight = false` + LayoutElement height 미설정 → height=0 | `childForceExpandHeight = true` 설정 |
| ScrollRect 내 버튼 런타임에 안 보임 | ContentSizeFitter+ScrollRect 계층 구조 문제 | ScrollRect 제거, 단순 HorizontalLayoutGroup 사용 |

---

## 향후 로드맵

| Phase | 작업 | 담당 에이전트 | 상태 |
|-------|------|-------------|------|
| Phase 1 (현재) | Unity UI, 코드 선택기, 코칭 화면 | fingering-coach-agent | ✅ 활성 |
| Phase 2 | WebGL 빌드 + Docker 배포 | 메인 세션 | ⏸ 예정 |
| Phase 3 | AI 코드 추천 기본 구현 | 메인 세션 + chord-recognizer-agent | ⏸ 예정 |
| Phase 4 | ML 코드 인식, /api/recommend-fingering | chord-recognizer-agent | ⏸ 예정 |
| Phase 5 | LED 하드웨어 브릿지 | hardware-bridge-agent | ⏸ 예정 |
