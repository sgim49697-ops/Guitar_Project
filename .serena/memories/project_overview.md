# Guitar Practice App — Serena 참조 요약

## 핵심 (CLAUDE.md 참조)
자세한 구조/규칙은 프로젝트 CLAUDE.md 참조. 여기엔 Serena 작업에 필요한 핵심만 기록.

## Linux 접근 가능 파일 경로
- `UnityApp/Assets/Scripts/` — C# 미러 (Windows 정본의 읽기용 사본)
- `api/main.py` — FastAPI (Python)
- `Tunerapp/server.js` — Express (JS)

## 동기화
- Windows→Linux: `bash scripts/sync_unity_from_windows.sh`
- Linux→Windows: `bash scripts/sync_unity_to_windows.sh`
- C# 수정 정본은 Windows. Linux 미러는 읽기/AI 참조용.

## C# LSP 상태
- `dotnet 8.0` 설치됨 (`~/.dotnet`)
- `project.yml`: `csharp_omnisharp` + `python` + `typescript`
- OmniSharp 활성화 여부는 세션 재시작 후 확인 필요

## FastAPI 엔드포인트 (현재)
- `GET /health`, `POST /api/explain-chord`, `POST /api/progress`
