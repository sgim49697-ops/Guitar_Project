## B-5-b — PracticeSessionController 세션 업데이트 + PracticeConfigPanel UI Spec

### 컨텍스트 확인
- `pipeline-state.md`에서 B-5-a 완료 확인
- `unity-specs/B5a-practiceconfig.md`에서 `PracticeConfig` 필드 확인
- `PracticeSessionController.cs` 현재 `OnLoopComplete()`(무인자), `StartCustomSession(List<string>, int)` 확인
- `TransportControls.cs`에서 `StartCustomSession(selected, bpm)` 호출 확인

---

## 1) `Assets/Scripts/Session/PracticeSessionController.cs` 수정 spec

### 1-1. `PracticeConfig` 참조 추가 (필수)
```csharp
[SerializeField] private PracticeConfig practiceConfig;
```

권장 내부 상태:
- `private int completedLoopCount;`

세션 시작 시 초기화:
- `completedLoopCount = 0;`

---

### 1-2. `OnLoopComplete(int)` 처리 추가 (필수)
이벤트 핸들러를 아래 형태로 맞춘다.

```csharp
private void OnLoopComplete(int loopIndex)
```

> `TimingEngine.OnLoopComplete`가 아직 무인자라면 `Action<int>`로 확장하거나, 래퍼에서 loopIndex를 계산해 전달.

#### A) tempoRamp 적용
조건:
- `practiceConfig != null`
- `practiceConfig.tempoRampBpm > 0`

동작:
- 루프 완료마다 BPM 증가
  - 예: `timingEngine.BPM += practiceConfig.tempoRampBpm;`

#### B) autoTranspose 적용
조건:
- `practiceConfig.autoTransposeEnabled == true`
- `practiceConfig.transposeSemitonesPerLoop != 0`

동작:
- 루프 완료마다 `transposeSemitonesPerLoop`만큼 전조 적용
- 적용 경로(프로젝트 구조에 맞는 방식 택1):
  1. 기존 Transposer/KeyManager 컴포넌트 호출
  2. `currentProgression` chordName을 반음 이동된 진행으로 재구성
- 전조 후 `progressionPanel` 텍스트/활성 코드도 동일 기준으로 동기화

#### C) loopCount 도달 시 자동 종료
조건:
- `practiceConfig.loopCount > 0` (`-1` = 무한)
- 완료 루프 수가 `loopCount`에 도달

동작:
- 즉시 `StopSession()` 호출

---

### 1-3. `StartCustomSession(List<string>, float)` 시그니처 유지 (필수)
요구사항에 맞춰 float 시그니처를 유지한다.

권장 구현:
```csharp
public void StartCustomSession(List<string> chordNames, float bpm)
{
    StartCustomSession(chordNames, Mathf.RoundToInt(bpm));
}

public void StartCustomSession(List<string> chordNames, int bpm)
{
    // 기존 로직 유지
}
```

- 기존 `TransportControls`(int bpm 전달)는 무수정 호환
- 외부 호출부가 float 사용 중이어도 하위 호환

---

## 2) 신규 파일 — `Assets/Scripts/UI/PracticeConfigPanel.cs`

### 목적
PracticeConfig를 UI에서 직접 수정하고 Apply로 즉시 반영한다.

### 필수 컴포넌트
- `tempoRamp` 슬라이더 (`Slider`, 범위 `0~20`)
- `loopCount` 입력 필드 (`-1 = ∞` 안내 포함)
- `countIn` 토글
- `autoTranspose` 토글
- 반음 수 입력 필드
- `Apply` 버튼

### 직렬화 필드 예시
```csharp
[SerializeField] private PracticeConfig practiceConfig;
[SerializeField] private Slider tempoRampSlider;
[SerializeField] private TMP_InputField loopCountInput;
[SerializeField] private Toggle countInToggle;
[SerializeField] private Toggle autoTransposeToggle;
[SerializeField] private TMP_InputField semitoneInput;
[SerializeField] private Button applyButton;
```

### 동작 규칙
1. 패널 초기화 시 `practiceConfig` 현재값을 UI에 로드
2. Apply 클릭 시 입력값 검증 후 `practiceConfig`에 반영
   - `loopCount == -1 || loopCount > 0`
   - 반음 입력 파싱 실패 시 `0`
3. `autoTranspose`가 OFF면 반음 입력 비활성화
4. Apply 후 디버그 로그 또는 간단한 성공 피드백 표시

### 필드 매핑
- `tempoRampSlider.value` → `practiceConfig.tempoRampBpm`
- `loopCountInput.text` → `practiceConfig.loopCount`
- `countInToggle.isOn` → `practiceConfig.countInEnabled`
- `autoTransposeToggle.isOn` → `practiceConfig.autoTransposeEnabled`
- `semitoneInput.text` → `practiceConfig.transposeSemitonesPerLoop`

---

## 3) Canvas 배치 spec (기존 UI 비충돌)

- 패널 위치: 우측 상단(Top-Right Anchor)
- 권장 크기: 약 `360 x 300`
- 안전 여백: 상단/우측 `16~24px`

비충돌 기준:
- 하단 `TransportControls`와 겹치지 않음
- 상단/중앙 `ChordProgressionPanel`와 겹치지 않음
- 좁은 화면에서는 접기/펼치기 토글(선택) 제공

---

## 완료 체크리스트
- [ ] `PracticeSessionController`에 `[SerializeField] PracticeConfig` 추가
- [ ] `OnLoopComplete(int)`에서 `tempoRamp`, `autoTranspose` 처리
- [ ] `loopCount` 도달 시 `StopSession()` 자동 호출
- [ ] `StartCustomSession(List<string>, float)` 시그니처 유지
- [ ] `PracticeConfigPanel.cs` 신규 작성
- [ ] tempoRamp(0~20), loopCount(-1=∞), countIn, autoTranspose+반음, Apply 반영
- [ ] Canvas 배치가 기존 UI와 비충돌
