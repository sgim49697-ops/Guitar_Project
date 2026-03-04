## Unity Spec — B-4-b SongSelectorPanel UI

참조 컨텍스트:
- `pipeline-state.md` (B-4-a 완료 확인)
- `unity-specs/B4a-songdata-cs.md` (SongData/SongDatabase API)
- `Assets/Scripts/UI/ChordSelector.cs`
- `Assets/Scripts/Session/PracticeSessionController.cs`

---

### 1) 신규 스크립트: `Assets/Scripts/UI/SongSelectorPanel.cs`

#### 목적
- `SongDatabase`의 곡 목록을 UI에 표시
- 곡 선택 시 코드 진행을 `ChordProgressionPanel`에 반영
- 적용 버튼으로 `PracticeSessionController.StartCustomSession()` 실행
- 기존 `ChordSelector`와 교체/병렬 공존 가능하게 구성

#### 필수 SerializeField
```csharp
[SerializeField] private SongDatabase songDatabase;
[SerializeField] private PracticeSessionController sessionController;
[SerializeField] private ChordProgressionPanel progressionPanel;
```

#### 권장 UI SerializeField
```csharp
[SerializeField] private TMP_Dropdown songDropdown;      // 드롭다운 모드
[SerializeField] private RectTransform listContent;      // 스크롤 리스트 모드 Content
[SerializeField] private Button listItemPrefab;          // 곡 1개 버튼 프리팹
[SerializeField] private Button applyButton;             // 선택 곡 적용
[SerializeField] private Button clearButton;             // 선택 해제
[SerializeField] private BPMControl bpmControl;          // BPM 연동(없으면 기본 BPM 사용)
[SerializeField] private bool useDropdown = true;        // true=Dropdown, false=ScrollList
```

#### 내부 상태(권장)
```csharp
private SongData selectedSong;
private readonly List<Button> spawnedItems = new();
```

#### 핵심 메서드
```csharp
public void RefreshSongList();
public void OnSongSelected(SongData song);
public SongData GetSelectedSong();
public void ApplySelectedSong();
public void ClearSelection();
```

#### 필수 동작: OnSongSelected
```csharp
public void OnSongSelected(SongData song)
{
    selectedSong = song;
    if (selectedSong == null || selectedSong.chordProgression == null)
        return;

    // 요구사항: 곡 선택 즉시 진행 패널 반영
    progressionPanel.SetProgression(selectedSong.chordProgression);
}
```

#### Apply 동작(권장)
```csharp
public void ApplySelectedSong()
{
    if (selectedSong == null || selectedSong.chordProgression == null || selectedSong.chordProgression.Count == 0)
        return;

    int bpm = bpmControl != null ? bpmControl.GetCurrentBPM() : 90;

    sessionController.StopSession();
    progressionPanel.SetProgression(selectedSong.chordProgression);
    sessionController.StartCustomSession(selectedSong.chordProgression, bpm);
}
```

---

### 2) 곡 목록 UI 방식 (드롭다운/스크롤 리스트)

#### A. Dropdown 모드
- `songDatabase.GetAll()` 결과를 title로 옵션 생성
- `onValueChanged` → 해당 인덱스 `SongData`로 `OnSongSelected()` 호출

#### B. Scroll List 모드
- `listItemPrefab`를 `listContent` 하위로 동적 생성
- 버튼 텍스트 = `song.title`
- 버튼 클릭 → `OnSongSelected(song)`

빈 목록 처리:
- Dropdown: `"No Songs"` placeholder 1개
- ScrollList: `"No Songs"` 비활성 텍스트 아이템 1개

---

### 3) 기존 ChordSelector와 공존 방법 (필수)

#### 옵션 A: 병렬(권장)
- `ChordSelector`(수동 코드 큐) + `SongSelectorPanel`(곡 선택) 동시 배치
- 둘 다 같은 `PracticeSessionController`, `ChordProgressionPanel` 참조
- 최종 적용 시점(Apply 클릭한 쪽)이 현재 세션을 덮어씀

권장 모드 전환 규칙:
1. Song 모드 활성화 시 ChordSelector 버튼 interactable off (선택)
2. Chord 모드 복귀 시 Song 선택 유지 또는 clear (정책 선택)
3. 세션 시작 전 항상 `StopSession()` 호출

#### 옵션 B: 교체
- ChordSelector 오브젝트를 비활성화/제거하고 SongSelectorPanel만 사용
- 단, 레거시 코드 호환 위해 아래 기존 API는 유지:
  - `PracticeSessionController.StartCustomSession(List<string>, int)`
  - `ChordSelector.GetSelectedChords()`

---

### 4) Canvas 레이아웃 위치 (필수)

기준:
- 기존 SelectorBar 사용 영역: `y=122~170`

권장 배치:
- SongSelectorPanel 신규 영역: `y=174~236`
- 기존 ChordSelector는 `y=122~170` 유지
- 두 패널을 상하 2열로 배치해 겹침 방지

---

### 5) RectTransform anchor/offset 수치 (필수)

기준 Canvas(1080x1920, Scale With Screen Size 가정)

#### SongSelectorPanel (Root)
- Anchor Min = `(0.5, 0)`
- Anchor Max = `(0.5, 0)`
- Pivot = `(0.5, 0)`
- Anchored Position = `(0, 174)`
- Size Delta = `(980, 62)`

#### SongDropdown (Dropdown 모드)
- Anchor Min = `(0, 0.5)`
- Anchor Max = `(0, 0.5)`
- Pivot = `(0, 0.5)`
- Anchored Position = `(16, 0)`
- Size Delta = `(420, 48)`

#### ScrollView (List 모드)
- Anchor Min = `(0, 0)`
- Anchor Max = `(1, 1)`
- Offset Min = `(12, 6)`
- Offset Max = `(-220, -6)`

#### ApplyButton
- Anchor Min/Max = `(1, 0.5)`
- Pivot = `(1, 0.5)`
- Anchored Position = `(-110, 0)`
- Size Delta = `(96, 44)`

#### ClearButton
- Anchor Min/Max = `(1, 0.5)`
- Pivot = `(1, 0.5)`
- Anchored Position = `(-10, 0)`
- Size Delta = `(96, 44)`

---

### 6) 기존 public API 유지 (필수)

다음 API는 시그니처 변경/삭제 금지:
1. `PracticeSessionController.StartCustomSession(List<string> chordNames, int bpm)`
2. `ChordSelector.GetSelectedChords()`

적용 원칙:
- SongSelectorPanel은 `StartCustomSession()`을 그대로 호출
- ChordSelector는 공존 시 기존 기능 유지
- 교체 시에도 컴파일 호환을 위해 `GetSelectedChords()` 호출부 정리 전까지 유지

---

### 7) Inspector 연결 필드 목록 (필수)

`SongSelectorPanel` 컴포넌트 연결 항목:

필수:
1. **Song Database** → `SongDatabase.asset`
2. **Session Controller** → `PracticeSessionController` 오브젝트
3. **Progression Panel** → `ChordProgressionPanel` 오브젝트

권장:
4. **Song Dropdown** (`TMP_Dropdown`)
5. **List Content** (`ScrollView/Viewport/Content`)
6. **List Item Prefab** (`Button + TMP_Text`)
7. **Apply Button**
8. **Clear Button**
9. **BPM Control** (선택)
10. **Use Dropdown** bool

---

### 8) 구현 주의사항
- `songDatabase == null`이면 에러 로그 후 입력 비활성화
- 이벤트 중복 등록 방지 (`RemoveAllListeners` 또는 명시적 해제)
- `selectedSong` null/empty 방어
- 모드 전환 시 세션 중복 시작 방지 (`StopSession()` 선호)
- 장르/태그 필터 UI는 추후 확장(B-4-b 범위 외)
