# 코드 스타일 (핵심만)

- 새 파일 최상단 한 줄 설명 주석
- 비밀값 하드코딩 금지 → .env
- C#: 람다 루프 변수 반드시 캡처 (`int c = i; () => f(c)`)
- C#: HorizontalLayoutGroup → `childForceExpandHeight = true`
- Python: uv pip 필수, 시스템 경계에서만 입력 검증
