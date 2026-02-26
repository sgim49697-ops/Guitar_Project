# iReal Pro 완전 분석 — 벤치마킹 레퍼런스

> 작성일: 2026-02-26
> 목적: Guitar Practice 앱을 iReal Pro 수준으로 발전시키기 위한 기능/구조 분석
> 참고: 현재 프로젝트와의 갭 분석 및 단계별 구현 로드맵 포함

---

## 1. iReal Pro 핵심 컨셉

iReal Pro는 **가상 리듬 섹션(Virtual Rhythm Section)** 앱이다.
악보(코드 진행)를 입력하면 베이스·하모니·드럼이 자동 반주를 생성하고, 연주자는 그 위에서 즉흥 연주 또는 연습을 한다.

원래 재즈 솔로이스트(기타리스트, 피아니스트, 관악기 연주자)가 코드 진행 위에서 솔로를 연습하기 위해 만들어졌으나, 현재는 초보자부터 전문 음악가까지 모든 수준의 연주자가 사용하는 범용 연습 도구로 성장했다.

**플랫폼:** iOS, Android, macOS
**가격:** 유료 앱 (일부 스타일 팩은 추가 인앱 구매)
**커뮤니티:** forums.irealpro.com — 전 세계 사용자가 악보 공유

---

## 2. 전체 기능 구조 (Feature Map)

```
iReal Pro
│
├── [A] Song Library         곡 저장소 및 플레이리스트 관리
├── [B] Chart Editor         코드 악보 편집기
├── [C] Player Engine        재생 엔진 (박자·코드 진행 처리)
├── [D] Accompaniment        반주 스타일 + 오디오 생성
├── [E] Mixer                악기별 볼륨/음색/뮤트 조절
├── [F] Chord Diagrams       운지 다이어그램 (기타/피아노/우쿨렐레)
└── [G] Practice Mode        자동 연습 보조 (템포/전조 자동화)
```

---

## 3. [A] Song Library — 곡 저장소

### 3.1 내장 콘텐츠
- **1,400+ 곡** 기본 내장 (Jazz 표준, 팝, 보사노바, 블루스 등)
- 장르별 기본 플레이리스트 제공:
  - Jazz Standards, Bossa Nova, Pop, Latin, Christmas, Classical 등
- 커뮤니티 포럼(forums.irealpro.com)에서 **수만 곡 무료 다운로드** 가능
  (계정 없이 검색 및 다운로드 가능)

### 3.2 플레이리스트 시스템
- 플레이리스트 생성·편집·삭제·순서 변경
- 플레이리스트 단위 **공유**: iReal Pro 포맷(URL) 또는 **PDF 북릿** 내보내기
- 포럼에 직접 업로드 가능
- 플레이리스트 내 곡 순서 드래그로 변경

### 3.3 곡 검색/필터
- 제목·작곡가·장르·키·스타일로 검색
- 정렬: 제목순, 최근 추가순
- 포럼 검색: 장르 탭 (Jazz / Pop / Latin / Christmas / Brazilian / Salsa 등)

### 3.4 우리 프로젝트에서 구현 방향
```csharp
// SongDatabase ScriptableObject
[CreateAssetMenu]
public class SongDatabase : ScriptableObject {
    public List<SongData> songs;
    public List<Playlist> playlists;
}

[Serializable]
public class SongData {
    public string title;
    public string composer;
    public string style;         // "Medium Swing", "Bossa Nova" 등
    public string key;           // "C", "Bb", "F#" 등
    public int bpm;
    public string timeSignature; // "4/4", "3/4" 등
    public List<string> chordProgression; // ["C^7","A-7","D-9","G7"]
    public List<SectionMarker> sections; // [*A, *B, *V, *i]
    public string genre;
    public string[] tags;
}
```

---

## 4. [B] Chart Editor — 코드 악보 편집기

### 4.1 그리드 구조
- **16셀(cell) × 최대 12줄** 고정 그리드
- 1셀 = 기본 1박자 (박자표 따라 달라짐)
- 4/4에서 1마디 = 4셀, 3/4에서 1마디 = 3셀
- 각 셀에 코드 1개 또는 공백 입력
- 줄 사이에 섹션 레이블 배치 가능

### 4.2 코드 표기법 (iReal Pro 코드 기호)

| 기호 | 의미 | 예시 |
|------|------|------|
| `^` | Major 7th | `C^7` = Cmaj7 |
| `-` | Minor | `A-7` = Am7 |
| `h` | Half-diminished (ø) | `Bh7` = Bm7b5 |
| `o` | Diminished | `Co` = Cdim |
| `+` | Augmented | `C+` = Caug |
| `^` (단독) | Major (단순) | `C^` = Cmaj |
| `sus` | Suspended | `G7sus` |
| `alt` | Altered | `G7alt` |
| `/X` | 베이스 음 지정 | `C^7/E` |
| `(X)` | 대체 코드 (소형 상단 표시) | `(Db^7/F)` |
| `n` | No Chord (N.C.) | |

**확장 코드 숫자 표기:**
- 6, 7, 9, 11, 13 — `C6`, `C7`, `C9`, `C11`, `C13`
- `b` / `#` — 변화음: `C7b9`, `G7#11`, `C9#11`
- `add` — 추가음: `Cadd9`
- `sus2`, `sus4` — 서스펜디드

### 4.3 구조 기호 (악보 기호)

| 기호 | 의미 |
|------|------|
| `\|` | 단일 마디선 |
| `[` | 이중 마디선 (시작) |
| `]` | 이중 마디선 (끝) |
| `{` | 반복 시작 (리피트 열기) |
| `}` | 반복 끝 (리피트 닫기) |
| `Z` | 최종 이중 마디선 (곡 끝) |
| `x` | 1마디 반복 (%) |
| `r` | 2마디 반복 |
| `S` | 세뇨 (Segno, %) |
| `Q` | 코다 (Coda, ⊕) |
| `f` | 페르마타 (Fermata) |

### 4.4 섹션/리허설 마크

| 기호 | 의미 |
|------|------|
| `*A` | A 섹션 |
| `*B` | B 섹션 |
| `*C` | C 섹션 |
| `*D` | D 섹션 |
| `*V` | Verse |
| `*i` | Intro |

### 4.5 박자표

| 기호 | 박자 |
|------|------|
| `T44` | 4/4 |
| `T34` | 3/4 |
| `T24` | 2/4 |
| `T54` | 5/4 |
| `T64` | 6/4 |
| `T74` | 7/4 |
| `T22` | 2/2 (Cut time) |
| `T68` | 6/8 |
| `T78` | 7/8 |
| `T98` | 9/8 |
| `T12` | 12/8 |

박자표는 마디 중간에도 변경 가능 — 곡 내 여러 번 등장 가능.

### 4.6 반복 구조 (D.S. / D.C. al Coda)
- **D.C. al Coda**: 처음으로 돌아가서 코다 기호(Q)에서 코다 섹션으로 점프
- **D.S. al Coda**: 세뇨(S) 기호로 돌아가서 코다 기호에서 점프
- **1st/2nd Ending**: `{ }` 반복 내에서 다른 마디 연주 (1번 마디, 2번 마디)

### 4.7 irealbook:// URL 포맷 (공개 포맷)

```
irealbook://[곡제목]=[작곡가 성 이름]=[스타일]=[키]=[n]=[코드진행]

예시 (Autumn Leaves):
irealbook://Autumn Leaves=Kosma Joseph=Medium Swing=Bb=n=
T44*A{C^7 |A-7 |D-9 |G7#5 |C^7 |A-7 |D-9 |G7#5 }
*B{Eb^7 |C-7 |F-7 |Bb7 |Eb^7 |C-7 |F-7 |Bb7 }Z
```

> **주의:** 최신 `irealb://` 포맷은 난독화(obfuscation) 처리됨.
> 공개 포맷은 `irealbook://`이며 파싱 라이브러리 다수 존재
> (GitHub: sciurius/perl-Data-iRealPro, eigenben/irealb_parser 등)

---

## 5. [C] Player Engine — 재생 엔진

### 5.1 박자 엔진
- DSP 기반 정밀 타이밍 (오디오 버퍼 레벨)
- 마디 단위 코드 전환 이벤트
- 비트 단위 인디케이터 업데이트
- 거의 모든 박자표 지원, **마디 단위 박자표 변환** 가능
- 카운트인(Count-in): 재생 전 1마디 클릭 준비 (최근 버전에서 키 루트 음 재생 옵션 추가)

### 5.2 재생 제어
- Play / Stop / Pause
- 루프 횟수 설정 (1회, N회, 무한 반복)
- BPM 조절: 슬라이더 + 텍스트 입력 (범위 대략 30~400 BPM)
- 조옮김: ±반음 버튼 또는 키 선택 드롭다운
- **두 섹션 연결 재생** (스타일 도중 변경 가능)

### 5.3 악보 스크롤
- 재생 중 현재 마디 자동 하이라이트
- 현재 코드 + 다음 코드 동시 표시 가능
- 화면 크기에 따라 전체 보기 / 스크롤 뷰 전환

---

## 6. [D] Accompaniment — 반주 스타일 시스템

### 6.1 기본 내장 스타일 (51개)

**Jazz 계열:**
- Medium Swing, Up Tempo Swing, Jazz Ballad
- Guitar Trio, Doo Doo Cats, New Orleans Swing
- Bossa Nova, Bossa/Swing

**Pop/Rock 계열:**
- Rock, Pop, Folk, Country, Bluegrass

**라틴/월드 계열:**
- Reggae, Funk, R&B, Gospel, Samba

**추가 구매 스타일 팩:**
- Blues 12개, Brazilian 11개, Salsa 17개

### 6.2 악기 구성 (스타일별 다름)
- **하모니 악기**: Acoustic Piano, Fender Rhodes, Electric Piano, Vibraphone, Organ, Acoustic Guitar, Electric Guitar
- **베이스**: Acoustic Bass, Electric Bass
- **드럼/타악기**: Standard Drum Kit (2025년부터 실제 드럼 녹음 샘플 적용, Jazz 스타일 우선)

### 6.3 반주 생성 방식
- 스타일 파일 = 리듬 패턴 + 코드 보이싱 규칙의 조합
- 코드명을 받으면 해당 스타일의 보이싱 규칙으로 실시간 화음 생성
- **Organic Variation**: 동일 코드라도 매 반복마다 약간씩 다른 보이싱/리듬으로 연주 (기계적 반복 방지)
- **Embellishment 옵션**: 코드 보이싱에 장식음 추가 여부 (믹서에서 토글)

### 6.4 스타일 중간 변경
- 한 곡 내에서 A 섹션은 Swing, B 섹션은 Ballad처럼 **섹션별 스타일 변경** 가능
- 편집기에서 섹션 마커와 함께 스타일 지정

---

## 7. [E] Mixer — 악기 믹서

### 7.1 UI 구성
- 각 악기마다 **볼륨 슬라이더** + **뮤트 버튼** + **음색 선택**
- 전체 믹스 리셋 버튼
- Embellishment 토글 (코드 장식음 on/off)

### 7.2 악기별 제어 항목
| 악기 | 볼륨 | 뮤트 | 음색 선택 |
|------|------|------|---------|
| 하모니 | ✅ | ✅ | Piano / Rhodes / Guitar / Vibraphone / Organ |
| 베이스 | ✅ | ✅ | Acoustic / Electric |
| 드럼 | ✅ | ✅ | 고정 (스타일에 종속) |

### 7.3 뮤트 활용 시나리오
- 피아노 뮤트 → 피아니스트가 자기 반주 연습
- 베이스 뮤트 → 베이시스트가 베이스라인 연습
- 드럼 뮤트 → 클리닉/레슨 환경에서 조용한 연습
- 전체 뮤트 후 하나씩 추가 → 편곡 분석 용도

---

## 8. [F] Chord Diagrams — 운지 다이어그램

### 8.1 지원 악기
- **기타**: 6현, 표준 튜닝 기준 (다수 포지션 제공)
- **피아노**: 양손 또는 오른손 단독 표기
- **우쿨렐레**: 4현 표준 튜닝

### 8.2 동작 방식
- 재생 중 현재 코드에 맞는 다이어그램 자동 업데이트
- 다음 코드 예고 다이어그램도 표시 (선택적)
- **정지/일시정지 상태에서 스와이프** → 같은 코드의 다른 보이싱/포지션으로 전환
- 선택한 보이싱은 곡 단위로 기억됨 (재생 재개 후에도 유지)

### 8.3 보이싱 데이터베이스
- 코드명 → 포지션 배열 매핑 (내부 데이터베이스)
- 예: D7의 경우 nut 포지션, 5프렛 바레, 10프렛 바레, 12프렛 바레 등
- **한계**: 사용자 정의 보이싱 추가 불가 (오랜 기간 요청됐으나 미구현)
  → 오디오 생성 엔진과 보이싱 데이터가 연동되어 있어 커스텀 구현이 기술적으로 복잡

### 8.4 코드 스케일 뷰
- 현재 코드에 어울리는 스케일(모드) 표시
- Guitar/Piano 지판 위에 스케일 음 표시 (즉흥 연주 학습용)

### 8.5 우리 프로젝트와의 차이점 및 개선 방향
현재 프로젝트는 지판 전체에 운지 위치를 점등하는 방식. iReal Pro는 소형 다이어그램을 별도로 표시.

```csharp
// 다중 보이싱 지원을 위한 ChordData 확장 방안
[Serializable]
public class ChordData : ScriptableObject {
    public string chordName;
    public List<FretPositionSet> voicings; // 여러 보이싱
    public int defaultVoicingIndex;

    public FretPositionSet GetVoicing(int index) =>
        voicings[Mathf.Clamp(index, 0, voicings.Count - 1)];
}

[Serializable]
public class FretPositionSet {
    public string voicingName;       // "Open", "5th Fret Barre", etc.
    public FretPosition[] positions;
    public int baseFret;             // 시작 프렛 (바레코드용)
}
```

---

## 9. [G] Practice Mode — 자동 연습 보조

### 9.1 자동 BPM 증가 (Tempo Ramp)
- **매 루프(반복)마다** 고정 BPM 자동 증가
- 설정 범위: +1 ~ +20 BPM / 반복
- 사용 시나리오: 느린 템포로 시작 → 목표 템포까지 점진적으로 올리기
- 재생 중 현재 BPM 실시간 표시 (화면 상단 우측)

### 9.2 자동 조옮김 (Key Transposition)
- **매 루프마다** 지정 반음 수만큼 키 자동 이동
- 추천 설정: +5 (완전 4도) 또는 +7 (완전 5도) → 12번 반복으로 모든 키 커버
- 또는 +1 (반음) → 12번으로 12키 순환
- 사용 시나리오: 12개 조성에서 동일 곡 연습 (즉흥 연주자의 근육기억 의존 방지)

### 9.3 두 옵션 동시 사용
- Tempo Ramp + Key Transposition 동시 활성화 가능
- 결과: 매 반복마다 키도 바뀌고 템포도 빨라짐 → 고난도 연습 세션

### 9.4 카운트인 (Count-In)
- 재생 전 1마디 준비 클릭
- 옵션: 클릭만 / 클릭 + 키 루트 음(사인파) 재생
- 리피트 시에는 카운트인 생략

### 9.5 표시 정보
- 재생 중 화면 상단: **현재 키 | 현재 BPM** 실시간 표시
- 반복 카운터 표시 (몇 번째 루프인지)

### 9.6 구현 예시 (C#)
```csharp
// PracticeSessionController 확장
public class PracticeConfig {
    public bool tempoRampEnabled;
    public int tempoRampBpmPerLoop;   // 1~20
    public bool autoTransposeEnabled;
    public int transposeSemitonesPerLoop; // 1~12
    public bool countInEnabled;
    public int countInBeats;          // 보통 4
}

// 루프 완료 시 호출
void OnLoopComplete(int loopIndex) {
    if (config.tempoRampEnabled)
        timingEngine.SetBPM(timingEngine.BPM + config.tempoRampBpmPerLoop);

    if (config.autoTransposeEnabled)
        TransposeSession(config.transposeSemitonesPerLoop);
}
```

---

## 10. 데이터 아키텍처 전체 흐름

```
[Song Library]
    │ SongData (제목, 키, BPM, 스타일, 코드진행)
    ▼
[Chart Parser]
    │ 코드 문자열 → ChordData[] + SectionMarker[]
    ▼
[Practice Session Controller]
    │ 루프 인덱스, 현재 키, 현재 BPM 관리
    │ 비트/마디/루프 이벤트 발행
    ▼
    ├──► [TimingEngine]         DSP 타이밍 → OnBeat / OnMeasure / OnLoop
    ├──► [FretboardRenderer]    코드 → 지판 운지 시각화
    ├──► [ChordDiagramPanel]    코드 → 소형 다이어그램 표시
    ├──► [ChordProgressionPanel] 진행 슬롯 하이라이트
    ├──► [UIManager]            키/BPM/박자 HUD 업데이트
    └──► [AudioEngine]          스타일 → 반주 오디오 생성 (Phase 4)
```

---

## 11. 현재 프로젝트 갭 분석 및 우선순위

| 기능 카테고리 | iReal Pro | 현재 구현 | 갭 | 우선순위 |
|---|---|---|---|---|
| **코드 진행 입력** | 에디터 (풀 악보) | 슬롯 클릭 1~8개 | 곡 단위 저장 없음 | ⭐⭐⭐ Phase 2 |
| **곡/진행 저장소** | 1,400+ 내장 곡 | 없음 | 표준 곡 데이터 없음 | ⭐⭐⭐ Phase 2 |
| **카운트인** | 있음 | 없음 | — | ⭐⭐ Phase 2 |
| **자동 BPM 증가** | 매 루프 +1~20 | 없음 | — | ⭐⭐ Phase 2 |
| **조옮김** | ±반음 버튼 | 없음 | 코드 포지션 반음 이동 필요 | ⭐⭐ Phase 2 |
| **BPM 조절** | 슬라이더 | ✅ 있음 | 완료 | — |
| **루프 재생** | N회 / 무한 | ✅ 무한 루프 | 횟수 설정 없음 | ⭐ Phase 2 |
| **다중 보이싱** | 스와이프 전환 | 단일 고정 포지션 | 보이싱 데이터 없음 | ⭐⭐ Phase 3 |
| **자동 12키 전조** | 매 루프 전조 | 없음 | — | ⭐⭐ Phase 3 |
| **코드 스케일 뷰** | 있음 | 없음 | — | ⭐ Phase 3 |
| **반주 스타일** | 51개 스타일 | 없음 | 대규모 오디오 작업 | ⭐ Phase 4 |
| **실제 오디오 반주** | 드럼/베이스/피아노 | 없음 | 오디오 엔진 필요 | ⭐ Phase 4 |
| **악기 믹서** | 있음 | 없음 | 반주 구현 후 | Phase 4+ |
| **커뮤니티 공유** | 포럼 | 없음 | 서버 인프라 필요 | Phase 5+ |

---

## 12. 단계별 구현 로드맵

### Phase 2 — 연습 기능 강화 (단기, 즉시 착수 가능)

#### 2-1. SongDatabase 시스템
- `SongData` ScriptableObject 정의 (제목, 키, BPM, 장르, 코드 진행)
- 표준 재즈 곡 10~20개 내장: Autumn Leaves, Blue Bossa, Fly Me to the Moon 등
- `SongDatabase` ScriptableObject에 목록 관리
- UI: 곡 선택 드롭다운 또는 리스트 패널

#### 2-2. 카운트인
- Play 버튼 → 1마디 클릭 후 반주 시작
- TimingEngine에 `CountInMeasures` 파라미터 추가
- 카운트인 중 지판 미표시

#### 2-3. 자동 BPM 증가
- `PracticeConfig.tempoRampBpm` 설정값 추가
- 루프 완료 이벤트에서 BPM 증가 처리
- HUD에 현재 BPM 실시간 표시

#### 2-4. 반음 조옮김
- `+1` / `-1` 버튼 UI 추가
- ChordDatabase에서 모든 코드를 N반음 이동하는 유틸 메서드
- FretPosition의 fretIndex를 일괄 오프셋 처리

#### 2-5. 루프 횟수 설정
- "∞ / 1 / 2 / 4 / 8회" 선택 버튼 추가

---

### Phase 3 — 코드 시스템 확장 (중기)

#### 3-1. 다중 보이싱 지원
- `ChordData`에 `List<FretPositionSet> voicings` 추가
- 각 코드에 2~5개 포지션 데이터 등록
- UI: 지판 상단 점(dot) 스와이프 또는 버튼으로 보이싱 전환
- 선택 보이싱 세션 단위 저장

#### 3-2. 자동 12키 전조 연습 모드
- `PracticeConfig.autoTransposeSemitones` 설정
- 루프 완료 시 전조 처리
- 예: +5씩 12회 = 완전4도 전조로 12키 커버

#### 3-3. 코드 스케일 뷰
- 코드명 → 추천 스케일 매핑 (C7 → Mixolydian, A-7 → Dorian 등)
- 지판에 스케일 음 반투명 오버레이 표시
- 토글 버튼으로 운지 뷰 ↔ 스케일 뷰 전환

#### 3-4. 섹션 마커 (A/B/Verse/Intro)
- `SongData`에 섹션 정보 추가
- 코드 진행 패널에 섹션 레이블 표시
- 특정 섹션만 반복 연습하는 기능

---

### Phase 4 — 오디오 반주 엔진 (장기, 대규모 작업)

#### 옵션 A: Unity Audio + 루프 샘플
- 스타일별 드럼/베이스 오디오 루프 파일 (WAV/OGG)
- Unity AudioSource로 BPM에 맞춰 루프 재생
- 코드 전환 시 하모니 악기 MIDI 또는 샘플 트리거
- 장점: 오프라인 동작, 지연 없음
- 단점: 오디오 파일 용량 큼, 유기적 변화 구현 어려움

#### 옵션 B: Web Audio API (WebGL 빌드)
- FastAPI → Tone.js 연동으로 WebGL에서 신디사이저 반주
- 코드명을 받아 브라우저에서 실시간 합성
- 장점: 파일 용량 없음, 다양한 음색 가능
- 단점: 지연 가능성, WebGL-Unity 통신 필요

#### 옵션 C: 외부 MIDI 출력 (고급)
- Unity → MIDI 아웃 → DAW 또는 하드웨어 신디
- Phase 5 하드웨어 연동 시나리오와 결합 가능

---

### Phase 5 — 공유 및 커뮤니티 (장기)

- irealbook:// URL 포맷 import/export 지원
- 코드 진행 QR 코드 공유
- 서버 사이드 곡 라이브러리 API (MongoDB 활용, 이미 docker-compose에 포함)
- 사용자 업로드 코드 진행 포럼

---

## 13. 우리 프로젝트의 차별화 전략

iReal Pro는 **반주(오디오) + 코드 진행**에 특화.
우리 프로젝트는 **운지법 시각화 + 신체 학습**에 특화.

```
iReal Pro    : 코드 진행 → 가상 밴드 반주 → 즉흥 연주 / 청음 연습
                타깃: 중급 이상, 코드 진행 이미 아는 연주자

이 프로젝트  : 코드 진행 → 지판 운지 점등 → 손가락 위치 학습
                타깃: 초보자, 코드 운지법을 모르는 입문자
                강점: 실제 손 동작 학습, 운지 예고(Preview), 타이밍 연습
```

**확장 시 고유 포지셔닝:**
- iReal Pro + 운지 시각화 = 이 프로젝트의 목표
- 향후 마이크 코드 인식(Phase 4) 추가 시: "연주하면 틀린 운지 교정"까지 가능
- 하드웨어 LED(Phase 5) 추가 시: 실물 기타 프렛 위에 LED로 운지 안내

---

## 14. 구현 언어 & 기술 스택

iReal Pro는 소스코드를 공개하지 않지만, 버전 히스토리·빌드 정보·개발자 인터뷰로 추론 가능한 내용이다.

### 14.1 개발 배경

- 개발자: **Massimo Biolcati** — 뉴욕 기반 재즈 베이시스트
- 2010년 초대 iPhone 출시 직후, 본인의 연습 도구로 제작 (독학으로 Objective-C 습득)
- 이후 소수 팀(Technimo LLC)으로 iOS / Android / macOS 3개 플랫폼 동시 유지
- 소규모 팀이 3개 플랫폼을 관리한다는 사실에서 **C++ 공유 코어** 구조가 유력하게 추정됨

### 14.2 플랫폼별 기술 스택

#### iOS / macOS
| 계층 | 기술 | 근거 |
|------|------|------|
| UI (구) | **Objective-C** | 2010년 첫 출시, 개발자 인터뷰 언급 |
| UI (신) | **SwiftUI** | 2021.8 버전: "Song View Settings rewritten in SwiftUI" 공식 명시 |
| 오디오 엔진 | **CoreAudio + AudioUnit** | iOS 네이티브 실시간 오디오의 사실상 표준 |
| 내부 DSP/합성 | **C++ (추정)** | 크로스플랫폼 공유, 실시간 오디오 처리에 적합 |
| 빌드 도구 | **Xcode 15** | 2023.11 릴리즈 노트 명시 |
| 최소 요구 사양 | iOS 16+, macOS 12+ | 2024.10 기준 드롭 |

#### Android
| 계층 | 기술 |
|------|------|
| UI | Java / Kotlin |
| 오디오 | OpenSL ES 또는 AAudio (Android 네이티브 오디오 API) |
| 공유 코어 | C++ JNI 브릿지 경유로 iOS 코어 재사용 가능성 높음 |

### 14.3 오디오 엔진 내부 추정 구조

공개된 정보는 아니지만, CoreAudio 기반 + 실제 드럼 샘플(2025.9 추가) 기반으로 유추하면:

```
코드명 입력 (예: "G7alt")
        │
        ▼
보이싱 룰 엔진 (C++ 추정)
        │  코드 타입 → 인터벌 배열 → MIDI 노트 집합 생성
        │  예: G7alt → [G, B, Db, F, Ab, Eb]
        ▼
스타일 패턴 엔진 (C++ 추정)
        │  리듬 그리드(스윙 셔플 비율 등) × 보이싱 → 이벤트 타임라인
        │  Organic Variation: 같은 코드라도 매 반복마다 미세하게 다른 타이밍/보이싱
        ▼
오디오 합성 레이어
        ├── 피아노/기타:  SoundFont(.sf2) 기반 샘플러 또는 내장 신디사이저
        ├── 베이스:       음역별 베이스 샘플 (어쿠스틱/일렉트릭)
        └── 드럼:         2025.9부터 Jazz 스타일에 실제 녹음 샘플(WAV) 적용
        ▼
CoreAudio AudioUnit (iOS) / OpenSL ES or AAudio (Android)
        │  DSP 버퍼 레벨 믹싱 → 최종 오디오 출력
        ▼
스피커 / 헤드폰 / AirPlay
```

### 14.4 데이터 직렬화
- **irealbook://** URL 포맷: 평문 텍스트, 파싱 라이브러리 다수 존재
- **irealb://** 포맷 (신): 난독화 처리, 비공개
- 앱 내부 데이터베이스: SQLite 추정 (iOS 앱의 표준 로컬 DB)
- 곡 공유: URL 스킴 → 클립보드 / AirDrop / 포럼 붙여넣기

### 14.5 버전 진화 타임라인 (주요 기술 변화)

| 연도 | 버전 | 주요 기술 변화 |
|------|------|--------------|
| 2010 | 1.0 | iOS 최초 출시, Objective-C |
| 2011 | 3.0 | iReal Book → iReal b 리브랜딩 |
| 2013 | — | iReal Pro 브랜딩, Android 버전 출시 |
| 2020 | 2020.9 | Universal App 지원 (iPhone + iPad 통합) |
| 2021 | 2021.8 | Song View Settings SwiftUI로 재작성 |
| 2023 | 2023.11 | Xcode 15 빌드, iOS 17 호환, Dark Mode |
| 2024 | 2024.1 | 피아노 보이싱 개선, 신규 코드 퀄리티 추가 |
| 2024 | 2024.10 | iOS 16+ / macOS 12+ 요구 사항 상향 |
| 2025 | 2025.2 | 카운트인 키 루트 음 재생 옵션 추가 |
| 2025 | 2025.7 | Half Time 재생 기능 |
| 2025 | 2025.9 | **Real Drums**: Jazz 스타일에 실제 드럼 녹음 샘플 적용 |
| 2025 | 2025.10 | 재생 시작 속도 개선 |

---

## 15. 전체 워크플로우

### 15.1 사용자 시나리오별 흐름

```
┌─────────────────────────────────────────────────────────────┐
│                    iReal Pro 전체 워크플로우                  │
└─────────────────────────────────────────────────────────────┘

① 곡 확보 (3가지 경로)
   │
   ├─A─ 내장 라이브러리
   │     └── 1,400+ 곡, 장르별 플레이리스트에서 선택
   │
   ├─B─ 포럼 다운로드
   │     └── forums.irealpro.com → 장르 탭 → 곡 선택
   │          → irealbook:// URL → 앱에서 자동 임포트
   │
   └─C─ 직접 작성 (Chart Editor)
         └── 새 곡 생성 → 편집기에서 코드 입력 → 저장

              ↓

② 플레이리스트 관리
   ├── 플레이리스트 생성 / 곡 추가 / 순서 변경
   ├── 플레이리스트 내보내기: iReal 포맷(URL) 또는 PDF 북릿
   └── 포럼에 공유 업로드

              ↓

③ 악보 에디터 (선택적 수정)
   ├── 코드 입력: 16셀 × 12줄 그리드
   ├── 구조 기호: 마디선, 반복({  }), 섹션(*A *B), 세뇨(S), 코다(Q)
   ├── 박자표 지정: T44 / T34 / T68 등 (마디 중간에도 변경 가능)
   ├── 대체 코드: (소괄호 안에 상단 소형 표시)
   └── 저장 → irealbook:// URL로 직렬화

              ↓

④ 플레이어 세팅 (재생 전 설정)
   ├── 스타일 선택 (51개 기본 + 추가 팩)
   │    └── 각 스타일: 드럼 패턴 + 베이스 패턴 + 보이싱 규칙 세트
   ├── BPM 설정 (슬라이더, 대략 30~400)
   ├── 키 조옮김 (±반음 버튼 또는 키 드롭다운)
   ├── 루프 횟수 (1회 / N회 / 무한)
   └── 카운트인 on/off (+ 루트 음 재생 옵션)

              ↓

⑤ 믹서 세팅 (선택)
   ├── 악기별 볼륨 슬라이더
   ├── 악기별 뮤트 버튼 (내 파트 연습 시)
   ├── 음색 변경 (Piano → Rhodes / Acoustic Guitar 등)
   └── Embellishment 토글 (코드 보이싱 장식음)

              ↓

⑥ Practice Mode 세팅 (선택)
   ├── 자동 BPM 증가: +1~20 BPM / 루프 (느린 템포 → 목표 템포 점진적 도달)
   └── 자동 전조: +N 반음 / 루프 (12키 순환 연습)
        └── 두 옵션 동시 활성화 가능 (고난도 연습)

              ↓

⑦ 재생 중 실시간 처리
   │
   ├── [오디오 엔진 레이어]
   │    ├── 코드명 → 보이싱 룰 → MIDI 노트 배열 생성
   │    ├── 스타일 패턴 × 보이싱 → 이벤트 타임라인
   │    ├── Organic Variation: 매 반복마다 미세하게 다른 보이싱/타이밍
   │    └── CoreAudio DSP 버퍼로 최종 합성 출력
   │
   ├── [악보 뷰 레이어]
   │    ├── 현재 마디 하이라이트 (자동 스크롤)
   │    ├── 다음 코드 예고 표시
   │    └── 섹션 레이블 (A / B / Verse / Intro) 표시
   │
   └── [운지 다이어그램 레이어]
        ├── 현재 코드 → 기타/피아노/우쿨렐레 운지 자동 업데이트
        ├── 정지 중 스와이프 → 같은 코드의 다른 보이싱/포지션 전환
        │    └── 선택한 보이싱: 곡 단위로 기억 유지
        └── 코드 스케일 뷰 토글 (즉흥 연주용 스케일 음 표시)

              ↓

⑧ 루프 완료 이벤트 처리
   ├── BPM += N (Tempo Ramp 활성 시)
   ├── Key += N 반음 (자동 전조 활성 시)
   └── HUD 갱신: 현재 키 | 현재 BPM 실시간 표시

              ↓

⑨ 종료 / 내보내기 (선택)
   ├── Stop → 재생 중지
   ├── 반주 + 자기 연주 녹음 → .wav / AAC 내보내기
   └── 완성된 악보 PDF 인쇄 / 공유
```

### 15.2 우리 프로젝트와 단계별 대응 맵

| iReal Pro 워크플로우 단계 | 우리 프로젝트 현재 상태 | 구현 필요 단계 |
|---|---|---|
| ① 곡 확보 (라이브러리) | ChordSelector 슬롯 수동 입력 | Phase 2: SongDatabase |
| ① 곡 확보 (포럼 다운로드) | 없음 | Phase 5+ |
| ① 곡 확보 (편집기 작성) | 없음 | Phase 2: 기본 입력 |
| ② 플레이리스트 관리 | 없음 | Phase 2 |
| ③ 악보 에디터 | 없음 (슬롯 클릭 방식) | Phase 3 |
| ④ 스타일 선택 | 없음 | Phase 4 |
| ④ BPM 설정 | ✅ 구현됨 | — |
| ④ 키 조옮김 | 없음 | Phase 2 |
| ④ 루프 설정 | ✅ 무한 루프 | Phase 2: 횟수 설정 |
| ④ 카운트인 | 없음 | Phase 2 |
| ⑤ 믹서 | 없음 | Phase 4 이후 |
| ⑥ 자동 BPM 증가 | 없음 | Phase 2 |
| ⑥ 자동 전조 | 없음 | Phase 2~3 |
| ⑦ 오디오 반주 엔진 | 없음 | Phase 4 |
| ⑦ 악보 뷰 (코드 하이라이트) | ✅ 구현됨 | — |
| ⑦ 운지 다이어그램 | ✅ 지판 점등 방식 | Phase 3: 다중 보이싱 |
| ⑧ 루프 완료 이벤트 | ✅ OnLoopComplete 있음 | Phase 2: Practice 로직 연결 |
| ⑨ 녹음/내보내기 | 없음 | Phase 5+ |

---

## 16. 참고 링크

- [iReal Pro 공식 사이트](https://www.irealpro.com/)
- [iReal Pro Custom Chord Chart Protocol](https://www.irealpro.com/ireal-pro-custom-chord-chart-protocol)
- [iReal Pro Developer Docs](https://www.irealpro.com/developer-docs)
- [Chord Symbols 전체 목록](https://technimo.helpshift.com/hc/en/3-ireal-pro/faq/88-chord-symbols-used-in-ireal-pro/)
- [Practice Mode 상세](https://technimo.helpshift.com/hc/en/3-ireal-pro/faq/36-practice-mode/)
- [Chord Diagrams 상세](https://technimo.helpshift.com/hc/en/3-ireal-pro/faq/77-chord-diagrams-for-guitar-piano-ukulele-and-chord-scales/)
- [Accompaniment Styles 목록](https://technimo.helpshift.com/hc/en/3-ireal-pro/faq/312-accompaniment-styles/)
- [Version History](https://www.irealpro.com/version-history)
- [iRealPro 파서 라이브러리 (Perl)](https://github.com/sciurius/perl-Data-iRealPro)
- [iRealb Parser (JS)](https://github.com/rubiety/irealb_parser)
- [고급 연습 기법 블로그](https://www.irealpro.com/news/advanced-practicing-techniques)
- [커뮤니티 포럼](https://forums.irealpro.com/)
