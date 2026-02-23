using System;
using System.Collections.Generic;

[Serializable]
public class PracticeSession
{
    public List<TimelineEvent> timeline;
    public int bpm;
    public bool isLooping;
}

[Serializable]
public class TimelineEvent
{
    public string chordName;       // ChordData 이름으로 참조
    public float durationInBeats;  // 보통 4.0 (한 마디)
}
