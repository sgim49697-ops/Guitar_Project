using UnityEngine;

public class FretMarker : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private FretboardConfig config;
    private bool isHighlighted;
    private float glowPhase;

    public void Initialize(int stringIdx, int fretIdx, FretboardConfig cfg)
    {
        config = cfg;
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();

        SetColor(config.inactiveMarkerColor);
        transform.localScale = Vector3.one * config.markerSize;
    }

    public void SetActive(bool active)
    {
        isHighlighted = active;
        if (active)
        {
            glowPhase = 0f;
            SetColor(config.activeMarkerColor);
        }
        else
        {
            SetColor(config.inactiveMarkerColor);
        }
    }

    void Update()
    {
        if (!isHighlighted) return;

        // 부드러운 펄스 발광 효과
        glowPhase += Time.deltaTime * 2f;
        float pulse = 0.8f + 0.2f * Mathf.Sin(glowPhase);
        Color c = Color.Lerp(config.activeMarkerColor, config.glowColor, pulse);
        SetColor(c);
    }

    private void SetColor(Color color)
    {
        if (meshRenderer != null && propBlock != null)
        {
            propBlock.SetColor("_Color", color);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}
