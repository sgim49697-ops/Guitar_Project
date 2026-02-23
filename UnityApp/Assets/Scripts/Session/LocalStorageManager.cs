using UnityEngine;
using System.Runtime.InteropServices;

public class LocalStorageManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SaveToLocalStorage(string key, string value);
    [DllImport("__Internal")] private static extern string LoadFromLocalStorage(string key);
    [DllImport("__Internal")] private static extern int HasLocalStorageKey(string key);
    [DllImport("__Internal")] private static extern void RemoveFromLocalStorage(string key);
#endif

    /// <summary>
    /// 키-값 저장 (WebGL: localStorage, Editor: PlayerPrefs)
    /// </summary>
    public static void Save(string key, string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToLocalStorage(key, json);
#else
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
#endif
    }

    /// <summary>
    /// 키로 값 불러오기
    /// </summary>
    public static string Load(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LoadFromLocalStorage(key);
#else
        return PlayerPrefs.GetString(key, "");
#endif
    }

    /// <summary>
    /// 연습 세션 저장
    /// </summary>
    public static void SaveSession(PracticeSession session)
    {
        string json = JsonUtility.ToJson(session);
        Save("guitar_practice_session", json);
    }

    /// <summary>
    /// 연습 세션 불러오기
    /// </summary>
    public static PracticeSession LoadSession()
    {
        string json = Load("guitar_practice_session");
        if (string.IsNullOrEmpty(json)) return null;
        return JsonUtility.FromJson<PracticeSession>(json);
    }

    /// <summary>
    /// BPM 설정 저장
    /// </summary>
    public static void SaveBPM(int bpm)
    {
        Save("guitar_bpm", bpm.ToString());
    }

    /// <summary>
    /// BPM 설정 불러오기 (기본값 80)
    /// </summary>
    public static int LoadBPM()
    {
        string val = Load("guitar_bpm");
        if (int.TryParse(val, out int bpm))
            return bpm;
        return 80;
    }
}
