## Unity 변경 요청 — B-4-a SongData/SongDatabase 신규 데이터 모델

> 이 문서는 **spec 문서**입니다. Unity 실제 코드 반영 전, 대상 프로젝트의 기존 파일/네임스페이스/asmdef 구조를 먼저 확인한 뒤 적용하세요.

### 목표
- Song Library용 ScriptableObject 데이터 모델을 신규 추가
- 기존 `ChordData` / `ChordDatabase`와 **독립된 데이터 축**으로 유지
- FastAPI Song 스키마(`api/main.py`) 및 irealpro.md §3 구조와 필드 정합 유지

---

## 1) 신규 파일: `Assets/Scripts/Data/SectionMarker.cs`

```csharp
using System;

[Serializable]
public class SectionMarker
{
    // iRealPro 섹션 표기: *A, *B, *V, *i
    public string label;

    // chordProgression 내 시작 인덱스 (0-based)
    public int startIndex;
}
```

요구사항:
- `label` (string: `*A/*B/*V/*i`)
- `startIndex` (int)

---

## 2) 신규 파일: `Assets/Scripts/Data/SongData.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSong", menuName = "Guitar/Song Data")]
public class SongData : ScriptableObject
{
    [Header("Basic Info")]
    public string title;
    public string composer;
    public string style;
    public string key;
    public int bpm;
    public string timeSignature;
    public string genre;
    public string[] tags;

    [Header("Progression")]
    // 코드명 문자열 리스트 (예: "Cm7", "F7", "Bbmaj7")
    public List<string> chordProgression = new List<string>();

    [Header("Structure")]
    public List<SectionMarker> sections = new List<SectionMarker>();
}
```

필수 public 필드 포함:
- `title`, `composer`, `style`, `key`, `bpm`, `timeSignature`, `genre`, `tags[]`
- `chordProgression: List<string>`
- `sections: List<SectionMarker>`

---

## 3) 신규 파일: `Assets/Scripts/Data/SongDatabase.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SongDatabase", menuName = "Guitar/Song Database")]
public class SongDatabase : ScriptableObject
{
    public List<SongData> songs = new List<SongData>();

    public SongData GetSong(string title)
    {
        return songs.Find(s => s != null && s.title == title);
    }

    public List<SongData> GetByGenre(string genre)
    {
        return songs.FindAll(s => s != null && s.genre == genre);
    }

    public List<SongData> GetAll()
    {
        return songs;
    }
}
```

필수 메서드 포함:
- `GetSong(string title)`
- `GetByGenre(string genre)`
- `GetAll()`

---

## 4) 기존 `ChordData`, `ChordDatabase`와의 관계

현재 구조:
- `ChordData`: 코드 1개 운지/포지션 데이터
- `ChordDatabase`: `List<ChordData>` 조회 (`GetChord`, `GetAllChordNames`)

신규 구조:
- `SongData`: 곡 메타데이터 + 코드 진행 + 섹션 마커
- `SongDatabase`: `List<SongData>` 조회

관계 원칙:
- 두 DB는 **독립 ScriptableObject**로 유지
- `SongData`는 코드명을 문자열로만 저장
- 실제 운지 조회가 필요할 때만 런타임에서 `ChordDatabase.GetChord(chordName)`으로 연결

---

## 5) Inspector 연결 방법

1. 스크립트 3개 추가 후 Unity 컴파일 완료 대기
2. Assets에서 생성
   - `Create > Guitar > Song Data`
   - `Create > Guitar > Song Database`
3. `SongDatabase` 에셋의 `songs` 리스트에 여러 `SongData` 에셋 드래그 앤 드롭
4. 씬 매니저 컴포넌트(예: SongSelector/PracticeSessionController)에 `SongDatabase` 참조 필드 추가 후 할당
5. 런타임 조회
   - 제목 검색: `GetSong(title)`
   - 장르 필터: `GetByGenre(genre)`
   - 전체 목록: `GetAll()`

---

## 6) 스키마 정합성 (api/main.py / irealpro.md §3)

FastAPI Song 스키마 매핑:
- `title`, `composer`, `style`, `key`, `bpm`, `timeSignature`, `genre`, `tags`
- `chordProgression: List[str]`
- `sections: List[SectionMarker(label,startIndex)]`

irealpro.md §3 SongLibrary 반영:
- 곡 메타 + 장르/태그 축
- 코드 진행 + 섹션 마커(`*A/*B/*V/*i`)
- 향후 Playlist 확장 가능한 독립 SongDatabase 구조

---

## 7) 컴파일/런타임 위험 요소

1. **타입명 충돌**
   - 프로젝트 내 기존 `SongData`/`SectionMarker` 존재 시 컴파일 충돌 가능
2. **직렬화 누락**
   - `SectionMarker`에 `[Serializable]` 누락 시 Inspector에서 필드 미표시
3. **NullReference 위험**
   - `songs`, `chordProgression`, `sections` 미초기화 시 런타임 에러 가능
4. **문자열 매칭 민감도**
   - `GetSong`, `GetByGenre`는 현재 완전일치 비교(대소문자/공백 차이 취약)
5. **코드명 표준 불일치**
   - `SongData.chordProgression` 문자열과 `ChordData.chordName` 불일치 시 운지 조회 실패
