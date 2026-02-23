// ChordSlotCreator.cs - ChordSlot UI 프리팹 자동 생성 Editor 유틸리티
// 슬롯 크기: 90×50px, 폰트: 20px (GridLayoutGroup 5열×2행 기준)
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class ChordSlotCreator
{
    static ChordSlotCreator()
    {
        // 프리팹이 없을 때만 자동 생성
        if (!System.IO.File.Exists("Assets/Prefabs/ChordSlot.prefab"))
        {
            EditorApplication.delayCall += CreateChordSlotPrefab;
        }
    }

    [MenuItem("Guitar/Create ChordSlot Prefab")]
    public static void CreateChordSlotPrefab()
    {
        // 루트 오브젝트 생성 (Image 포함)
        GameObject slot = new GameObject("ChordSlot");
        RectTransform rt = slot.AddComponent<RectTransform>();
        // GridLayoutGroup이 cellSize를 덮어쓰지만 프리팹 기준 크기를 명시
        rt.sizeDelta = new Vector2(90f, 50f);
        Image bg = slot.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.2f, 0.38f, 1f);

        // Button 컴포넌트 (클릭 → 운지 표시)
        Button btn = slot.AddComponent<Button>();
        btn.targetGraphic = bg;
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor     = new Color(0.82f, 0.82f, 0.82f, 1f);
        cb.selectedColor    = Color.white;
        btn.colors = cb;

        // TextMeshPro 자식 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(slot.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = "C";
        tmp.fontSize  = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.7f, 0.7f, 0.7f, 1f);

        // 프리팹 저장
        string path = "Assets/Prefabs/ChordSlot.prefab";
        // Assets/Prefabs 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slot, path);
        Object.DestroyImmediate(slot);

        if (prefab != null)
        {
            Debug.Log($"ChordSlot 프리팹 생성 완료 (90x50, 20px): {path}");
            Selection.activeObject = prefab;
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("ChordSlot 프리팹 생성 실패");
        }
    }
}
