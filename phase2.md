# Phase 2 실행/수정 요약 (cron 기반)

작성일: 2026-03-04  
대상 프로젝트: `projects/Guitar_Project`

---

## 0) 최종 상태

`pipeline-state.md` 기준 현재 상태:
- `Phase: 2`
- `Checkpoint: C`
- `Step: C-8`
- `Status: completed`
- `Phase2Status: completed`

즉, **Phase 2는 완료 처리**되어 있음.

---

## 1) 이번에 실제 반영된 수정 파일

### 코드/상태 파일(수정)
- `api/main.py`
- `Tunerapp/index.html`
- `pipeline-state.md`

### 스펙 문서(생성)
- `unity-specs/B4a-songdata-cs.md`
- `unity-specs/B4b-songselector-ui.md`
- `unity-specs/B5a-practiceconfig.md`
- `unity-specs/B5b-session-update.md`
- `unity-specs/B6-transpose.md`

---

## 2) Checkpoint A (FastAPI) 수행 내용

### A-1-a / A-1-b / A-1-c
- `SongData` 스키마 + 내장 5곡 데이터 구성
- `GET /api/songs`, `GET /api/songs/{id}`
- `POST/PUT/DELETE /api/songs` (커스텀 곡 저장/수정/삭제 제한 포함)

### A-2-a / A-2-b / A-2-c
- `POST /api/session/start`
- `POST /api/session/loop-complete` (loopIndex 증가, tempo ramp, auto transpose 반영)
- `GET /api/session/state`, `GET /api/session/state/{sessionId}`
- `POST /api/session/end`, `GET /api/session/logs`

### A-3-a / A-3-b 테스트
- `test_songs.py` 통과 기록(7/7)
- `test_session.py` 포함 전체 테스트 통과 기록(11/11)

---

## 3) Checkpoint B (Unity Spec) 수행 내용

### B-4-a / B-4-b
- SongData/SongDatabase 구조 스펙 문서화
- SongSelectorPanel UI 스펙 문서화

### B-5-a / B-5-b
- PracticeConfig + CountIn 스펙
- PracticeSessionController 업데이트 + ConfigPanel 스펙

### B-6
- 키 조옮김(Transpose) 스펙 문서화
- `TransposeSemitones`, `GetTransposed`, UI 패널 요구사항 정리

---

## 4) Checkpoint C (Tunerapp) 수행 내용

### C-7-a
- 피치 스무딩(7프레임 히스토리 기반) 로직 반영/검증
- 중앙값 기반 이상치 제거 + 이동평균 + 침묵 구간 처리(`—`)

### C-8
- `server.js` 문법 검증(`node --check`) 통과
- 포트 정합성 확인(`8000`)
- `/tuner` no-store 확인
- `/api/*` 프록시 경로 확인

---

## 5) 실행 로그에서 보인 대표 검증 명령

- `python3 -m py_compile api/main.py`
- `python3 -m pytest api/tests/test_songs.py -v`
- `python3 -m pytest api/tests/ -v`
- `node --check Tunerapp/server.js`

---

## 6) 에러/주의사항 (중요)

cron 실행 중 다수 job이 `error`로 보였던 공통 원인:
- `Delivering to Telegram requires target <chatId>`

의미:
- 구현 작업 자체가 실패했다기보다,
- **완료 보고 전달 단계(announce delivery)에서 대상 chatId 미지정으로 실패 처리**된 케이스가 많았음.

또한 과거 테스트 환경 이슈 기록:
- 샌드박스 Python/venv 바이너리 불일치(`pydantic_core`) 관련 오류 기록이 `pipeline-state.md`에 남아 있음.

---

## 7) 결론

- `pipeline-state.md` 기준 **Phase 2는 완료**.
- 핵심 코드(`api/main.py`, `Tunerapp/index.html`)와 스펙 문서군(`unity-specs/B*.md`)이 반영됨.
- 이후 안정 운영을 위해 cron 알림은 `to=telegram:8194519852` 고정 권장.

---

## 8) Claude Code 교차검증 결과 (2026-03-04)

### 실제 테스트 실행 결과

```
환경: uv venv (Python 3.11.0rc1) + httpx, pytest, fastapi, pydantic 설치
명령: PYTHONPATH=/home/user/projects/Guitar_Project pytest api/tests/ -v
결과: 11 passed in 0.49s ✅
```

| 테스트 파일 | 항목 수 | 결과 |
|---|---|---|
| test_session.py | 4개 | ✅ PASSED |
| test_songs.py | 7개 | ✅ PASSED |

문법 검증:
- `node --check Tunerapp/server.js` → OK ✅
- `python -m py_compile api/main.py` → OK ✅

### 실제 구현 vs phase2.md 일치 여부

| 항목 | phase2.md 기록 | 실제 구현 | 일치 |
|---|---|---|---|
| 내장 5곡 | autumn-leaves 등 5곡 | 동일 5곡 존재 | ✅ |
| 세션 API | start/loop/state/end/logs | 전부 구현됨 | ✅ |
| tempoRamp | loopIndex마다 BPM 증가 | line 398-400 구현 | ✅ |
| autoTranspose | C+5→F 등 크로매틱 | `_transpose_key_chromatic()` | ✅ |
| 피치 스무딩 | 7프레임 중앙값+이동평균 | `PITCH_HISTORY_SIZE=7` | ✅ |
| 포트 일치 | 8000 | `PORT=8000` | ✅ |
| unity-specs 5종 | B4a~B6 | 전부 존재, 크기 정상 | ✅ |

**불일치 없음. phase2.md 내용과 실제 구현이 완전히 일치함.**

---

## 9) openclaw cron 명령 수정 필요 사항

### 현재 에이전트가 반복 실패하는 명령

에이전트가 작업 완료 후 아래 패턴으로 상태 확인을 시도하지만 모두 실패함:

```bash
# ❌ 잘못된 사용 - status는 인자 미지원
openclaw cron status p2-a1a-songdata-schema
# Error: too many arguments for 'status'. Expected 0 arguments but got 1.
```

### 올바른 명령 패턴

```bash
# ✅ 스케줄러 전체 상태 확인
openclaw cron status

# ✅ 특정 job 실행 이력 확인
openclaw cron runs --id <uuid>

# ✅ 전체 목록 + 상태 확인
openclaw cron list

# ✅ job 이름으로 UUID 찾기
openclaw cron list | grep <job-name>
```

### cron prompt 수정 권고 사항

`guitar-pipeline.cron.json` 또는 각 job의 prompt에 아래 내용 명시 필요:

#### 1. 상태 확인 명령 교정
```
# 작업 완료 후 상태 확인 시
openclaw cron status             # 스케줄러 전체 상태 (인자 없음)
openclaw cron list | grep <name> # 특정 job 존재 여부
```

#### 2. pytest 실행 환경
에이전트 sandbox(Python 3.11)에서 pytest 직접 실행 가능하지 않음.
올바른 실행 방법:
```bash
# sandbox에서는 아래 방식만 작동
PYTHONPATH=/workspace/projects/Guitar_Project \
  python3 -m pytest api/tests/ -v
```
단, `pytest`가 sandbox에 미설치 상태면 `python3 -c "..."` 방식으로 개별 테스트 수행 필요.

#### 3. 파일 경로 주의
에이전트가 종종 틀리는 경로:
```
❌ /home/sandbox/Guitar_Project/...
✅ /home/sandbox/projects/Guitar_Project/...
```
cron prompt에서 경로 참조 시 `/home/sandbox/projects/Guitar_Project` 기준 명시 필요.

#### 4. Docker 컨테이너 내 테스트 불가
`docker exec guitar-ai-api python3 -m pytest ...` 방식은 컨테이너에 pytest 미설치로 실패함.
pytest는 별도 venv 또는 sandbox 환경에서만 실행 가능.

#### 5. announce delivery target 설정
cron job 등록 시 `--announce` 플래그 사용하는 경우
Telegram chatId 미설정 시 `Delivering to Telegram requires target <chatId>` 에러 발생.
```bash
# 에이전트 prompt에 명시하거나, job 등록 시 target 지정 필요
```

### 향후 cron job prompt 작성 템플릿 권장사항

```
작업 완료 후 pipeline-state.md 업데이트 후:
1. 완료 확인: openclaw cron list (인자 없이)
2. 실행 이력: openclaw cron runs --id <해당 job UUID>
3. 다음 job 트리거: openclaw cron run --id <다음 job UUID>
경로 기준: /home/sandbox/projects/Guitar_Project/
pytest 실행: PYTHONPATH=/workspace/projects/Guitar_Project python3 -m pytest api/tests/ -v
```
