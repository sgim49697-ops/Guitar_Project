# 🎸 Guitar Practice Project

기타 튜너 + AI 기반 연습 앱 통합 프로젝트

## 📁 프로젝트 구조

```
Guitar_Project/
├── Tunerapp/           # 기타 튜너 (Node.js + WebSocket)
│   ├── server.js
│   ├── input.html      # iPhone 마이크 입력
│   ├── display.html    # 컴퓨터 디스플레이
│   └── index.html      # 독립형 튜너
│
├── UnityApp/           # Unity WebGL 연습 앱
│   └── Assets/Scripts/
│       ├── Data/       # 코드 데이터 정의
│       ├── Fretboard/  # 지판 렌더링
│       ├── Timing/     # DSP 기반 타이밍 엔진
│       ├── Session/    # 연습 세션 관리
│       ├── UI/         # 인터페이스
│       └── API/        # FastAPI 통신
│
├── api/                # FastAPI AI 서버
│   ├── main.py
│   ├── Dockerfile
│   └── pyproject.toml  # uv 패키지 관리
│
└── docker-compose.yml  # 전체 서비스 오케스트레이션
```

---

## 🚀 빠른 시작 (Docker)

### 1️⃣ 사전 준비

- **Docker Desktop** 설치 (Windows/WSL2)
- **uv** 설치 (선택사항, Python 패키지 관리)
  ```bash
  curl -LsSf https://astral.sh/uv/install.sh | sh
  ```

### 2️⃣ 서비스 시작

```bash
# WSL2에서 실행
cd /home/user/projects/Guitar_Project

# 모든 서비스 시작 (Node.js + FastAPI + MongoDB)
docker compose up -d

# 로그 확인
docker compose logs -f
```

### 3️⃣ 접속

| 서비스 | URL | 설명 |
|--------|-----|------|
| 🎵 튜너 | http://localhost:8000/tuner | 기타 튜너 앱 |
| 🎸 연습앱 | http://localhost:8000/practice | Unity WebGL 연습 앱 |
| 🤖 AI API | http://localhost:8080/docs | FastAPI Swagger UI |
| 📊 MongoDB UI | http://localhost:8081 | Mongo Express |

### 4️⃣ 서비스 종료

```bash
# 전체 종료
docker compose down

# 데이터까지 삭제
docker compose down -v
```

---

## 🛠 로컬 개발 (Docker 없이)

### Node.js 서버

```bash
cd Tunerapp
npm install
npm start
```

### FastAPI 서버 (uv 사용)

```bash
cd api

# uv로 의존성 설치
uv sync

# 서버 실행
uv run uvicorn main:app --reload --port 8080
```

### FastAPI 서버 (pip 사용)

```bash
cd api

# 가상환경 생성
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate

# 의존성 설치
pip install -r requirements.txt

# 서버 실행
uvicorn main:app --reload --port 8080
```

---

## 🎮 Unity 개발

### Unity 프로젝트 열기 (Windows)

1. **Unity Hub** 설치 (https://unity.com/download)
2. **Unity 2022.3 LTS** 설치 (WebGL Build Support 모듈 포함)
3. Unity Hub에서 `UnityApp/` 프로젝트 열기

### WebGL 빌드

1. Unity Editor에서 **File → Build Settings**
2. **WebGL** 플랫폼 선택 → **Switch Platform**
3. **Build** 클릭 → 출력 경로: `UnityApp/Builds/WebGL/`
4. 빌드 완료 후 서버 재시작

---

## 🤖 AI 기능

### API 엔드포인트

| 엔드포인트 | 메서드 | 설명 |
|-----------|--------|------|
| `/api/explain-chord` | POST | 코드 설명 생성 |
| `/health` | GET | 헬스체크 |

### 예시: 코드 설명 요청

```bash
curl -X POST http://localhost:8080/api/explain-chord \
  -H "Content-Type: application/json" \
  -d '{"chord_name": "C"}'
```

### Unity에서 호출

```csharp
// AIClient 사용 예시
aiClient.ExplainChord("C",
    (response) => {
        Debug.Log(response.explanation);
    },
    (error) => {
        Debug.LogError(error);
    }
);
```

---

## 🔧 환경 변수

`.env` 파일 생성 (`.env.example` 참고):

```bash
# OpenAI API 키
OPENAI_API_KEY=sk-your-api-key

# Anthropic Claude API 키
ANTHROPIC_API_KEY=sk-ant-your-key

# MongoDB 인증
MONGO_USERNAME=admin
MONGO_PASSWORD=REDACTED
```

---

## 📱 iPhone 원격 튜너 사용 (ngrok)

```bash
# ngrok 설치 (WSL)
curl -s https://ngrok-agent.s3.amazonaws.com/ngrok.asc | \
  sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null
echo "deb https://ngrok-agent.s3.amazonaws.com buster main" | \
  sudo tee /etc/apt/sources.list.d/ngrok.list
sudo apt update && sudo apt install ngrok

# Authtoken 설정
ngrok config add-authtoken YOUR_TOKEN

# 서버 실행 후 ngrok 터널 생성
ngrok http 8000
```

생성된 URL로 iPhone에서 접속:
- 입력: `https://xxx.ngrok-free.dev/tuner/input.html`
- 디스플레이: `https://xxx.ngrok-free.dev/tuner/display.html`

---

## 🏗 아키텍처

```
┌──────────────────────────────────────────┐
│         Windows (Unity Editor)          │
│  ┌────────────────────────────────────┐ │
│  │   Unity WebGL Client               │ │
│  │   - Fretboard Renderer             │ │
│  │   - Timing Engine (DSP)            │ │
│  │   - Practice Session Controller    │ │
│  └──────────┬─────────────────────────┘ │
│             │ HTTP API                   │
└─────────────┼────────────────────────────┘
              │
┌─────────────┼────────────────────────────┐
│      WSL2   ↓    Docker Compose          │
│  ┌──────────────────────────────────┐   │
│  │ 🐳 Node.js (Express + WebSocket) │   │
│  │    → 튜너 서빙                    │   │
│  │    → Unity WebGL 서빙            │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ 🐳 FastAPI (AI 엔진)              │   │
│  │    → 코드 설명                    │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ 🐳 MongoDB                        │   │
│  │    → 사용자 데이터                │   │
│  │    → 연습 기록                    │   │
│  └──────────────────────────────────┘   │
└──────────────────────────────────────────┘
```

---

## 📝 MVP 목표

**곡(코드 진행)을 선택하면 운지법이 타이밍에 맞춰 지판에 점등되는 것**

---

## 🔍 현재 상태 진단

### ✅ C# 코드 레벨 - 완성

| 컴포넌트 | 상태 | 설명 |
|---|---|---|
| `TimingEngine` | ✅ 완성 | DSP 기반 비트/마디 타이밍 |
| `PracticeSessionController` | ✅ 완성 | 마디 바뀔 때 코드 전환 |
| `FretboardRenderer` + `FretMarker` | ✅ 완성 | 지판 렌더링 + 점등 애니메이션 |
| `ChordSelector` | ✅ 완성 | 드롭다운 4개로 코드 진행 선택 |
| `ChordProgressionPanel` | ✅ 완성 | 현재 재생 코드 하이라이트 |
| `TransportControls` | ✅ 완성 | Play/Stop 버튼 |
| `BPMControl` | ✅ 완성 | BPM 슬라이더 |

### ❌ 미완성 (Unity Editor 레벨)

- 씬에서 Inspector 연결 미완
- `ChordDatabase` ScriptableObject에 실제 운지 데이터 미입력 (5개 코드 FretPosition)
- `ChordSlot` 프리팹 미생성
- WebGL 빌드 미완

---

## 🎮 MVP 사용자 워크플로우

```
[코드 진행 선택]
  드롭다운 4개에서 코드 선택
  (C→G→Am→F 프리셋 or 직접 선택)
        ↓
[BPM 설정]
  슬라이더로 BPM 조정
        ↓
[Play 버튼]
  TimingEngine 시작
        ↓
[마디마다 자동 전환] ← 루프
  코드 A 점등 (1마디)
  코드 B 점등 (2마디)
  코드 C 점등 (3마디)
  코드 D 점등 (4마디)
        ↓
[Stop 버튼]
  지판 초기화
```

---

## 🚀 구현/배포 파이프라인

### Phase 1: Unity Editor 작업 (블로커)

코드는 완성 상태이며, Unity Editor에서 데이터 입력과 Inspector 연결이 필요합니다.

**1. ChordDatabase ScriptableObject 데이터 입력**
- 5개 코드(C, G, Am, F, D)에 FretPosition 데이터 직접 입력

**2. ChordSlot 프리팹 생성**
- Image + TextMeshPro 조합

**3. 씬 Inspector 연결 완료**

```
GameManager (PracticeSessionController)
  ├─ TimingEngine 연결
  ├─ FretboardRenderer 연결
  ├─ ChordDatabase 연결
  └─ ChordProgressionPanel 연결

Canvas
  ├─ TransportControls → PlayBtn, StopBtn 연결
  ├─ BPMControl        → Slider 연결
  ├─ ChordSelector     → Dropdown 4개 연결
  └─ ChordProgressionPanel → SlotPrefab, SlotsParent 연결
```

**4. Play Mode 테스트**
- 코드 선택 → Play → 지판 점등 확인

---

### Phase 2: 빌드 & 배포

**5. WebGL 빌드**
```
File → Build Settings → WebGL → Build
출력: GuitarPracticeUnity/Builds/WebGL/
```

**6. Docker 서빙**
```bash
docker compose restart
# 접속: http://localhost:8000/practice
```

node-server가 `/practice` 경로로 WebGL 빌드를 서빙합니다.

---

### Phase 3: 코칭 고도화 (`fingering-coach-agent` 담당)

**7. SongData ScriptableObject 추가**
- "곡 이름" + "코드 진행 프리셋" 묶음
- 드롭다운으로 "곡 선택" UX 구현

**8. 튜너 통합**
- iPhone으로 소리 내면 현재 코드와 일치 여부 표시

**9. 코칭 UI 고도화**
- 현재 코드 / 다음 코드 / 코칭 힌트 패널 표시
- 경량 텔레메트리 이벤트: `chord_completed`, `retry_count`, `common_error_type` (PII 없음)

**10. Fingering 추천 API**

| 엔드포인트 | 메서드 | 설명 |
|---|---|---|
| `/api/recommend-fingering` | POST | 운지법 추천 (휴리스틱 → ML) |

```json
// Response DTO
{
  "suggested_fingering": { "positions": [...], "finger_numbers": [...] },
  "coaching_messages": ["검지를 더 프렛 가까이", "손목 각도 조정"],
  "next_exercise": "C→G 전환 연습"
}
```

---

### Phase 4: ML 인식 (`chord-recognizer-agent` 담당)

> 데이터 수집 및 ML 인프라 준비 후 진행

**11. 코드 인식 엔진**
- 휴리스틱 기반 우선 구현 → 데이터 축적 후 ML 교체
- 피처 추출 유틸리티 (오디오 → 코드명)
- 오프라인 평가 스크립트 (정확도 측정)

**12. LLM 연동**
- OpenAI / Claude / 로컬 모델로 코드 설명 생성
- `/api/explain-chord` 하드코딩 → LLM 응답으로 교체

**13. 사용자 진행도 추적 (MongoDB)**
- 세션 로그 저장 (privacy filter 적용)
- 연습 히스토리 조회 API

---

### Phase 5: 하드웨어 연동 (`hardware-bridge-agent` 담당)

> 하드웨어 타겟(MQTT/BLE/Serial) 확정 후 진행. 보안 검토 필수.

**14. 하드웨어 브릿지 모듈**
- 전송 방식 1개 선택: MQTT (멀티 디바이스 권장) / BLE / Serial
- 디바이스 ID 허용 목록 강제 적용
- 레이트 리밋 (최대 프레임/초)
- 원격 호출 시 인증 토큰 필수

| 엔드포인트 | 메서드 | 설명 |
|---|---|---|
| `/api/hardware/push-frame` | POST | LED 프레임 데이터 전송 |
| `/api/hardware/clear` | POST | LED 전체 초기화 |

**15. 보안 요구사항**
- 자격증명 환경변수 관리 (하드코딩 금지)
- 원격 연결 활성화 전 보안 검토 필수
- ML 로직과 브릿지 모듈 분리 유지

---

## 🧪 테스트

### FastAPI 테스트

```bash
cd api

# pytest로 테스트 실행
uv run pytest

# 또는 pip 환경에서
pytest
```

### Unity 테스트

Unity Editor에서 Play 버튼으로 테스트

---

## 📚 참고 자료

- [FastAPI 공식 문서](https://fastapi.tiangolo.com/)
- [Unity WebGL 가이드](https://docs.unity3d.com/Manual/webgl.html)
- [uv 패키지 매니저](https://github.com/astral-sh/uv)
- [Docker Compose](https://docs.docker.com/compose/)

---

## 🤝 기여

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 라이선스

MIT License

---

## 💡 팁

- **WSL2 + Docker Desktop**: Windows에서 Docker Desktop을 실행하면 WSL2에서 자동으로 사용 가능
- **Unity 빌드 최적화**: Development Build 체크 해제, Brotli 압축 활성화
- **FastAPI 개발**: `--reload` 옵션으로 코드 변경 시 자동 재시작
- **uv 속도**: pip보다 10-100배 빠른 패키지 설치
