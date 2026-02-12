using UnityEngine;

[CreateAssetMenu(fileName = "FretboardConfig", menuName = "Guitar/Fretboard Config")]
public class FretboardConfig : ScriptableObject
{
    public int numberOfStrings = 6;
    public int numberOfFrets = 15;
    public float stringSpacing = 0.3f;
    public float fretSpacing = 0.5f;
    public float markerSize = 0.2f;

    // 색상
    public Color fretboardColor = new Color(0.15f, 0.12f, 0.08f);
    public Color stringColor = new Color(0.8f, 0.8f, 0.7f);
    public Color fretWireColor = new Color(0.6f, 0.6f, 0.6f);
    public Color inactiveMarkerColor = new Color(0.2f, 0.2f, 0.3f, 0.3f);
    public Color activeMarkerColor = new Color(0.4f, 0.4f, 1.0f, 1.0f);
    public Color glowColor = new Color(0.5f, 0.5f, 1.0f, 0.8f);
}
