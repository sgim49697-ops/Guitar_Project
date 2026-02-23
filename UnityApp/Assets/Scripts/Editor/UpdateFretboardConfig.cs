using UnityEngine;
using UnityEditor;

public class UpdateFretboardConfig : MonoBehaviour
{
    [MenuItem("Guitar/Update FretboardConfig Colors")]
    static void UpdateColors()
    {
        // FretboardConfig 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:FretboardConfig");

        if (guids.Length == 0)
        {
            Debug.LogError("FretboardConfig 에셋을 찾을 수 없습니다!");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FretboardConfig config = AssetDatabase.LoadAssetAtPath<FretboardConfig>(path);

            if (config != null)
            {
                // 색상 업데이트
                config.inactiveMarkerColor = new Color(0.15f, 0.15f, 0.15f, 0.3f); // 거의 투명한 어두운 회색
                config.activeMarkerColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // 밝은 시안색
                config.glowColor = new Color(1.0f, 1.0f, 0.0f, 1.0f); // 밝은 노란색

                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                Debug.LogWarning($"✅ {path} 색상 업데이트 완료!");
                Debug.LogWarning($"   Inactive: {config.inactiveMarkerColor}");
                Debug.LogWarning($"   Active: {config.activeMarkerColor}");
                Debug.LogWarning($"   Glow: {config.glowColor}");
            }
        }

        AssetDatabase.Refresh();
    }
}
