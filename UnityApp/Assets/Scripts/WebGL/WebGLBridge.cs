using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// WebGL JavaScript 플러그인과 C# 간의 브릿지
/// LocalStorage.jslib의 함수들을 C#에서 호출 가능하게 함
/// </summary>
public class WebGLBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UnlockWebAudio();

    [DllImport("__Internal")]
    private static extern void SaveToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern string LoadFromLocalStorage(string key);
#endif

    /// <summary>
    /// 브라우저 오디오 잠금 해제 (사용자 제스처 후 호출)
    /// </summary>
    public static void ResumeAudioContext()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnlockWebAudio();
#endif
    }

    /// <summary>
    /// WebGL 환경인지 확인
    /// </summary>
    public static bool IsWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }
}
