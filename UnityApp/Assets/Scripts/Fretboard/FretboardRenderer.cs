// FretboardRenderer.cs - 기타 지판 렌더링 + 코드 전환 애니메이션 제어
using UnityEngine;

public class FretboardRenderer : MonoBehaviour
{
    [SerializeField] private FretboardConfig config;
    [SerializeField] private GameObject fretMarkerPrefab;

    // 2D 배열: [줄][프렛] -> FretMarker
    private FretMarker[,] markers;

    // 현재 활성 코드 (TransitionToChord에서 사용)
    private ChordData currentChord;

    void Awake()
    {
        Debug.LogWarning("FretboardRenderer Awake() - 지판 생성 시작");
        BuildFretboard();
        Debug.LogWarning("FretboardRenderer 지판 생성 완료!");
    }

    // ── 지판 빌드 ─────────────────────────────────────────────────────────
    private void BuildFretboard()
    {
        markers = new FretMarker[config.numberOfStrings, config.numberOfFrets + 1];

        for (int s = 0; s < config.numberOfStrings; s++)
        {
            float y = s * config.stringSpacing;
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

    // ── 공개 API ──────────────────────────────────────────────────────────

    /// <summary>
    /// 코드 하이라이트 (즉시). 세션 시작 첫 코드에 사용.
    /// </summary>
    public void HighlightChord(ChordData chord)
    {
        ClearAllHighlights();

        foreach (var pos in chord.positions)
        {
            if (IsValidPosition(pos) && !pos.isMuted)
                markers[pos.stringIndex, pos.fretIndex].SetActive(true);
        }

        currentChord = chord;
    }

    /// <summary>
    /// 모든 하이라이트 즉시 해제.
    /// </summary>
    public void ClearAllHighlights()
    {
        if (markers == null)
        {
            Debug.LogWarning("ClearAllHighlights 호출됐지만 markers가 아직 초기화되지 않음");
            return;
        }

        for (int s = 0; s < config.numberOfStrings; s++)
            for (int f = 0; f <= config.numberOfFrets; f++)
                markers[s, f].SetActive(false);

        currentChord = null;
    }

    /// <summary>
    /// 다음 코드 예고 표시. 마지막 비트(beat 3)에 호출.
    /// Active 상태 마커는 건드리지 않는다.
    /// </summary>
    public void PreviewNextChord(ChordData nextChord)
    {
        if (nextChord == null) return;

        foreach (var pos in nextChord.positions)
        {
            if (!IsValidPosition(pos) || pos.isMuted) continue;

            // Active(겹치는 위치) 포함 전부 SetPreview — 노란색도 즉시 dim cyan으로 전환
            markers[pos.stringIndex, pos.fretIndex].SetPreview();
        }
    }

    /// <summary>
    /// 크로스페이드로 다음 코드로 전환. OnMeasureStart에서 호출.
    /// </summary>
    public void TransitionToChord(ChordData nextChord, float duration)
    {
        if (nextChord == null) return;

        float fadeDuration = Mathf.Max(duration, 0.05f);

        // 1. 남아있는 Preview 마커 → Inactive (stale 예고 클리어)
        for (int s = 0; s < config.numberOfStrings; s++)
        {
            for (int f = 0; f <= config.numberOfFrets; f++)
            {
                FretMarker m = markers[s, f];
                if (m.CurrentState == FretMarker.State.Preview)
                    m.CrossfadeToInactive(fadeDuration * 0.3f);
            }
        }

        // 2. 현재 코드 마커 처리
        //    - 겹치는 위치: CrossfadeToInactive 호출 없이 바로 CrossfadeToActive
        //      (yellow → transparentCyan 즉시 전환 → 새 코드와 동시에 시안 점등)
        //    - 겹치지 않는 위치: 천천히 fade out
        if (currentChord != null)
        {
            foreach (var pos in currentChord.positions)
            {
                if (!IsValidPosition(pos) || pos.isMuted) continue;

                FretMarker m = markers[pos.stringIndex, pos.fretIndex];
                if (!IsPositionInChord(pos, nextChord))
                    m.CrossfadeToInactive(fadeDuration * 1.8f);
                // 겹치는 위치는 아래 step 3에서 CrossfadeToActive만 호출됨
            }
        }

        // 3. 다음 코드 마커 → Active (겹치는 위치 포함 전부 시안 점등)
        foreach (var pos in nextChord.positions)
        {
            if (!IsValidPosition(pos) || pos.isMuted) continue;
            markers[pos.stringIndex, pos.fretIndex].CrossfadeToActive(fadeDuration);
        }

        // 4. currentChord 갱신
        currentChord = nextChord;
    }

    // ── 내부 헬퍼 ─────────────────────────────────────────────────────────

    private bool IsValidPosition(FretPosition pos)
    {
        return pos.stringIndex >= 0
            && pos.stringIndex < config.numberOfStrings
            && pos.fretIndex >= 0
            && pos.fretIndex <= config.numberOfFrets;
    }

    private bool IsPositionInChord(FretPosition pos, ChordData chord)
    {
        if (chord == null) return false;
        foreach (var p in chord.positions)
        {
            if (!p.isMuted && p.stringIndex == pos.stringIndex && p.fretIndex == pos.fretIndex)
                return true;
        }
        return false;
    }
}
