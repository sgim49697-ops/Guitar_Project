# Guitar Project — OpenClaw 자동화 파이프라인 가이드

> 목표: iReal Pro 수준의 기능을 갖춘 기타 연습 앱을 OpenClaw + Codex 자동화로 구현한다.
> 방법: 기능을 Phase → Checkpoint → Step 3단계로 분해하고, OpenClaw cron으로 순차 실행한다.

---

## 1. 전략 개요

### 자동화 원칙

```
사용자 트리거 → OpenClaw cron → Codex 호출 (수십~수백 회) → 결과물 생성
                                       ↓
                            Telegram 체크포인트 알람
                                       ↓
                            사용자 검토 → 다음 Phase 승인
```

### 역할 분담

| 구성요소 | 역할 |
|---------|------|
| OpenClaw cron | 파이프라인 스케줄링 및 Step 순차 실행 |
| Codex (gpt-5.3-codex) | 실제 코드 작성 (FastAPI, Node.js, C# spec) |
| Claude Code (MCP) | Unity C# 파일 실제 수정 (Windows 경로 접근) |
| Telegram | 체크포인트 알람 및 사용자 승인 수신 |
| pipeline-state.md | 파이프라인 상태 추적 파일 |

### 실행 환경 제약

| 항목 | 가능 | 불가 |
|------|------|------|
| FastAPI (`api/`) 수정 | ✅ 샌드박스 접근 가능 | |
| Tunerapp (`Tunerapp/`) 수정 | ✅ 샌드박스 접근 가능 | |
| Unity C# 직접 수정 | ❌ `/mnt/c/...` 샌드박스 밖 | |
| Unity C# spec MD 생성 | ✅ → Claude Code로 전달 | |
| pip/npm 설치 | ❌ network:none → bridge 전환됨 | |
| 테스트 실행 | ✅ pytest, node --check | |

---

## 2. Phase별 구현 로드맵

irealpro.md 갭 분석 기반. 우선순위 순으로 배치.

```
Phase 2 — 연습 기능 강화        (FastAPI + Unity spec)   ← 즉시 착수
Phase 3 — 코드 시스템 확장      (Unity spec 중심)
Phase 4 — 오디오 반주 엔진      (Tunerapp + Unity)
Phase 5 — 공유 및 커뮤니티      (FastAPI + DB)
```

---

## 3. Phase 2 — 연습 기능 강화

### Checkpoint A: FastAPI 백엔드 + 데이터 모델

**Step A-1: SongDatabase API 구현**
- `api/main.py`에 `SongData` 스키마 추가
- `GET /api/songs` — 내장 곡 목록 반환
- `GET /api/songs/{id}` — 단일 곡 상세 (코드 진행 포함)
- 초기 내장 곡: Autumn Leaves, Blue Bossa, Fly Me to the Moon, Summertime, All of Me (5곡)
- `POST /api/songs` — 커스텀 코드 진행 저장

```python
# 목표 스키마
class SongData(BaseModel):
    id: str
    title: str
    composer: str
    style: str          # "Medium Swing", "Bossa Nova"
    key: str            # "C", "Bb"
    bpm: int
    timeSignature: str  # "4/4", "3/4"
    chordProgression: list[str]  # ["C^7", "A-7", "D-9", "G7"]
    sections: list[SectionMarker]
    genre: str
    tags: list[str]
```

**Step A-2: PracticeSession API 구현**
- `POST /api/session/start` — 연습 세션 시작 (곡 ID, BPM, 키, 루프 설정)
- `POST /api/session/loop-complete` — 루프 완료 이벤트 수신 → BPM/키 자동 조정
- `GET /api/session/state` — 현재 세션 상태 반환
- `POST /api/session/end` — 세션 종료 + 연습 로그 저장

**Step A-3: 단위 테스트**
- `pytest api/tests/test_songs.py`
- `pytest api/tests/test_session.py`

---

### Checkpoint B: Unity spec — 연습 기능 UI

**Step B-4: SongDatabase Unity spec 작성**
파일: `unity-specs/B4-song-database.md`

```
목표: SongData ScriptableObject + SongDatabase 구현
수정 대상:
  - Assets/Scripts/Data/SongData.cs (신규)
  - Assets/Scripts/Data/SongDatabase.cs (신규)
  - Assets/Scripts/UI/SongSelectorPanel.cs (신규)
기존 API 유지: ChordDatabase.GetChord(), FretboardRenderer.HighlightChord()
참고: irealpro.md §3 SongLibrary 구조
```

**Step B-5: PracticeConfig Unity spec 작성**
파일: `unity-specs/B5-practice-config.md`

```
목표: 자동 BPM 증가 + 루프 횟수 설정 + 카운트인 구현
수정 대상:
  - Assets/Scripts/PracticeSessionController.cs (수정)
  - Assets/Scripts/TimingEngine.cs (수정 — CountInMeasures 추가)
  - Assets/Scripts/UI/PracticeConfigPanel.cs (신규)
새 필드:
  - PracticeConfig.tempoRampBpm (int)
  - PracticeConfig.loopCount (int, -1=무한)
  - PracticeConfig.countInEnabled (bool)
  - PracticeConfig.countInBeats (int)
기존 API 유지: OnLoopComplete(int), SetBPM(float)
```

**Step B-6: 키 조옮김 Unity spec 작성**
파일: `unity-specs/B6-key-transpose.md`

```
목표: ±반음 조옮김 버튼 + 자동 12키 전조 모드
수정 대상:
  - Assets/Scripts/ChordDatabase.cs (TransposeSemitones 유틸 추가)
  - Assets/Scripts/PracticeSessionController.cs (자동 전조 로직)
  - Assets/Scripts/UI/TransposePanel.cs (신규)
알고리즘:
  - 현재 키에서 N반음 이동 → 모든 코드 포지션 오프셋 처리
  - +5씩 12회 = 완전4도 전조로 12키 완전 커버
```

---

### Checkpoint C: Tunerapp 개선

**Step C-7: 피치 스무딩 구현**
- `Tunerapp/index.html`: 최근 5~8프레임 이동평균 필터
- 현재 상태: 프레임마다 튀는 피치 표시 문제

**Step C-8: 서버 정합성 검증**
- `Tunerapp/server.js` ↔ `docker-compose.yml` 포트 일치 확인
- `node --check Tunerapp/server.js` 문법 검증

---

## 4. Phase 3 — 코드 시스템 확장

### Checkpoint D: 다중 보이싱

**Step D-9: ChordData 다중 보이싱 spec**
파일: `unity-specs/D9-multi-voicing.md`

```
목표: 각 코드마다 2~5개 보이싱 포지션 지원
수정 대상:
  - Assets/Scripts/Data/ChordData.cs (voicings 리스트 추가)
  - Assets/Scripts/FretboardRenderer.cs (보이싱 인덱스 파라미터)
  - Assets/Scripts/UI/VoicingSelector.cs (신규 — 스와이프/버튼 전환)
```

**Step D-10: 코드 스케일 뷰 spec**
파일: `unity-specs/D10-scale-view.md`

```
목표: 코드 → 추천 스케일 오버레이 표시 (즉흥 연주 학습)
매핑 예시:
  - C7 → Mixolydian
  - A-7 → Dorian
  - G7alt → Altered
수정 대상:
  - Assets/Scripts/Data/ScaleDatabase.cs (신규)
  - Assets/Scripts/FretboardRenderer.cs (ScaleOverlay 모드 추가)
```

---

## 5. Phase 4 — 오디오 반주 엔진

### Checkpoint E: WebGL 기반 반주

**Step E-11: Tone.js 반주 엔진 (Tunerapp)**
- `Tunerapp/index.html`에 Tone.js 통합
- 코드명 → 보이싱 → 피아노/기타 샘플 실시간 합성
- Unity WebGL ↔ Tunerapp 통신: `window.postMessage` 또는 WebSocket

**Step E-12: FastAPI 반주 스타일 API**
- `GET /api/styles` — 지원 스타일 목록 (Swing, Bossa Nova, Pop 등)
- `POST /api/accompaniment/generate` — 코드 진행 + 스타일 → 반주 이벤트 시퀀스 반환

---

## 6. OpenClaw cron 설정

### 6.1 cron 작업 등록 방법

OpenClaw cron은 `openclaw.json`에 등록하거나 `openclaw cron add` 명령으로 추가한다.

```json
// openclaw.json에 추가할 cron 섹션 예시
{
  "cron": {
    "enabled": true,
    "maxConcurrentRuns": 1,
    "sessionRetention": "7d",
    "jobs": [
      {
        "id": "guitar-phase2-checkpoint-a",
        "schedule": "manual",
        "sessionTarget": "isolated",
        "delivery": {
          "mode": "announce",
          "channel": "telegram"
        },
        "prompt": "파이프라인 실행: /home/sandbox/Guitar_Project/pipeline-state.md 읽고 Checkpoint A 실행"
      }
    ]
  }
}
```

> **주의:** `schedule: "manual"`은 Telegram에서 수동으로 트리거하는 방식.
> 자동 실행은 cron 표현식 사용: `"0 2 * * *"` (매일 새벽 2시 등).

### 6.2 Telegram에서 파이프라인 트리거

```
# Telegram 메시지로 cron job 수동 실행
/cron run guitar-phase2-checkpoint-a

# 또는 자연어로
"Guitar Project Phase 2 Checkpoint A 실행해줘"
```

### 6.3 sessionTarget 선택 기준

| 옵션 | 사용 시점 |
|------|---------|
| `isolated` | 독립 실행 — 코드 작성, 테스트, 파일 수정 |
| `main` | 사용자와 대화 중인 세션 재사용 |

**항상 `isolated` 사용 권장** — 이전 컨텍스트 오염 없이 깨끗한 상태 보장.

---

## 7. 파이프라인 상태 파일

### 위치
`/home/sandbox/Guitar_Project/pipeline-state.md`

### 형식

```md
# Pipeline State

## 현재 단계
Phase: 2
Checkpoint: A
Step: A-2
Status: in_progress

## 완료된 Steps
- [x] A-1: SongDatabase API 구현 (2026-03-01) — 5곡 내장, pytest 통과
- [ ] A-2: PracticeSession API 구현
- [ ] A-3: 단위 테스트
- [ ] B-4: SongDatabase Unity spec
- [ ] B-5: PracticeConfig Unity spec
- [ ] B-6: 키 조옮김 Unity spec
- [ ] C-7: Tunerapp 피치 스무딩
- [ ] C-8: 서버 정합성 검증

## 마지막 오류
없음

## Unity Spec 대기 목록
(Claude Code가 처리해야 할 spec 파일 목록)
- [ ] unity-specs/B4-song-database.md
- [ ] unity-specs/B5-practice-config.md
```

---

## 8. Codex 프롬프트 템플릿

각 Step을 Codex에 위임할 때 사용하는 프롬프트 구조.

### Step A-1 프롬프트 예시

```
작업: Guitar_Project FastAPI에 SongDatabase API 구현

환경:
- 작업 경로: /home/sandbox/Guitar_Project/api/
- 실행 가능 명령: python3 -m pytest, python3 -m py_compile

현재 main.py 내용: (파일 직접 읽어서 포함)

구현 요구사항:
1. SongData Pydantic 모델 추가 (title, composer, style, key, bpm, timeSignature, chordProgression, sections, genre, tags)
2. GET /api/songs — 5곡 내장 반환 (Autumn Leaves, Blue Bossa, Fly Me to the Moon, Summertime, All of Me)
3. GET /api/songs/{id} — 단일 곡 반환, 없으면 404
4. POST /api/songs — 커스텀 곡 추가 (인메모리 저장)

완료 기준:
- python3 -m py_compile api/main.py 통과
- pytest api/tests/test_songs.py 통과 (테스트 파일도 작성할 것)

완료 후:
- /home/sandbox/Guitar_Project/pipeline-state.md에 A-1 완료 표시
- 결과 요약을 Telegram으로 전송
```

### Unity spec 프롬프트 예시

```
작업: Unity C# 변경 사양서 작성

Unity 파일은 샌드박스에서 접근 불가. 대신 아래 형식으로 spec MD 파일을 작성한다.
저장 위치: /home/sandbox/Guitar_Project/unity-specs/B4-song-database.md

참고 파일:
- irealpro.md §3 (SongLibrary 구조)
- CLAUDE.md (Unity 스크립트 경로 및 기존 구현 현황)
- AGENTS.md (에이전트 역할 정의)

기존 구현 (반드시 하위 호환 유지):
- FretboardRenderer.HighlightChord(ChordData chord)
- PracticeSessionController.OnLoopComplete(int loopIndex)
- ChordDatabase.GetChord(string name) → ChordData

spec 포함 필수 항목:
1. 신규/수정 파일 목록 (Assets/Scripts/... 경로 명시)
2. 각 클래스의 public API (시그니처 수준)
3. 기존 API와의 호환성 명시
4. 유니티 씬 연결 방법 (Inspector 설정 포함)
5. 컴파일 에러 위험 요소 사전 명시
```

---

## 9. 체크포인트 알람 형식

### Checkpoint 완료 알람

```
✅ Checkpoint A 완료 — Guitar Project Phase 2

완료된 Steps:
- A-1: SongDatabase API — 5곡 내장, GET/POST 엔드포인트 구현, pytest 전체 통과
- A-2: PracticeSession API — start/loop-complete/state/end 엔드포인트 구현
- A-3: 단위 테스트 — 15개 테스트 전체 통과

생성된 파일:
- api/main.py (수정)
- api/tests/test_songs.py (신규)
- api/tests/test_session.py (신규)

다음 Checkpoint: B (Unity spec 작성)
계속하려면 "Checkpoint B 시작해줘"라고 보내주세요.
```

### 오류 알람

```
❌ Step A-2 실패 — Guitar Project

오류: AssertionError: /api/session/start 422 Unprocessable Entity
위치: api/tests/test_session.py:34

원인 분석: sessionId 필드 누락 (UUID 자동 생성 코드 미작성)
중단됨 — 확인 후 "A-2 재시도해줘"라고 보내주세요.
```

---

## 10. Unity spec → Claude Code 전달 워크플로우

Unity C# 파일은 Windows 경로(`/mnt/c/...`)에 있어 샌드박스 접근 불가.
OpenClaw가 spec MD를 작성하면, Claude Code(이 세션)가 직접 수정한다.

```
[OpenClaw/Codex]                    [Claude Code (이 세션)]
    │                                        │
    ├─ unity-specs/B4-song-database.md 작성 ─┤
    ├─ Telegram으로 spec 완료 알람          │
    │                                        ├─ spec 파일 읽기
    │                                        ├─ Unity 파일 직접 수정
    │                                        │  (/mnt/c/.../Assets/Scripts/...)
    │                                        ├─ recompile_scripts 실행
    │                                        └─ 결과 보고
    │                                        │
    └─ "Unity 수정 완료" 확인 후 다음 Step 진행
```

### 전달 명령 예시

Telegram에서 OpenClaw가 spec 완료 알람을 보내면:

```
"unity-specs/B4-song-database.md 보고 Unity 파일 수정해줘"
→ Claude Code가 spec 읽고 /mnt/c/... 경로 직접 수정
```

---

## 11. 전체 실행 순서 요약

```
1. Telegram: "Guitar Project Phase 2 시작해줘"
   → OpenClaw가 pipeline-state.md 확인 후 Checkpoint A 실행

2. [자동] Checkpoint A: Step A-1 → A-2 → A-3 순차 실행
   → 완료 시 Telegram 알람

3. 사용자 검토 → "Checkpoint B 시작해줘"
   → OpenClaw가 Checkpoint B (Unity spec 작성) 실행

4. [자동] Checkpoint B: Step B-4 → B-5 → B-6 spec 파일 작성
   → 완료 시 Telegram 알람 (spec 파일 목록 포함)

5. 사용자: "Unity 파일 수정해줘"
   → Claude Code가 spec 읽고 /mnt/c/... 직접 수정
   → recompile_scripts → 에러 0개 확인

6. "Checkpoint C 시작해줘" → Tunerapp 개선

7. Phase 2 완료 → "Phase 3 시작해줘" → 반복
```

---

## 12. 권장 OpenClaw guitar-pipeline 스킬 설정

`~/.openclaw/workspace/skills/guitar-pipeline/SKILL.md`에 아래 내용으로 업데이트:

```yaml
---
name: guitar-pipeline
description: Guitar_Project Phase별 자동화 파이프라인 실행 스킬
metadata: {"openclaw":{"always":true}}
---

## 트리거 패턴
- "Guitar Project [Phase/Checkpoint/Step] 시작해줘"
- "Checkpoint [A-E] 실행해줘"
- "Step [X-N] 재시도해줘"

## 실행 전 필수 확인
1. /home/sandbox/Guitar_Project/pipeline-state.md 읽기
2. 현재 Status 확인 (completed/in_progress/failed/waiting_review)
3. 이전 Step 완료 여부 확인 후 다음 Step 실행

## 파일 경로
- 상태 파일: /home/sandbox/Guitar_Project/pipeline-state.md
- Unity spec: /home/sandbox/Guitar_Project/unity-specs/
- API: /home/sandbox/Guitar_Project/api/main.py
- Tunerapp: /home/sandbox/Guitar_Project/Tunerapp/

## 실행 후 필수
- pipeline-state.md 업데이트
- Telegram 알람 전송
- 오류 시 즉시 중단 (다음 Step 진행 금지)
```

---

## 13. 오류 복구 전략

| 오류 유형 | 복구 방법 |
|---------|---------|
| pytest 실패 | Codex에 에러 로그 + 파일 내용 함께 전달해 재시도 |
| Unity spec 불완전 | Claude Code가 "spec에서 누락된 부분" 지적 후 Codex에 재요청 |
| 컴파일 에러 | 에러 메시지를 Unity spec 수정 요청으로 Telegram 전송 |
| Docker 포트 충돌 | `docker compose restart` 후 재시도 |
| 네트워크 타임아웃 | 자동 재시도 없음 — Telegram 알람 후 수동 재시작 |

---

## 14. 비용 및 토큰 관리

- 각 Step을 `isolated` 세션으로 실행 → 컨텍스트 누적 없음
- 파일을 직접 읽어 프롬프트에 포함 → 추측 코드 생성 방지
- Step 단위로 분할 → 단일 프롬프트 과부하 방지
- 체크포인트 단위로 사용자 검토 → 방향 오류 조기 발견

예상 호출 수:
- Phase 2: ~20~30회 (Step당 평균 3~5회 재시도 포함)
- Phase 3: ~15~20회
- Phase 4: ~40~60회 (오디오 엔진 복잡도)
- 총계: 100~150회 예상 (여유있게 500회 예산 잡아도 무방)

---

*이 가이드는 프로젝트 진행에 따라 업데이트된다.*
*최종 수정: 2026-03-01*
