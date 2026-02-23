// SetupSceneReferences.cs - Inspector 참조를 코드로 자동 연결하는 Editor 유틸리티
// SlotsParent: HorizontalLayoutGroup → GridLayoutGroup (90×50, 5열×2행, spacing 6)
// ChordProgressionPanel 높이: 80px → 120px
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupSceneReferences
{
    [MenuItem("Guitar/Setup Scene References")]
    public static void SetupReferences()
    {
        SetupChordProgressionPanel();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("씬 참조 연결 완료");
    }

    private static void SetupChordProgressionPanel()
    {
        var panel = Object.FindFirstObjectByType<ChordProgressionPanel>();
        if (panel == null)
        {
            Debug.LogError("ChordProgressionPanel 컴포넌트를 씬에서 찾을 수 없습니다.");
            return;
        }

        // ── 패널 높이: 80 → 120px ─────────────────────────────────────────
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        if (panelRt != null)
        {
            // anchorMin:(0,1) anchorMax:(1,1) offsetMin:(0,-120) offsetMax:(0,0)
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(1f, 1f);
            panelRt.pivot     = new Vector2(0.5f, 1f);
            panelRt.offsetMin = new Vector2(0f, -120f);
            panelRt.offsetMax = new Vector2(0f,    0f);
            Debug.Log("ChordProgressionPanel 높이 120px으로 설정 완료");
        }

        // ── ChordSlot 프리팹 ──────────────────────────────────────────────
        string prefabPath = "Assets/Prefabs/ChordSlot.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"ChordSlot 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        // ── SlotsParent 탐색 ─────────────────────────────────────────────
        Transform slotsParent = panel.transform.Find("SlotsParent");
        if (slotsParent == null)
        {
            Debug.LogError("SlotsParent 자식 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // ── HorizontalLayoutGroup 제거, GridLayoutGroup 추가 ─────────────
        HorizontalLayoutGroup hlg = slotsParent.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            Object.DestroyImmediate(hlg);
            Debug.Log("HorizontalLayoutGroup 제거 완료");
        }

        GridLayoutGroup glg = slotsParent.GetComponent<GridLayoutGroup>();
        if (glg == null)
            glg = slotsParent.gameObject.AddComponent<GridLayoutGroup>();

        glg.cellSize            = new Vector2(90f, 50f);
        glg.spacing             = new Vector2(6f,  6f);
        glg.startAxis           = GridLayoutGroup.Axis.Horizontal;
        glg.startCorner         = GridLayoutGroup.Corner.UpperLeft;
        glg.childAlignment      = TextAnchor.UpperCenter;
        glg.constraint          = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount     = 5;
        Debug.Log("GridLayoutGroup 설정 완료 — cellSize:(90,50) spacing:(6,6) 5열×2행");

        // ── SlotsParent RectTransform: 운지 슬롯을 약간 위로 이동 ────────
        RectTransform slotsRt = slotsParent.GetComponent<RectTransform>();
        if (slotsRt != null)
        {
            slotsRt.anchorMin = Vector2.zero;
            slotsRt.anchorMax = Vector2.one;
            // 기존 inset: bottom 4 / top 4
            // 변경 inset: bottom 8 / top 0  -> 슬롯 중심이 약 4px 위로 이동
            slotsRt.offsetMin = new Vector2(8f,  8f);
            slotsRt.offsetMax = new Vector2(-8f, 0f);
        }

        // ── FretboardRenderer (씬에서 탐색) ──────────────────────────────
        var fretboard = Object.FindFirstObjectByType<FretboardRenderer>();

        // ── ChordDatabase (에셋에서 로드) ─────────────────────────────────
        string dbPath  = "Assets/Data/ChordDatabase.asset";
        var chordDb    = AssetDatabase.LoadAssetAtPath<ChordDatabase>(dbPath);

        // ── 참조 설정 ─────────────────────────────────────────────────────
        SerializedObject so = new SerializedObject(panel);
        so.FindProperty("chordSlotPrefab").objectReferenceValue   = prefab;
        so.FindProperty("slotsParent").objectReferenceValue       = slotsParent;
        so.FindProperty("fretboardRenderer").objectReferenceValue = fretboard;
        so.FindProperty("chordDatabase").objectReferenceValue     = chordDb;
        so.ApplyModifiedProperties();

        Debug.Log($"ChordProgressionPanel 참조 연결 완료 — fretboard:{(fretboard != null ? "OK" : "None")}, db:{(chordDb != null ? "OK" : "None")}");
    }
}
