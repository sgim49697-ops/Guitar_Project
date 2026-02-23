# Guitar Practice Project - Root Agent Guide

이 문서는 프로젝트 루트에서 에이전트 운영 규칙을 빠르게 찾기 위한 요약/진입점 문서입니다.

## Source of Truth

규칙 우선순위는 아래 순서를 따릅니다.

1. `codex/AGENTS.md` (주 규칙, 오케스트레이터 규칙, 코딩 표준)
2. `.claude/agents/*.md` (서브에이전트별 상세 역할/제약)
3. `CLAUDE.md`, `gemini/GEMINI.md` (보조 컨텍스트)

충돌 시 `codex/AGENTS.md`를 우선합니다.

## Sub-Agent Registry

| Agent | Status | Trigger Keywords (요약) | Primary Scope | Out of Scope | Spec File |
|---|---|---|---|---|---|
| `fingering-coach-agent` | ACTIVE (MVP Phase 1) | UI 수정, 코칭 힌트, DTO/응답 형태, 텔레메트리 | Unity UI 패널, 코칭 표시, DTO(`suggested_fingering`, `coaching_messages`, `next_exercise`) | ML 코드 인식, 하드웨어 연동 | `.claude/agents/fingering-coach-agent.md` |
| `chord-recognizer-agent` | INACTIVE (Phase 4) | 코드 인식, 음 감지, ML, 피처 추출, 추천 API | `/api/recommend-fingering`, 피처 추출, 오프라인 평가 | Unity UI, 하드웨어 연동 | `.claude/agents/chord-recognizer-agent.md` |
| `hardware-bridge-agent` | INACTIVE (Phase 5) | LED, MQTT/BLE/Serial, 디바이스 제어, `/api/hardware` | 하드웨어 브릿지, `push-frame/clear`, 보안(allowlist/rate limit/token) | Unity UI, ML 코드 인식 | `.claude/agents/hardware-bridge-agent.md` |

## Dispatch Rules

- UI/코칭/DTO 중심 요청은 `fingering-coach-agent` 규칙을 우선 적용합니다.
- 코드 인식/모델/평가 요청은 `chord-recognizer-agent` 규칙을 적용합니다.
- 하드웨어/프로토콜/보안 요청은 `hardware-bridge-agent` 규칙을 적용합니다.
- 혼합 요청은 도메인별로 분리하고, 공개 API 호환성 검사를 공통으로 수행합니다.

## Orchestration Checklist

작업 전:

1. 관련 파일을 먼저 직접 읽고 현재 구현 상태를 확인합니다.
2. 필요한 경우 서브에이전트 스펙 파일을 함께 읽어 범위/금지사항을 고정합니다.

작업 후:

1. 수정 파일 전체 재검토
2. Unity/C# 변경 시 컴파일/진단 확인
3. 기존 공개 API 호환성 확인
4. 보안/비밀정보/PII 규칙 위반 여부 확인

상세 체크리스트와 구현 규칙은 `codex/AGENTS.md`를 그대로 따릅니다.

## Sync Policy

- 서브에이전트 정의가 변경되면 이 루트 요약 문서도 함께 업데이트합니다.
- 이 문서는 요약 문서이므로 세부 규칙 본문은 복제하지 않습니다.
