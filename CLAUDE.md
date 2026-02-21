# Guitar Project - 작업 컨텍스트

## 프로젝트 구조

```
Guitar_Project/
├── Tunerapp/          # 기타 튜너 웹앱 (Express + WebAudio)
│   ├── index.html     # 튜너 메인 (피치 감지 포함)
│   └── server.js      # Express 서버 (포트 8000)
├── api/               # FastAPI 백엔드
│   └── main.py        # AI 코드 추천 엔드포인트
├── docker-compose.yml # 전체 서비스 오케스트레이션
└── UnityApp/          # (구) Unity 앱 경로 - 현재 미사용
```

**Unity 프로젝트 실제 경로 (Windows):**
`C:\Projects\Project_Guitar\GuitarPracticeUnity\`
→ WSL 경로: `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/`

---

## 현재 완료된 작업

### 1. Tunerapp (기타 튜너)
- **iOS Safari 대응**: `echoCancellation`, `noiseSuppression`, `autoGainControl` 모두 OFF
- **AudioContext**: `window.AudioContext || window.webkitAudioContext` 폴백 추가
- **피치 감지 알고리즘**: 기타 주파수 범위(70~1400Hz)만 탐색하는 최적화된 autocorrelation
  - `fftSize = 8192` (저음 E2=82Hz 감지용)
  - `RMS 임계값 = 0.001` (iPhone 마이크 민감도 대응)
  - O(n²) 전체 → O(n × 600lag) 로 대폭 단축
- **캐시 방지**: `/tuner` 경로에 `Cache-Control: no-store`
- **디버그 패널**: RMS 실시간 표시 (마이크 시작 시 화면 하단에 표시)

**알려진 한계:**
- 1~2번 줄(얇은 줄) 인식률이 4~6번보다 낮음 (에너지 차이)
- 옥타브 오류 간혹 발생
- 피치 표시가 프레임마다 튐 (스무딩 없음)

**향후 개선 계획 (논의됨, 미구현):**
- YIN 알고리즘 교체 (옥타브 오류 감소)
- 시간축 스무딩 (최근 5~8프레임 평균) → 체감 개선 효과 큼
- 고역 이중 경로 (얇은 줄 전용 highpass 게이트, 컷오프 150Hz)

### 2. Unity WebGL 기타 연습 앱
**경로:** `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/Assets/Scripts/`

**완료된 구현:**
- `FretboardRenderer.cs` - 지판 렌더링 (Awake()에서 초기화, Start() 아님)
- `FretMarker.cs` - 2D Sprite 기반 마커, 시안↔노란색 펄스 애니메이션
- `FretboardConfig.cs` - ScriptableObject 설정
  - inactive: `(0.15, 0.15, 0.15, 0.3)` 거의 투명
  - active: `(0, 1, 1, 1)` 밝은 시안
  - glow: `(1, 1, 0, 1)` 노란색
- `TimingEngine.cs` - DSP 기반 비트 타이밍 엔진
- `PracticeSessionController.cs` - C 코드 하이라이트 테스트 동작 확인됨
- `ChordDatabase` ScriptableObject - C, G, Am, F, D 코드 5개 등록

**Unity 씬 구조 (MainScene):**
```
MainScene
├── Main Camera
├── TimingEngine
├── GameManager (PracticeSessionController 부착)
├── Fretboard (FretboardRenderer 부착)
└── Canvas
    └── ChordProgressionPanel
```

**현재 상태:** C 코드 하이라이트 동작 확인. Inspector에서 FretboardConfig 색상 수동 업데이트 필요 (Guitar → Update FretboardConfig Colors 메뉴 사용)

**다음 작업:**
- UI 컨트롤 구현 (BPM 슬라이더, Play/Stop 버튼)
- 자동 코드 진행 (C→G→Am→F 루프)
- WebGL 빌드 후 Docker 배포

### 3. FastAPI 백엔드
- 대체 운지법, 맞춤형 루틴 기능 제거됨
- `/mnt/c/.../Builds/WebGL` → Docker volume mount로 Unity WebGL 서빙

---

## 환경 정보

- **OS**: WSL2 (Linux) + Windows
- **Unity**: 6.3 LTS, 3D 프로젝트 (2D Sprite 렌더링 사용)
- **Python**: uv 사용 (`uv pip` 필수, bare `pip` 금지)
- **Docker**: `docker compose restart` 로 서버 재시작
- **포트**: 8000 (Express + Unity WebGL + Tuner 통합)

## 주의사항

- Unity 스크립트 수정 시 **반드시 Windows 경로** 사용
  - ✅ `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/...`
  - ❌ `/home/user/projects/Guitar_Project/UnityApp/...` (구버전, 미사용)
- Tunerapp `index.html` 수정은 Docker 재시작 불필요 (정적 파일)
- `server.js` 수정은 `docker compose restart` 필요
- FretboardConfig 색상 변경 시 기존 에셋에 자동 반영 안 됨 → Inspector에서 직접 수정 또는 Guitar 메뉴 사용

---

## 아키텍처 원칙

- **Unity** = UI + 인터랙션 레이어
- **Backend (FastAPI)** = 로직 레이어: 추천, 세션 로깅, (향후) LED 브릿지 + ML 추론
- 네트워크 I/O는 안정적인 인터페이스 뒤에 격리 (현재 HTTP, MCP Unity 연결 완료)

### 코딩 규칙 (Unity)

- UI와 AI 네트워크 호출 간 강한 결합 금지
- JSON 통신 시 DTO 사용, 향후 하드웨어 확장을 위한 하위 호환 필드 유지
- 새 엔드포인트 추가 시 필수 항목:
  - request/response 스키마
  - 에러 코드
  - 레이턴시 예산 (로컬 < 200ms, 원격 < 500ms)

### 데이터 & 프라이버시

- 세션 로그: 필요한 것만 저장 (명시적으로 허용하지 않는 한 raw audio 저장 금지)
- 로그 기록 전 privacy filter 함수 적용

---

## 서브에이전트 위임 프로토콜

| 작업 | 담당 에이전트 | 상태 |
|---|---|---|
| DTO 스키마 + 엔드포인트 계약 | `fingering-coach-agent` | ✅ MVP 활성 |
| ML/휴리스틱 추천 로직 | `chord-recognizer-agent` | ⏸ Phase 3 예정 |
| 하드웨어 커맨드 프로토콜 & 보안 | `hardware-bridge-agent` | ⏸ Phase 3 예정 |
