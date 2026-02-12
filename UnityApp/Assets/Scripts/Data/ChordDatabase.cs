using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChordDatabase", menuName = "Guitar/Chord Database")]
public class ChordDatabase : ScriptableObject
{
    public List<ChordData> chords;

    public ChordData GetChord(string name)
    {
        return chords.Find(c => c.chordName == name);
    }

    public List<string> GetAllChordNames()
    {
        List<string> names = new List<string>();
        foreach (var chord in chords)
        {
            names.Add(chord.chordName);
        }
        return names;
    }
}
