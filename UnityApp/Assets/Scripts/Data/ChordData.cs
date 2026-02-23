using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChord", menuName = "Guitar/Chord Data")]
public class ChordData : ScriptableObject
{
    public string chordName;       // "C", "Am", "G" 등
    public string chordFullName;   // "C Major", "A Minor" 등
    public List<FretPosition> positions; // 6개 (줄당 하나)
    public List<int> fingerNumbers;     // 운지 번호 (1=검지, 2=중지, 3=약지, 4=소지)
}
