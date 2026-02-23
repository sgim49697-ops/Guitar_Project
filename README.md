# Guitar Project (MVP v1)

기타 연습 MVP 프로젝트입니다.  
구성은 `Unity WebGL 연습앱 + Tuner 웹앱 + FastAPI 백엔드` 입니다.

## 1. 프로젝트 요약

- `Tunerapp/`: Node.js(Express + WebSocket) 기반 튜너/디스플레이
- `api/`: FastAPI 기반 API 서버
- `UnityApp/`: Linux 저장소 기준 Unity 미러(협업/AI 참고용)
- 실사용 Unity 원본(로컬 Windows): `/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity`

중요:
- 이 저장소는 Windows Unity 원본을 직접 포함하지 않습니다.
- Unity 변경 기준선은 Windows 경로이고, `UnityApp/`은 동기화된 미러입니다.

## 2. clone 후 첫 실행 (권장: WSL + Windows Unity)

### 2.1 사전 요구사항

- Git
- Docker / Docker Compose
- WSL2 + Windows Unity 설치 환경 (Unity 연습앱까지 실행할 경우)
- `rsync` (Unity 미러 동기화에 사용)

### 2.2 clone

```bash
git clone <YOUR_REPO_URL>
cd Guitar_Project
```

### 2.3 Unity 미러 동기화 (권장)

`UnityApp/`을 최신 Windows Unity 기준으로 맞춥니다.

```bash
# 변경 예정 확인
./scripts/sync_unity_from_windows.sh --dry-run

# 실제 반영
./scripts/sync_unity_from_windows.sh
```

Windows Unity 경로가 기본값과 다르면:

```bash
UNITY_WIN_SRC=/mnt/c/Projects/<YOUR_PATH>/GuitarPracticeUnity ./scripts/sync_unity_from_windows.sh --dry-run
UNITY_WIN_SRC=/mnt/c/Projects/<YOUR_PATH>/GuitarPracticeUnity ./scripts/sync_unity_from_windows.sh
```

동기화 정책 문서:
- `UnityApp/SYNC_POLICY.md`
- `UnityApp/CONTEXT_WINDOWS_SOURCE.md`

## 3. Docker로 실행

```bash
docker compose up -d --build
docker compose logs -f
```

접속:
- 튜너: `http://localhost:8000/tuner`
- Unity 연습앱(WebGL): `http://localhost:8000/practice`
- FastAPI docs: `http://localhost:8080/docs`
- Mongo Express: `http://localhost:8081`

## 4. 다른 컴퓨터에서 자주 막히는 포인트

`docker-compose.yml`의 `/practice` 볼륨은 현재 아래 경로를 마운트합니다.

```yaml
- /mnt/c/Projects/Project_Guitar/GuitarPracticeUnity/Builds/WebGL:/app/practice:ro
```

즉, 다른 컴퓨터에서도:
1. 해당 경로(또는 동일 구조의 경로)에 Unity WebGL 빌드 파일이 있어야 하고
2. 경로가 다르면 `docker-compose.yml` 볼륨 경로를 로컬 환경에 맞게 수정해야 합니다.

경로가 맞지 않으면 `/practice`는 정상 표시되지 않습니다.

## 5. 로컬 개발 실행 (Docker 없이)

### 5.1 Tunerapp

```bash
cd Tunerapp
npm install
npm start
```

### 5.2 FastAPI

```bash
cd api
uv sync
uv run uvicorn main:app --reload --port 8080
```

## 6. 현재 MVP 범위

필수:
- Unity 연습 루프(코드 진행 재생 + 지판 점등 + UI 하이라이트)

보조:
- 튜너(Tunerapp)
- 코드 설명 API(`POST /api/explain-chord`)

상세 MVP 기준은 `mvp.md` 참고.

## 7. 권장 작업 순서 (새 환경)

1. clone
2. `./scripts/sync_unity_from_windows.sh --dry-run`
3. `./scripts/sync_unity_from_windows.sh`
4. Unity에서 WebGL 빌드 생성(Windows Unity 원본)
5. `docker compose up -d --build`
6. `http://localhost:8000/practice` 확인

