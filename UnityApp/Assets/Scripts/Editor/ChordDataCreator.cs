using UnityEngine;
using UnityEditor;

/// <summary>
/// ChordDatabase에 기본 코드 데이터를 자동으로 추가하는 에디터 스크립트
/// </summary>
public class ChordDataCreator
{
    [MenuItem("Guitar/Setup Chord Database")]
    public static void SetupChordDatabase()
    {
        // ChordDatabase 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:ChordDatabase");
        if (guids.Length == 0)
        {
            Debug.LogError("ChordDatabase 에셋을 찾을 수 없습니다!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        ChordDatabase database = AssetDatabase.LoadAssetAtPath<ChordDatabase>(path);

        if (database == null)
        {
            Debug.LogError("ChordDatabase를 로드할 수 없습니다!");
            return;
        }

        // 기존 데이터 초기화
        database.chords = new System.Collections.Generic.List<ChordData>();

        // C 코드
        ChordData cChord = ScriptableObject.CreateInstance<ChordData>();
        cChord.chordName = "C";
        cChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = 3, fingerNumber = 3 },
            new FretPosition { stringIndex = 4, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 3, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 2, fretIndex = 1, fingerNumber = 1 },
            new FretPosition { stringIndex = 1, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 0, fretIndex = 0, fingerNumber = 0 }
        };
        AssetDatabase.AddObjectToAsset(cChord, database);
        database.chords.Add(cChord);

        // G 코드
        ChordData gChord = ScriptableObject.CreateInstance<ChordData>();
        gChord.chordName = "G";
        gChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = 3, fingerNumber = 2 },
            new FretPosition { stringIndex = 4, fretIndex = 2, fingerNumber = 1 },
            new FretPosition { stringIndex = 3, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 2, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 1, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 0, fretIndex = 3, fingerNumber = 3 }
        };
        AssetDatabase.AddObjectToAsset(gChord, database);
        database.chords.Add(gChord);

        // Am 코드
        ChordData amChord = ScriptableObject.CreateInstance<ChordData>();
        amChord.chordName = "Am";
        amChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 4, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 3, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 2, fretIndex = 2, fingerNumber = 3 },
            new FretPosition { stringIndex = 1, fretIndex = 1, fingerNumber = 1 },
            new FretPosition { stringIndex = 0, fretIndex = 0, fingerNumber = 0 }
        };
        AssetDatabase.AddObjectToAsset(amChord, database);
        database.chords.Add(amChord);

        // F 코드
        ChordData fChord = ScriptableObject.CreateInstance<ChordData>();
        fChord.chordName = "F";
        fChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = 1, fingerNumber = 1 },
            new FretPosition { stringIndex = 4, fretIndex = 3, fingerNumber = 3 },
            new FretPosition { stringIndex = 3, fretIndex = 3, fingerNumber = 4 },
            new FretPosition { stringIndex = 2, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 1, fretIndex = 1, fingerNumber = 1 },
            new FretPosition { stringIndex = 0, fretIndex = 1, fingerNumber = 1 }
        };
        AssetDatabase.AddObjectToAsset(fChord, database);
        database.chords.Add(fChord);

        // D 코드
        ChordData dChord = ScriptableObject.CreateInstance<ChordData>();
        dChord.chordName = "D";
        dChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 4, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 3, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 2, fretIndex = 2, fingerNumber = 1 },
            new FretPosition { stringIndex = 1, fretIndex = 3, fingerNumber = 3 },
            new FretPosition { stringIndex = 0, fretIndex = 2, fingerNumber = 2 }
        };
        AssetDatabase.AddObjectToAsset(dChord, database);
        database.chords.Add(dChord);

        // Em 코드 (022000)
        ChordData emChord = ScriptableObject.CreateInstance<ChordData>();
        emChord.chordName = "Em";
        emChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 4, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 3, fretIndex = 2, fingerNumber = 3 },
            new FretPosition { stringIndex = 2, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 1, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 0, fretIndex = 0, fingerNumber = 0 }
        };
        AssetDatabase.AddObjectToAsset(emChord, database);
        database.chords.Add(emChord);

        // A 코드 (x02220)
        ChordData aChord = ScriptableObject.CreateInstance<ChordData>();
        aChord.chordName = "A";
        aChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 4, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 3, fretIndex = 2, fingerNumber = 1 },
            new FretPosition { stringIndex = 2, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 1, fretIndex = 2, fingerNumber = 3 },
            new FretPosition { stringIndex = 0, fretIndex = 0, fingerNumber = 0 }
        };
        AssetDatabase.AddObjectToAsset(aChord, database);
        database.chords.Add(aChord);

        // E 코드 (022100)
        ChordData eChord = ScriptableObject.CreateInstance<ChordData>();
        eChord.chordName = "E";
        eChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 4, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 3, fretIndex = 2, fingerNumber = 3 },
            new FretPosition { stringIndex = 2, fretIndex = 1, fingerNumber = 1 },
            new FretPosition { stringIndex = 1, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 0, fretIndex = 0, fingerNumber = 0 }
        };
        AssetDatabase.AddObjectToAsset(eChord, database);
        database.chords.Add(eChord);

        // Dm 코드 (xx0231)
        ChordData dmChord = ScriptableObject.CreateInstance<ChordData>();
        dmChord.chordName = "Dm";
        dmChord.positions = new System.Collections.Generic.List<FretPosition>
        {
            new FretPosition { stringIndex = 5, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 4, fretIndex = -1, fingerNumber = 0, isMuted = true },
            new FretPosition { stringIndex = 3, fretIndex = 0, fingerNumber = 0 },
            new FretPosition { stringIndex = 2, fretIndex = 2, fingerNumber = 2 },
            new FretPosition { stringIndex = 1, fretIndex = 3, fingerNumber = 3 },
            new FretPosition { stringIndex = 0, fretIndex = 1, fingerNumber = 1 }
        };
        AssetDatabase.AddObjectToAsset(dmChord, database);
        database.chords.Add(dmChord);

        // 변경사항 저장
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ ChordDatabase 설정 완료! {database.chords.Count}개 코드 추가됨");
    }
}
