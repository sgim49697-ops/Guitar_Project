using UnityEngine;

[CreateAssetMenu(fileName = "FretboardConfig", menuName = "Guitar/Fretboard Config")]
public class FretboardConfig : ScriptableObject
{
    public int numberOfStrings = 6;
    public int numberOfFrets = 15;
    public float stringSpacing = 0.3f;
    public float fretSpacing = 0.5f;
    public float markerSize = 0.7f;

    // 색상
    public Color fretboardColor = new Color(0.0f, 0.0f, 0.0f); // 검은색 배경
    public Color stringColor = new Color(0.8f, 0.8f, 0.7f); // 밝은 회색 줄
    public Color fretWireColor = new Color(0.6f, 0.6f, 0.6f); // 회색 프렛
    public Color inactiveMarkerColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);  // 거의 투명한 어두운 회색
    public Color activeMarkerColor   = new Color(0.0f,  1.0f,  1.0f,  1.0f); // 밝은 시안색 (청록색)
    public Color glowColor           = new Color(1.0f,  1.0f,  0.0f,  1.0f); // 밝은 노란색 (펄스 효과)
    public Color previewMarkerColor  = new Color(0.0f,  0.55f, 0.55f, 0.45f); // 반투명 시안 (다음 코드 예고)
}
