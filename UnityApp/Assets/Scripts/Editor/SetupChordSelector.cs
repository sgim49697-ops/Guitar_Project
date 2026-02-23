// SetupChordSelector.cs - SelectorBar UI 자동 생성 (하단 인터페이스 세로 높이 66% 축소)
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public class SetupChordSelector
{
    private const float ItemHeightScale = 0.66f;

    [MenuItem("Guitar/Setup Chord Selector")]
    public static void Create()
    {
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("❌ Canvas not found"); return; }

        // 기존 SelectorBar 제거 후 재생성
        var existing = canvasGO.transform.Find("SelectorBar");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // 의존 컴포넌트 수집
        var sessionCtrl = Object.FindFirstObjectByType<PracticeSessionController>();
        var progPanel = Object.FindFirstObjectByType<ChordProgressionPanel>();
        var bpmCtrl = Object.FindFirstObjectByType<BPMControl>();
        var transport = Object.FindFirstObjectByType<TransportControls>();

        // ChordDatabase — AssetDatabase 검색
        ChordDatabase chordDB = null;
        string[] guids = AssetDatabase.FindAssets("t:ChordDatabase");
        if (guids.Length > 0)
            chordDB = AssetDatabase.LoadAssetAtPath<ChordDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));

        // ── SelectorBar (ControlBar 위, 인터페이스 세로 높이 66% 반영) ────
        // 원본 높이 48px의 66% -> 31.68px
        var selectorBar = new GameObject("SelectorBar");
        selectorBar.transform.SetParent(canvasGO.transform, false);
        selectorBar.layer = 5;
        var sbRt = selectorBar.AddComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(0f, 0f);
        sbRt.anchorMax = new Vector2(0.65f, 0f);

        const float selectorBottomY = 122f;
        const float selectorTopY = 170f;
        const float selectorHeightScale = 0.66f;
        float selectorCenterY = (selectorBottomY + selectorTopY) * 0.5f;
        float selectorHalfHeight = (selectorTopY - selectorBottomY) * 0.5f * selectorHeightScale;
        sbRt.offsetMin = new Vector2(0f, selectorCenterY - selectorHalfHeight);
        sbRt.offsetMax = new Vector2(0f, selectorCenterY + selectorHalfHeight);
        selectorBar.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.09f, 0.95f);

        // ── 슬롯 영역 (빨강 박스) ─────────────────────────────────────────
        var slotsArea = new GameObject("SlotsArea");
        slotsArea.transform.SetParent(selectorBar.transform, false);
        slotsArea.layer = 5;
        var saRt = slotsArea.AddComponent<RectTransform>();
        saRt.anchorMin = new Vector2(0, 0);
        saRt.anchorMax = new Vector2(1, 1);
        saRt.offsetMin = new Vector2(6, 4);
        saRt.offsetMax = new Vector2(-340, -4);

        var hlg = slotsArea.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(2, 2, 0, 0);

        // ── 우측 버튼 그룹 (파랑 박스: [-][+][Apply]) ────────────────────
        var btnGroup = new GameObject("SelectorButtons");
        btnGroup.transform.SetParent(selectorBar.transform, false);
        btnGroup.layer = 5;
        var bgRt = btnGroup.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(1, 0);
        bgRt.anchorMax = new Vector2(1, 1);
        bgRt.pivot = new Vector2(1, 0.5f);
        bgRt.offsetMin = new Vector2(-336, 4);
        bgRt.offsetMax = new Vector2(-4, -4);

        var btnHlg = btnGroup.AddComponent<HorizontalLayoutGroup>();
        btnHlg.spacing = 4;
        btnHlg.childAlignment = TextAnchor.MiddleCenter;
        btnHlg.childControlWidth = true;
        btnHlg.childControlHeight = true;
        btnHlg.childForceExpandWidth = false;
        btnHlg.childForceExpandHeight = false;

        var removeBtn = MakeSmallButton(btnGroup.transform, "RemoveBtn", "−",
            new Color(0.48f, 0.14f, 0.14f), 34);
        var addBtn = MakeSmallButton(btnGroup.transform, "AddBtn", "+",
            new Color(0.14f, 0.40f, 0.14f), 34);
        var applyBtn = MakeSmallButton(btnGroup.transform, "ApplyBtn", "Apply",
            new Color(0.10f, 0.40f, 0.40f), 82);
        var clearBtn = MakeSmallButton(btnGroup.transform, "ClearBtn", "Clear",
            new Color(0.40f, 0.26f, 0.10f), 82);

        // ── ChordSelector 컴포넌트 연결 ──────────────────────────────────
        var sel = selectorBar.AddComponent<ChordSelector>();
        var so = new SerializedObject(sel);
        so.FindProperty("dropdownContainer").objectReferenceValue = saRt;
        so.FindProperty("addButton").objectReferenceValue = addBtn;
        so.FindProperty("removeButton").objectReferenceValue = removeBtn;
        so.FindProperty("applyButton").objectReferenceValue = applyBtn;
        so.FindProperty("clearButton").objectReferenceValue = clearBtn;
        so.FindProperty("sessionController").objectReferenceValue = sessionCtrl;
        so.FindProperty("progressionPanel").objectReferenceValue = progPanel;
        so.FindProperty("bpmControl").objectReferenceValue = bpmCtrl;
        so.FindProperty("chordDatabase").objectReferenceValue = chordDB;
        so.ApplyModifiedProperties();

        // ChordProgressionPanel에 ChordSelector 참조 연결 (상단 클릭 -> 하단 저장)
        if (progPanel != null)
        {
            var soP = new SerializedObject(progPanel);
            soP.FindProperty("chordSelector").objectReferenceValue = sel;
            soP.ApplyModifiedProperties();
        }

        // ── TransportControls에 ChordSelector 참조 추가 ──────────────────
        if (transport != null)
        {
            var soT = new SerializedObject(transport);
            soT.FindProperty("chordSelector").objectReferenceValue = sel;
            soT.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("✅ ChordSelector 생성 완료 (하단 인터페이스 세로 높이 66%, 상단 클릭-재생목록 연동)");
        if (chordDB == null)
            Debug.LogWarning("⚠ ChordDatabase를 찾지 못했습니다. Inspector에서 수동 연결 필요");
    }

    private static Button MakeSmallButton(Transform parent, string name, string label,
        Color bgColor, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = 5;

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 48f * ItemHeightScale;
        le.preferredHeight = 48f * ItemHeightScale;
        le.flexibleHeight = 0f;

        var rtBtn = go.GetComponent<RectTransform>();
        if (rtBtn != null)
            rtBtn.sizeDelta = new Vector2(width, 48f * ItemHeightScale);

        go.AddComponent<Image>().color = bgColor;
        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        textGO.layer = 5;
        var rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = label.Length > 2 ? 13 : 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }
}
