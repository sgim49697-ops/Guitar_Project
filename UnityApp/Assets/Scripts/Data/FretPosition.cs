using System;

[Serializable]
public struct FretPosition
{
    public int stringIndex;  // 0=6번줄(low E), 5=1번줄(high E)
    public int fretIndex;    // 0=개방현, 1~20=프렛
    public bool isMuted;     // true면 안 치는 줄

    public FretPosition(int s, int f, bool muted = false)
    {
        stringIndex = s;
        fretIndex = f;
        isMuted = muted;
    }
}
