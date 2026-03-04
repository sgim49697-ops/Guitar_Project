## B-5-a — PracticeConfig + CountIn 확장 Unity Spec

### 컨텍스트 확인
- `pipeline-state.md`에서 B-4-b 완료 확인
- `TimingEngine.cs` 현재 이벤트/루프 구조 확인 (`OnBeat`, `OnMeasureStart`, `BPM`)
- `PracticeSessionController.cs` 현재 코드 전환 시점 확인 (`ShowChord`, `PreviewNextChord`)
- `api/main.py`의 `PracticeConfig` 스키마 확인

---

## 1) 신규 데이터 모델: `Assets/Scripts/Data/PracticeConfig.cs`

`ScriptableObject` 또는 `[System.Serializable]` 클래스(또는 병행)로 추가.

### 필수 필드
- `tempoRampBpm: int` (`0` = 비활성)
- `loopCount: int` (`-1` = 무한)
- `countInEnabled: bool`
- `countInBeats: int` (기본값 `4`)
- `autoTransposeEnabled: bool`
- `transposeSemitonesPerLoop: int`

### 기본값 권장
- `tempoRampBpm = 0`
- `loopCount = -1`
- `countInEnabled = true`
- `countInBeats = 4`
- `autoTransposeEnabled = false`
- `transposeSemitonesPerLoop = 0`

### 유효성 권장
- `countInBeats >= 0`
- `loopCount == -1 || loopCount > 0`

---

## 2) `TimingEngine.cs` 수정 사항

### 추가 프로퍼티
- `public int CountInMeasures { get; }`
  - 계산식: `countInEnabled ? Mathf.CeilToInt((float)countInBeats / beatsPerMeasure) : 0`
- (권장) `public bool IsCountInActive { get; }`
  - Count-In 구간 중인지 외부(`PracticeSessionController`)에서 가드하기 위한 상태

### Count-In 동작 규칙
1. `StartPlayback(...)` 시 Count-In 활성 조건이면 Count-In 상태로 시작
2. Count-In 동안도 내부 beat clock(`nextBeatTime`)은 동일하게 전진
3. Count-In 종료 직후 실연주 `measure 0` 진입
4. 기존 오디오 스케줄링 while 루프 구조는 유지

### 호환성(필수)
- 기존 `SetBPM(float)`, `OnBeat`, `OnMeasure` 이벤트 계약은 유지
- 현재 코드 기준으로는 `OnMeasureStart`를 유지하고 필요 시 `OnMeasure` alias/event wrapper 제공
- 현재 `BPM` 프로퍼티 사용 흐름은 그대로 유지
  - `SetBPM(float)` 호출처가 있으면 wrapper로 `BPM`에 위임해 하위호환 보장

---

## 3) Count-In 중 `FretboardRenderer` 업데이트 차단

대상: `Assets/Scripts/Session/PracticeSessionController.cs`

### 차단 포인트
- `OnBeatTick()` 내 `PreviewNextChord(...)`
- `OnMeasureChanged()` 내 `ShowChord(...)`
- 세션 시작 직후 즉시 `ShowChord(0)` 호출

### 적용 규칙
1. `timingEngine.IsCountInActive == true`이면
   - `OnBeatTick()` 즉시 return
   - `OnMeasureChanged()` 즉시 return
2. 시작 시 첫 코드 표시
   - `countInEnabled == false`: 기존처럼 즉시 `ShowChord(0)`
   - `countInEnabled == true`: 즉시 표시하지 않고 Count-In 종료 후 첫 실마디에서 `ShowChord(0)`

### 기대 결과
Count-In 동안은 메트로놈/카운트 UI만 동작하고, 지판 하이라이트/프리뷰는 실제 연주 구간에서만 표시.

---

## 4) 기존 API 변경 없이 확장하는 방법

서버(`api/main.py`)의 현재 `PracticeConfig` 필드:
- `tempoRampBpm`
- `loopCount`
- `countInEnabled`
- `autoTransposeEnabled`
- `transposeSemitonesPerLoop`

### 확장 원칙
- 백엔드 API 스키마/엔드포인트 변경 없음
- Unity 로컬 확장 필드로만 `countInBeats` 도입

### 구현 가이드
1. API 요청/응답 직렬화는 기존 필드 그대로 유지
2. Unity에서 `countInBeats` 누락 시 기본값 `4` 자동 적용
3. 다음 엔드포인트 계약 불변
   - `POST /api/session/start`
   - `POST /api/session/loop-complete`
   - `GET /api/session/state`
   - `POST /api/session/end`

요약: **서버 무변경 + Unity 기능 확장 방식**으로 반영.

---

## 완료 기준
- [ ] `PracticeConfig.cs` 생성 및 필드/기본값 반영
- [ ] `TimingEngine`에 `CountInMeasures`(및 권장 `IsCountInActive`) 추가
- [ ] Count-In 중 프렛보드 업데이트 차단 확인
- [ ] `OnBeat`, `OnMeasure`(또는 `OnMeasureStart` 호환), `SetBPM(float)` 계약 유지
- [ ] API 계약 변경 없음 확인
