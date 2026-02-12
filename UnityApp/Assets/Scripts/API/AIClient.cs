using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

/// <summary>
/// FastAPI AI 서버와 통신하는 클라이언트
/// Unity WebGL에서 HTTP 요청 처리
/// </summary>
public class AIClient : MonoBehaviour
{
    private const string API_BASE_URL = "http://localhost:8080";

    // ==================== 데이터 모델 ====================

    [Serializable]
    public class ChordExplainRequest
    {
        public string chord_name;
    }

    [Serializable]
    public class ChordExplainResponse
    {
        public string chord_name;
        public string explanation;
        public string[] tips;
        public string[] similar_chords;
    }

    [Serializable]
    public class AlternativeFingeringRequest
    {
        public string chord_name;
        public string difficulty = "beginner";
    }

    [Serializable]
    public class AlternativeFingering
    {
        public string name;
        public string description;
        public string positions;
        public string difficulty;
    }

    [Serializable]
    public class AlternativeFingeringResponse
    {
        public string chord_name;
        public AlternativeFingering[] alternatives;
    }

    [Serializable]
    public class PracticeRoutineRequest
    {
        public string skill_level;
        public int available_time_minutes;
        public string focus_area;
    }

    [Serializable]
    public class RoutineActivity
    {
        public string activity;
        public int duration;
    }

    [Serializable]
    public class PracticeRoutineResponse
    {
        public RoutineActivity[] routine;
        public int total_minutes;
        public string difficulty;
    }

    // ==================== API 호출 메서드 ====================

    /// <summary>
    /// 코드 설명 요청
    /// </summary>
    public void ExplainChord(string chordName, Action<ChordExplainResponse> onSuccess, Action<string> onError)
    {
        var request = new ChordExplainRequest { chord_name = chordName };
        StartCoroutine(PostRequest("/api/explain-chord", request, onSuccess, onError));
    }

    /// <summary>
    /// 대체 운지법 요청
    /// </summary>
    public void GetAlternativeFingering(string chordName, string difficulty,
        Action<AlternativeFingeringResponse> onSuccess, Action<string> onError)
    {
        var request = new AlternativeFingeringRequest { chord_name = chordName, difficulty = difficulty };
        StartCoroutine(PostRequest("/api/alternative-fingering", request, onSuccess, onError));
    }

    /// <summary>
    /// 연습 루틴 생성 요청
    /// </summary>
    public void GeneratePracticeRoutine(string skillLevel, int timeMinutes, string focusArea,
        Action<PracticeRoutineResponse> onSuccess, Action<string> onError)
    {
        var request = new PracticeRoutineRequest
        {
            skill_level = skillLevel,
            available_time_minutes = timeMinutes,
            focus_area = focusArea
        };
        StartCoroutine(PostRequest("/api/practice-routine", request, onSuccess, onError));
    }

    // ==================== 헬퍼 메서드 ====================

    /// <summary>
    /// 제네릭 POST 요청
    /// </summary>
    private IEnumerator PostRequest<TRequest, TResponse>(
        string endpoint,
        TRequest requestData,
        Action<TResponse> onSuccess,
        Action<string> onError)
    {
        string url = API_BASE_URL + endpoint;
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(www.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    onError?.Invoke($"JSON 파싱 오류: {e.Message}");
                }
            }
            else
            {
                onError?.Invoke($"API 오류: {www.error}");
            }
        }
    }

    /// <summary>
    /// 서버 헬스체크
    /// </summary>
    public void CheckHealth(Action<bool> onComplete)
    {
        StartCoroutine(HealthCheckCoroutine(onComplete));
    }

    private IEnumerator HealthCheckCoroutine(Action<bool> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(API_BASE_URL + "/health"))
        {
            yield return www.SendWebRequest();
            onComplete?.Invoke(www.result == UnityWebRequest.Result.Success);
        }
    }
}
