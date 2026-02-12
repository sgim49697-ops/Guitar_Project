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
| `/api/alternative-fingering` | POST | 대체 운지법 추천 |
| `/api/practice-routine` | POST | 맞춤형 연습 루틴 생성 |
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
MONGO_PASSWORD=guitarsecret
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
│  │    → 운지법 추천                  │   │
│  │    → 연습 루틴 생성               │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ 🐳 MongoDB                        │   │
│  │    → 사용자 데이터                │   │
│  │    → 연습 기록                    │   │
│  └──────────────────────────────────┘   │
└──────────────────────────────────────────┘
```

---

## 📝 MVP 기능

### ✅ 구현 완료
- [x] 기타 튜너 (피치 감지, 실시간 시각화)
- [x] WebSocket 원격 디스플레이 (iPhone → PC)
- [x] Unity C# 스크립트 (DSP 타이밍, 지판 렌더링)
- [x] FastAPI AI 서버 (코드 설명, 운지법 추천)
- [x] Docker 통합 환경

### 🚧 Unity 에디터 작업 필요
- [ ] ScriptableObject 에셋 생성 (5개 코드: C, G, Am, F, D)
- [ ] 씬 구성 (GameManager, Fretboard, UICanvas)
- [ ] 프리팹 생성 (FretMarker, StringLine)
- [ ] 메트로놈 사운드 추가
- [ ] WebGL 빌드 및 테스트

### 🔮 향후 추가 예정
- [ ] LLM 연동 (OpenAI/Claude/로컬 모델)
- [ ] 사용자 진행도 추적 (MongoDB)
- [ ] 강화학습 기반 맞춤형 추천
- [ ] 3D 지판 렌더링

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
