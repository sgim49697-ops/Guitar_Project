# Pipeline State

> 이 파일은 OpenClaw 자동화 파이프라인의 상태를 추적한다.
> 각 세션은 이 파일을 먼저 읽고, 완료 후 업데이트한다.

---

## 현재 단계

Phase: 2
Checkpoint: C
Step: C-8
Status: completed
Phase2Status: completed
LastUpdated: 2026-03-04 (C-8 서버 정합성 재검증 완료, Phase 2 완료 확인)

---

## 마지막 오류

pytest 실행 불가: 샌드박스 /usr/bin/python3 에 pytest 미설치 + 프로젝트 venv(3.12) pydantic_core 바이너리 불일치

pytest 실행 불가: 샌드박스 Python 3.11 환경에서 프로젝트 .venv(3.12 빌드) 의존성(pydantic_core) 로드 실패

---

## Unity Spec 대기 목록

(OpenClaw가 작성 완료 → Claude Code가 처리해야 할 spec 파일 목록)

- [x] unity-specs/B4-song-database.md (구현 완료 2026-03-04)
- [x] unity-specs/B5-practice-config.md (구현 완료 2026-03-04)
- [x] unity-specs/B6-transpose.md (구현 완료 2026-03-04)
- [ ] unity-specs/D9-multi-voicing.md
- [ ] unity-specs/D10-scale-view.md

---

## Phase 2 — 연습 기능 강화

Phase 2 Status: ✅ completed

### Checkpoint A: FastAPI 백엔드 + 데이터 모델

Checkpoint A Status: ✅ completed

- [x] A-1-a: SongData 스키마 + 내장 5곡 데이터 정의
- [x] A-1-b: GET /api/songs, GET /api/songs/{id} 엔드포인트
- [x] A-1-c: POST /api/songs 엔드포인트 (커스텀 곡 저장)
- [x] A-2-a: POST /api/session/start 엔드포인트
- [x] A-2-b: POST /api/session/loop-complete 엔드포인트
- [x] A-2-c: GET /api/session/state + POST /api/session/end 엔드포인트
- [x] A-3: 단위 테스트 (test_songs.py + test_session.py, pytest 전체 통과)
  - [x] A-3-a: completed — test_songs.py 7개 테스트 통과 (python3 -m pytest api/tests/test_songs.py -v)
  - [x] A-3-b: completed — test_session.py 작성 및 전체 테스트 통과 (python3 -m pytest api/tests/ -v)

### Checkpoint B: Unity spec — 연습 기능 UI ✅ completed

- [ ] B-4: SongDatabase Unity spec (unity-specs/B4-song-database.md)
- [x] B-4-a: SongData.cs/SongDatabase.cs spec 작성 (unity-specs/B4a-songdata-cs.md)
- [x] B-4-b: SongSelectorPanel UI spec 작성 (unity-specs/B4b-songselector-ui.md)
- [x] B-5: PracticeConfig Unity spec (unity-specs/B5-practice-config.md)
- [x] B-5-a: PracticeConfig + CountIn spec 작성 (unity-specs/B5a-practiceconfig.md)
- [x] B-5-b: PracticeSessionController + ConfigPanel spec 작성 (unity-specs/B5b-session-update.md)
- B-5-b: reverified 2026-03-04 (cron p2-b5b-spec-practicesession-update, PracticeSessionController 수정 항목 + PracticeConfigPanel/UI 배치 spec 반영)
- [x] B-6: 키 조옮김 Unity spec (unity-specs/B6-transpose.md)

### Checkpoint C: Tunerapp 개선

- [x] C-7-a: 피치 스무딩 (7프레임 중앙값 + 이동평균 필터)
- [x] C-8: 서버 정합성 검증 (포트 일치 확인, node --check 통과)

---

## Phase 3 — 코드 시스템 확장

### Checkpoint D: 다중 보이싱 + 스케일 뷰

- [ ] D-9-a: ChordData 다중 보이싱 spec (unity-specs/D9-multi-voicing.md)
- [ ] D-9-b: VoicingSelector UI spec 추가
- [ ] D-10-a: ScaleDatabase + FretboardRenderer 스케일 오버레이 spec (unity-specs/D10-scale-view.md)
- [ ] D-10-b: 코드→스케일 매핑 데이터 (C7→Mixolydian, A-7→Dorian, G7alt→Altered 등)

---

## Phase 4 — 오디오 반주 엔진

### Checkpoint E: WebGL 기반 반주

- [ ] E-11-a: Tone.js 반주 엔진 기초 (Tunerapp/index.html — Tone.js CDN 통합)
- [ ] E-11-b: Unity WebGL ↔ Tunerapp 통신 (postMessage 브릿지 + Unity spec)
- [ ] E-12-a: GET /api/styles 엔드포인트 (Swing, Bossa Nova, Pop 등)
- [ ] E-12-b: POST /api/accompaniment/generate 엔드포인트

---

## Phase 5 — 공유 및 커뮤니티

### Checkpoint F: 공유 기능 + DB

- [ ] F-13-a: SQLite 기반 영속화 (SongData + PracticeSession DB 스키마 마이그레이션)
- [ ] F-13-b: GET /api/community/songs 엔드포인트 (공개 곡 목록)
- [ ] F-14-a: POST /api/songs/{id}/share 엔드포인트
- [ ] F-14-b: GET /api/user/progress 엔드포인트 (연습 기록 통계)

---

## 완료된 Steps

- A-1-a: SongData 스키마 + 내장 5곡 데이터 정의 (verified 2026-03-04, py_compile)
- A-1-b: GET /api/songs, GET /api/songs/{id} 구현 (verified 2026-03-04, py_compile)
- B-4-a: SongData.cs/SongDatabase.cs spec 작성 완료 (unity-specs/B4a-songdata-cs.md) (reverified 2026-03-04)
- B-4-b: SongSelectorPanel UI spec 작성 (unity-specs/B4b-songselector-ui.md)
- B-4-b: reverified 2026-03-04 (cron p2-b4b-spec-songselector-ui, SongSelectorPanel spec required fields/API/layout 포함)
- A-2-b: POST /api/session/loop-complete 구현 (loopIndex 증가, tempoRampBpm 적용, 자동 전조, loopCount 완료 처리) (verified 2026-03-04)
- A-1-c: POST/PUT/DELETE /api/songs 구현 (커스텀 저장, 수정, 삭제 제한, 유효성 검증) (verified 2026-03-04, py_compile)
- A-2-a: PracticeSession 모델 + POST /api/session/start 구현 (UUID sessionId, SESSIONS 저장, 단일 활성 세션 제한) (verified 2026-03-04)
- A-2-a: reverified 2026-03-04 (cron p2-a2a-session-start, py_compile)
- A-2-b: reverified 2026-03-04 (cron p2-a2b-session-loop, py_compile)
- A-2-c: GET /api/session/state, GET /api/session/state/{sessionId}, POST /api/session/end, GET /api/session/logs 구현 (PracticeLog 기록 포함) (verified 2026-03-04)
- C-7-a: Tunerapp/index.html 피치 스무딩 구현 (7프레임 중앙값 필터 + 이동평균, 침묵 시 '—' 처리)
- C-7-a: reverified 2026-03-04 (cron p2-c7a-pitch-smoothing, 스무딩 로직/침묵 처리 확인 + index inline script parse check)
- C-8: Tunerapp/server.js 정합성 검증 완료 (node --check 통과, 8000 포트 일치, /api 프록시 추가)
- A-3-a: test_songs.py 작성/검증 완료 (7/7 passed) (verified 2026-03-04)
- A-3-a: reverified 2026-03-04 (cron p2-a3a-test-songs, python3 -m pytest api/tests/test_songs.py -v, 7/7 passed)
- A-3-b: reverified 2026-03-04 (cron p2-a3b-test-session, PYTHONPATH=/workspace/projects/Guitar_Project python3 -m pytest /workspace/projects/Guitar_Project/api/tests/ -v, 11/11 passed)

---

- B-5-a: PracticeConfig + CountIn spec 작성 (unity-specs/B5a-practiceconfig.md) (verified 2026-03-04)
- B-5-a: reverified 2026-03-04 (cron p2-b5a-spec-practiceconfig, PracticeConfig 필수 필드/CountInMeasures/API 무변경 확장 명시)
- B-5-b: PracticeSessionController + ConfigPanel spec 작성 (unity-specs/B5b-session-update.md)
- B-5-b: reverified 2026-03-04 (cron p2-b5b-spec-practicesession-update, PracticeSessionController 수정 항목 + PracticeConfigPanel/UI 배치 spec 반영)
- B-6: 키 조옮김 Unity spec 작성 (unity-specs/B6-transpose.md)
- B-6: reverified 2026-03-04 (cron p2-b6-spec-transpose, TransposeSemitones/GetTransposed/TransposePanel/open-string 처리/API 호환성 spec 반영)

## 생성된 파일 목록

- 수정: /home/sandbox/projects/Guitar_Project/api/main.py
- 수정: /home/sandbox/projects/Guitar_Project/Tunerapp/index.html
- 수정: /home/sandbox/projects/Guitar_Project/pipeline-state.md
- 생성: /home/sandbox/projects/Guitar_Project/unity-specs/B4a-songdata-cs.md
- 생성: /home/sandbox/projects/Guitar_Project/unity-specs/B4b-songselector-ui.md
- 생성: /home/sandbox/projects/Guitar_Project/unity-specs/B5a-practiceconfig.md
- 생성: /home/sandbox/projects/Guitar_Project/unity-specs/B5b-session-update.md
- 생성: /home/sandbox/projects/Guitar_Project/unity-specs/B6-transpose.md

### Claude Code Unity 구현 (2026-03-04)
- 생성: Assets/Scripts/Data/SectionMarker.cs
- 생성: Assets/Scripts/Data/SongData.cs
- 생성: Assets/Scripts/Data/SongDatabase.cs
- 생성: Assets/Scripts/Data/PracticeConfig.cs
- 생성: Assets/Scripts/UI/SongSelectorPanel.cs
- 생성: Assets/Scripts/UI/PracticeConfigPanel.cs
- 생성: Assets/Scripts/UI/TransposePanel.cs
- 수정: Assets/Scripts/Data/ChordDatabase.cs (GetTransposed, TransposeSemitones, CHROMATIC_ROOT_MAP)
- 수정: Assets/Scripts/Timing/TimingEngine.cs (CountInMeasures, IsCountInActive, SetBPM 래퍼)
- 수정: Assets/Scripts/Session/PracticeSessionController.cs (PracticeConfig 연동, tempoRamp, autoTranspose, loopCount)
- 컴파일 에러: 0개 확인

---

## 참고

- 파이프라인 가이드: `/home/sandbox/Guitar_Project/openclaw_cron_pipeline.md`
- cron 스케줄: `/home/sandbox/Guitar_Project/guitar-pipeline.cron.json`
- Unity spec 경로: `/home/sandbox/Guitar_Project/unity-specs/`
- API 경로: `/home/sandbox/Guitar_Project/api/main.py`
- Tunerapp 경로: `/home/sandbox/Guitar_Project/Tunerapp/`
