# 주요 명령어 (핵심만)

## Unity C# 동기화
```bash
bash scripts/sync_unity_from_windows.sh   # Windows→Linux
bash scripts/sync_unity_to_windows.sh     # Linux→Windows
```

## 서버
```bash
docker compose restart   # server.js 변경 후
docker compose logs -f
```

## Python (uv pip 필수, bare pip 금지)
```bash
uv pip install <package>
uv run uvicorn main:app --reload --port 8080
uv run pytest
