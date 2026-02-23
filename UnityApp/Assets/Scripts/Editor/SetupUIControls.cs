// SetupUIControls.cs - Canvas UI 컨트롤 자동 생성 및 참조 연결 Editor 유틸리티
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class SetupUIControls
{
    [MenuItem("Guitar/Setup UI Controls")]
    public static void CreateUIControls()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("❌ Canvas not found"); return; }

        PracticeSessionController sessionCtrl = Object.FindFirstObjectByType<PracticeSessionController>();
        TimingEngine timingEngine = Object.FindFirstObjectByType<TimingEngine>();
        ChordProgressionPanel progPanel = Object.FindFirstObjectByType<ChordProgressionPanel>();
        UIManager uiManager = canvasGO.GetComponent<UIManager>();

        // ── 1. ControlBar (하단 전체 폭, 120px) ──────────────────────────────────
        GameObject controlBar = MakePanel(canvasGO.transform, "ControlBar",
            amin: new Vector2(0, 0), amax: new Vector2(1, 0),
            omin: new Vector2(0, 0), omax: new Vector2(0, 120),
            color: new Color(0.05f, 0.05f, 0.1f, 0.92f));

        // BPMControl 영역 (왼쪽 45%)
        GameObject bpmGO = MakePanel(controlBar.transform, "BPMControl",
            amin: new Vector2(0, 0), amax: new Vector2(0.45f, 1),
            omin: new Vector2(8, 6), omax: new Vector2(-4, -6),
            color: Color.clear);

        Slider bpmSlider = MakeSlider(bpmGO.transform, "BPMSlider",
            amin: new Vector2(0, 0.08f), amax: new Vector2(1, 0.52f));
        TextMeshProUGUI bpmText = MakeTMP(bpmGO.transform, "BPMText",
            amin: new Vector2(0, 0.52f), amax: new Vector2(1, 1),
            text: "80 BPM", size: 22, bold: true);

        // TransportControls 영역 (오른쪽 45%)
        GameObject transportGO = MakePanel(controlBar.transform, "TransportControls",
            amin: new Vector2(0.55f, 0), amax: new Vector2(1, 1),
            omin: new Vector2(4, 6), omax: new Vector2(-8, -6),
            color: Color.clear);

        Button playBtn = MakeButton(transportGO.transform, "PlayButton", "Play",
            amin: new Vector2(0, 0.2f), amax: new Vector2(0.48f, 0.85f),
            color: new Color(0.15f, 0.55f, 0.15f));
        Button stopBtn = MakeButton(transportGO.transform, "StopButton", "Stop",
            amin: new Vector2(0.52f, 0.2f), amax: new Vector2(1, 0.85f),
            color: new Color(0.55f, 0.15f, 0.15f));

        // ── 2. HUD (우측 상단: 현재 코드명 + 비트 인디케이터) ─────────────────────
        GameObject hud = MakePanel(canvasGO.transform, "HUD",
            amin: new Vector2(0.72f, 0.82f), amax: new Vector2(1f, 1f),
            omin: Vector2.zero, omax: Vector2.zero,
            color: new Color(0.05f, 0.05f, 0.12f, 0.85f));

        TextMeshProUGUI chordNameText = MakeTMP(hud.transform, "ChordNameText",
            amin: new Vector2(0, 0.5f), amax: new Vector2(1, 1f),
            text: "--", size: 36, bold: true, color: new Color(0, 1, 1));

        GameObject[] beatIndicators = MakeBeatIndicators(hud.transform);

        // ── 3. BPMControl 스크립트 연결 ─────────────────────────────────────────
        BPMControl bpmComp = bpmGO.AddComponent<BPMControl>();
        var soBPM = new SerializedObject(bpmComp);
        soBPM.FindProperty("bpmSlider").objectReferenceValue = bpmSlider;
        soBPM.FindProperty("bpmText").objectReferenceValue = bpmText;
        soBPM.FindProperty("timingEngine").objectReferenceValue = timingEngine;
        soBPM.ApplyModifiedProperties();

        // ── 4. TransportControls 스크립트 연결 ───────────────────────────────────
        TransportControls transportComp = transportGO.AddComponent<TransportControls>();
        var soTC = new SerializedObject(transportComp);
        soTC.FindProperty("playButton").objectReferenceValue = playBtn;
        soTC.FindProperty("stopButton").objectReferenceValue = stopBtn;
        soTC.FindProperty("bpmControl").objectReferenceValue = bpmComp;
        soTC.FindProperty("sessionController").objectReferenceValue = sessionCtrl;
        soTC.FindProperty("uiManager").objectReferenceValue = uiManager;
        soTC.ApplyModifiedProperties();

        // ── 5. UIManager 활성화 및 참조 연결 ──────────────────────────────────────
        if (uiManager != null)
        {
            uiManager.enabled = true;
            var soUI = new SerializedObject(uiManager);
            soUI.FindProperty("chordNameText").objectReferenceValue = chordNameText;
            soUI.FindProperty("bpmValueText").objectReferenceValue = bpmText;
            soUI.FindProperty("sessionController").objectReferenceValue = sessionCtrl;
            soUI.FindProperty("timingEngine").objectReferenceValue = timingEngine;

            var beatArr = soUI.FindProperty("beatIndicators");
            beatArr.arraySize = beatIndicators.Length;
            for (int i = 0; i < beatIndicators.Length; i++)
                beatArr.GetArrayElementAtIndex(i).objectReferenceValue = beatIndicators[i];

            soUI.ApplyModifiedProperties();
        }

        // ── 6. SetupSceneReferences 재실행 (ChordProgressionPanel 참조 재확인) ────
        SetupSceneReferences.SetupReferences();

        Debug.Log("✅ UI 컨트롤 생성 및 참조 연결 완료 — ControlBar, HUD, UIManager");
    }

    // ─── 헬퍼 메서드 ────────────────────────────────────────────────────────────

    private static GameObject MakePanel(Transform parent, string name,
        Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.offsetMin = omin;
        rt.offsetMax = omax;

        if (color.a > 0f)
        {
            Image img = go.AddComponent<Image>();
            img.color = color;
        }
        return go;
    }

    private static TextMeshProUGUI MakeTMP(Transform parent, string name,
        Vector2 amin, Vector2 amax, string text, float size,
        bool bold = false, Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.offsetMin = new Vector2(4, 0);
        rt.offsetMax = new Vector2(-4, 0);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        return tmp;
    }

    private static Slider MakeSlider(Transform parent, string name, Vector2 amin, Vector2 amax)
    {
        // Background
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.offsetMin = new Vector2(8, 0);
        rt.offsetMax = new Vector2(-8, 0);

        Slider slider = go.AddComponent<Slider>();
        slider.minValue = 40;
        slider.maxValue = 240;
        slider.wholeNumbers = true;
        slider.value = 80;

        // Background image
        Image bgImg = go.AddComponent<Image>();
        bgImg.color = new Color(0.25f, 0.25f, 0.3f);
        slider.targetGraphic = bgImg;

        // Fill Area
        GameObject fillArea = MakeChild(go.transform, "Fill Area", 5,
            new Vector2(0, 0.25f), new Vector2(1, 0.75f), new Vector2(5, 0), new Vector2(-15, 0));
        GameObject fill = MakeChild(fillArea.transform, "Fill", 5,
            new Vector2(0, 0), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        fill.AddComponent<Image>().color = new Color(0, 0.75f, 0.75f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        // Handle Slide Area
        GameObject handleArea = MakeChild(go.transform, "Handle Slide Area", 5,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 0), new Vector2(-10, 0));
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        handle.layer = 5;
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 0);
        handleRt.anchorMin = new Vector2(0.5f, 0);
        handleRt.anchorMax = new Vector2(0.5f, 1);
        handle.AddComponent<Image>().color = new Color(0, 0.9f, 0.9f);
        slider.handleRect = handleRt;

        return slider;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        Vector2 amin, Vector2 amax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        go.AddComponent<Image>().color = color;
        Button btn = go.AddComponent<Button>();

        // Label text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        textGO.layer = 5;
        RectTransform textRt = textGO.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }

    private static GameObject[] MakeBeatIndicators(Transform parent)
    {
        GameObject row = new GameObject("BeatRow");
        row.transform.SetParent(parent, false);
        row.layer = 5;
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.05f, 0.05f);
        rowRt.anchorMax = new Vector2(0.95f, 0.48f);
        rowRt.offsetMin = Vector2.zero;
        rowRt.offsetMax = Vector2.zero;

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var indicators = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject dot = new GameObject($"Beat{i + 1}");
            dot.transform.SetParent(row.transform, false);
            dot.layer = 5;
            RectTransform dotRt = dot.AddComponent<RectTransform>();
            dotRt.sizeDelta = new Vector2(22, 22);
            dot.AddComponent<Image>().color = new Color(0.35f, 0.35f, 0.4f);
            indicators[i] = dot;
        }
        return indicators;
    }

    private static GameObject MakeChild(Transform parent, string name, int layer,
        Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = layer;
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.offsetMin = omin;
        rt.offsetMax = omax;
        return go;
    }
}
