## B-6 — Chord Transpose (Key Shift) Unity Spec

### 컨텍스트 확인
- `pipeline-state.md`에서 B-5-b 완료 상태 확인
- `unity-specs/B5b-session-update.md`의 세션/루프 확장 방향 확인
- `Assets/Scripts/Data/ChordDatabase.cs` 현재 구조 확인 (`GetChord`, `GetAllChordNames`)
- `Assets/Scripts/Data/FretPosition.cs` 확인 (`fretIndex`, `isMuted`, `fingerNumber`)

---

## 목표
사용자가 UI에서 반음 단위(+1/-1)로 키를 이동할 수 있도록 하고, 기존 코드 데이터는 보존한 채 **렌더링용 전조 복사본**을 반환한다.

핵심 원칙:
1. 원본 `ChordDatabase.chords`는 절대 파괴적으로 수정하지 않는다.
2. 전조 결과는 임시 복사본(`ChordData`)으로 반환한다.
3. 기존 `FretboardRenderer.HighlightChord()` 호출부는 깨지지 않도록 API 호환성을 유지한다.

---

## 1) `Assets/Scripts/Data/ChordDatabase.cs` 수정 spec

### 1-1. CHROMATIC_ROOT_MAP 추가 (필수)
근음 문자열을 반음 인덱스로 매핑하는 상수 맵을 추가한다.

```csharp
private static readonly Dictionary<string, int> CHROMATIC_ROOT_MAP = new()
{
    { "C", 0 }, { "C#", 1 }, { "Db", 1 },
    { "D", 2 }, { "D#", 3 }, { "Eb", 3 },
    { "E", 4 },
    { "F", 5 }, { "F#", 6 }, { "Gb", 6 },
    { "G", 7 }, { "G#", 8 }, { "Ab", 8 },
    { "A", 9 }, { "A#", 10 }, { "Bb", 10 },
    { "B", 11 }
};
```

권장: 역매핑용 배열도 추가 (`0~11 -> canonical root`) 하여 이름 재구성 시 사용.

---

### 1-2. `TransposeSemitones(int n)` 추가 (필수)
모든 `ChordData`의 `FretPosition.fretIndex`를 `n`만큼 오프셋하는 내부 유틸리티 메서드를 추가한다.

주의: `FretPosition`은 `struct`이므로 리스트 요소를 직접 수정할 때 값 복사 이슈를 피하도록 재할당 패턴을 사용한다.

예시 패턴:
```csharp
for (int i = 0; i < chord.fretPositions.Count; i++)
{
    var pos = chord.fretPositions[i];
    if (!pos.isMuted)
    {
        pos.fretIndex = ApplyTransposeToFret(pos.fretIndex, n);
    }
    chord.fretPositions[i] = pos;
}
```

> 이 메서드는 **원본 DB 전체를 바꾸는 위험 API**이므로, 실제 런타임 UI 흐름에서는 직접 호출하지 말고 `GetTransposed(...)` 내부/테스트 유틸로 제한하는 것을 권장.

---

### 1-3. `GetTransposed(string chordName, int semitones)` 추가 (필수)
원본 `ChordData`를 유지하면서 전조된 임시 복사본을 반환한다.

동작:
1. `GetChord(chordName)`로 원본 조회
2. null이면 null 반환
3. 깊은 복사본 생성 (`ChordData` + 내부 `fretPositions` 새 List)
4. 복사본의 모든 포지션에 대해 전조 오프셋 적용
5. 필요 시 `chordName`도 전조된 루트명으로 갱신
6. 복사본 반환

핵심:
- `chords` 리스트와 원본 요소는 변경 금지
- 호출자는 렌더링에만 복사본 사용

---

## 2) 프렛 오프셋 알고리즘 명세 (개방현 처리 포함)

`fretIndex` 전조 함수:

```csharp
private int ApplyTransposeToFret(int fret, int semitones)
```

규칙:
1. `isMuted == true` 인 줄은 무조건 변경하지 않음
2. `fret > 0` 인 일반 프렛은 `fret + semitones`
3. `fret == 0` 개방현은 아래 정책 중 하나를 채택 (권장: 정책 A)

### 정책 A (권장, 일관성 우선)
- 개방현도 동일하게 오프셋: `0 + semitones`
- 단, 음수 결과는 0 미만 불가이므로 clamp

```csharp
int shifted = fret + semitones;
return Mathf.Clamp(shifted, 0, MAX_FRET);
```

장점:
- 시각적/수학적 규칙이 단순
- +전조 시 open string이 자연스럽게 fretted note로 이동

주의:
- -전조 시 open string은 여전히 0에 머물 수 있음 (물리적 한계 반영)

### 정책 B (대안, 개방현 고정)
- `fret == 0`이면 항상 0 유지
- 나머지 fret만 오프셋

장점: 기존 오픈 코드 형태 보존
단점: 실제 음고 전조 관점에서 불일치 가능

**이번 구현 권장안: 정책 A + clamp(0~MAX_FRET)**

추가 경계:
- `MAX_FRET`는 프로젝트 렌더 가능한 최대 프렛(예: 20)로 상수화
- 범위 밖은 clamp 처리하여 렌더러 out-of-range 방지

---

## 3) 신규 UI — `Assets/Scripts/UI/TransposePanel.cs`

### 3-1. 목적
사용자가 현재 키를 확인하고 반음 단위로 전조를 제어하는 패널.

### 3-2. 필수 UI 요소
- `+1` 버튼
- `-1` 버튼
- 현재 키 텍스트 (`Key: C` 형태)
- `Reset` 버튼 (원래 키로 복귀)

### 3-3. 직렬화 필드 예시
```csharp
[SerializeField] private ChordDatabase chordDatabase;
[SerializeField] private FretboardRenderer fretboardRenderer;
[SerializeField] private TMPro.TMP_Text keyText;
[SerializeField] private UnityEngine.UI.Button plusButton;
[SerializeField] private UnityEngine.UI.Button minusButton;
[SerializeField] private UnityEngine.UI.Button resetButton;
```

내부 상태:
```csharp
private int currentSemitoneOffset = 0;
private string baseChordName; // 현재 세션/선택 코드의 원본 이름
```

### 3-4. 동작 규칙
- `+1` 클릭: `currentSemitoneOffset += 1`
- `-1` 클릭: `currentSemitoneOffset -= 1`
- `Reset`: `currentSemitoneOffset = 0`
- offset 변경 시:
  1. `GetTransposed(baseChordName, currentSemitoneOffset)` 호출
  2. 반환된 임시 `ChordData`를 `FretboardRenderer.HighlightChord(...)`에 전달
  3. `keyText`를 현재 키로 갱신 (`Key: C#` 등)

권장:
- UI 표시용 offset 범위는 `[-12, +12]` 또는 순환 표시(모듈로 12)

---

## 4) `FretboardRenderer.HighlightChord()` 호환성 유지

호환성 요구:
- 기존 호출부에서 `HighlightChord(ChordData chord)` 또는 `HighlightChord(string chordName)`를 사용 중일 가능성이 있으므로, **기존 public API를 제거/변경하지 않는다.**

권장 방식:
1. 기존 `HighlightChord(string chordName)` 유지
2. 오버로드 추가 가능:
   - `HighlightChord(ChordData chordData)`
3. `TransposePanel`은 전조 복사본을 생성해 `HighlightChord(ChordData)` 경로를 사용
4. 기존 흐름(원본 chordName 기반 렌더)은 그대로 동작

결과:
- 신규 전조 기능 추가
- 기존 코드 진행/하이라이트 로직과 충돌 최소화

---

## 5) 구현 체크리스트
- [ ] `ChordDatabase.cs`에 `CHROMATIC_ROOT_MAP` 추가
- [ ] `TransposeSemitones(int n)` 추가 (fretIndex 오프셋 로직)
- [ ] `GetTransposed(string chordName, int semitones)` 추가 (deep copy + 원본 보존)
- [ ] 개방현(`fret=0`) 처리 정책 명시/적용 (권장: 정책 A + clamp)
- [ ] `Assets/Scripts/UI/TransposePanel.cs` 신규 작성
- [ ] `+1/-1`, `Key: X`, `Reset` UI 동작 연결
- [ ] `FretboardRenderer.HighlightChord()` 기존 API 호환성 유지

---

## Claude Code 적용 요청 메모
실제 적용 전 아래 파일을 반드시 먼저 읽고 반영해 주세요.
- `Assets/Scripts/Data/ChordDatabase.cs`
- `Assets/Scripts/Data/FretPosition.cs`
- `Assets/Scripts/UI/FretboardRenderer.cs`
- (존재 시) 현재 코드 선택/렌더를 담당하는 Panel/Controller 스크립트

적용 후에는 Unity Console에서:
1. 원본 코드 조회 반복 시 데이터 변형이 누적되지 않는지
2. +1/-1/Reset 반복 시 fret 표시 범위 오류가 없는지
3. 기존 HighlightChord 호출 경로가 깨지지 않는지
를 확인해 주세요.
