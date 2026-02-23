using System;

[Serializable]
public struct FretPosition
{
    public int stringIndex;  // 0=6번줄(low E), 5=1번줄(high E)
    public int fretIndex;    // 0=개방현, 1~20=프렛
    public int fingerNumber; // 0=개방현, 1=검지, 2=중지, 3=약지, 4=소지
    public bool isMuted;     // true면 안 치는 줄

    public FretPosition(int s, int f, int finger = 0, bool muted = false)
    {
        stringIndex = s;
        fretIndex = f;
        fingerNumber = finger;
        isMuted = muted;
    }
}
