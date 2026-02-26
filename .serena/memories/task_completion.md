# 작업 완료 체크리스트

## C# 수정 후
1. `bash scripts/sync_unity_to_windows.sh` 로 Windows 반영
2. Unity 재컴파일 + 에러 0건
3. 공개 API 호환성 확인: HighlightChord, TransitionToChord, PreviewNextChord

## Python 수정 후
1. `uv run pytest`
2. `docker compose restart`

## JS 수정 후
- server.js: `docker compose restart`
- index.html: 재시작 불필요
