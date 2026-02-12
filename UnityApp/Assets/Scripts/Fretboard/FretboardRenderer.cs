using UnityEngine;

public class FretboardRenderer : MonoBehaviour
{
    [SerializeField] private FretboardConfig config;
    [SerializeField] private GameObject fretMarkerPrefab;

    // 2D 배열: [줄][프렛] -> FretMarker
    private FretMarker[,] markers;

    void Start()
    {
        BuildFretboard();
    }

    private void BuildFretboard()
    {
        markers = new FretMarker[config.numberOfStrings, config.numberOfFrets + 1];

        for (int s = 0; s < config.numberOfStrings; s++)
        {
            float y = s * config.stringSpacing;

            // 줄 시각화 (LineRenderer)
            DrawString(s, y);

            for (int f = 0; f <= config.numberOfFrets; f++)
            {
                float x = CalculateFretX(f);
                Vector3 pos = new Vector3(x, y, 0);

                GameObject go = Instantiate(fretMarkerPrefab, pos, Quaternion.identity, transform);
                FretMarker marker = go.GetComponent<FretMarker>();
                marker.Initialize(s, f, config);
                markers[s, f] = marker;
            }
        }

        // 프렛 와이어 그리기
        DrawFretWires();
    }

    // 프렛 간격은 올라갈수록 좁아짐 (12분의 1 법칙)
    private float CalculateFretX(int fretIndex)
    {
        if (fretIndex == 0) return -0.3f; // 개방현 위치 (너트 바깥)
        float scaleLength = config.numberOfFrets * config.fretSpacing;
        return scaleLength - (scaleLength / Mathf.Pow(2f, fretIndex / 12f));
    }

    private void DrawString(int stringIndex, float y)
    {
        GameObject stringObj = new GameObject($"String_{stringIndex}");
        stringObj.transform.SetParent(transform);

        LineRenderer lr = stringObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(-0.5f, y, 0));
        lr.SetPosition(1, new Vector3(CalculateFretX(config.numberOfFrets) + 0.2f, y, 0));

        // 줄 굵기 (6번줄이 가장 굵고, 1번줄이 가장 가늘다)
        float thickness = 0.02f + (5 - stringIndex) * 0.005f;
        lr.startWidth = thickness;
        lr.endWidth = thickness;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = config.stringColor;
        lr.endColor = config.stringColor;
    }

    private void DrawFretWires()
    {
        float totalHeight = (config.numberOfStrings - 1) * config.stringSpacing;

        for (int f = 1; f <= config.numberOfFrets; f++)
        {
            float x = CalculateFretX(f);

            GameObject fretObj = new GameObject($"Fret_{f}");
            fretObj.transform.SetParent(transform);

            LineRenderer lr = fretObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x, -0.1f, 0));
            lr.SetPosition(1, new Vector3(x, totalHeight + 0.1f, 0));
            lr.startWidth = 0.015f;
            lr.endWidth = 0.015f;

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = config.fretWireColor;
            lr.endColor = config.fretWireColor;
        }
    }

    /// <summary>
    /// 코드 하이라이트: 해당 코드의 포지션을 발광 표시
    /// </summary>
    public void HighlightChord(ChordData chord)
    {
        ClearAllHighlights();

        foreach (var pos in chord.positions)
        {
            if (!pos.isMuted && pos.fretIndex >= 0
                && pos.stringIndex >= 0 && pos.stringIndex < config.numberOfStrings
                && pos.fretIndex <= config.numberOfFrets)
            {
                markers[pos.stringIndex, pos.fretIndex].SetActive(true);
            }
        }
    }

    /// <summary>
    /// 모든 하이라이트 해제
    /// </summary>
    public void ClearAllHighlights()
    {
        for (int s = 0; s < config.numberOfStrings; s++)
            for (int f = 0; f <= config.numberOfFrets; f++)
                markers[s, f].SetActive(false);
    }
}
