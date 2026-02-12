using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PracticeSessionController sessionController;
    [SerializeField] private TimingEngine timingEngine;

    [Header("코드 표시")]
    [SerializeField] private TextMeshProUGUI chordNameText;

    [Header("비트 인디케이터")]
    [SerializeField] private GameObject[] beatIndicators; // 4개 원형 오브젝트

    [Header("BPM 표시")]
    [SerializeField] private TextMeshProUGUI bpmValueText;

    void Update()
    {
        if (!timingEngine.IsPlaying) return;

        UpdateBeatIndicators();
    }

    /// <summary>
    /// 현재 비트에 해당하는 인디케이터 펄스 효과
    /// </summary>
    private void UpdateBeatIndicators()
    {
        int currentBeat = timingEngine.CurrentBeat;

        for (int i = 0; i < beatIndicators.Length; i++)
        {
            float targetScale = (i == currentBeat) ? 1.3f : 1.0f;
            float current = beatIndicators[i].transform.localScale.x;
            float smoothed = Mathf.Lerp(current, targetScale, Time.deltaTime * 10f);
            beatIndicators[i].transform.localScale = Vector3.one * smoothed;
        }
    }

    /// <summary>
    /// 코드 이름 UI 업데이트 (PracticeSessionController에서 호출)
    /// </summary>
    public void UpdateChordDisplay(string chordName, string chordFullName)
    {
        if (chordNameText != null)
        {
            chordNameText.text = chordFullName;
        }
    }

    /// <summary>
    /// BPM 표시 업데이트
    /// </summary>
    public void UpdateBPMDisplay(int bpm)
    {
        if (bpmValueText != null)
        {
            bpmValueText.text = bpm.ToString();
        }
    }

    /// <summary>
    /// 재생 중지 시 인디케이터 리셋
    /// </summary>
    public void ResetIndicators()
    {
        foreach (var indicator in beatIndicators)
        {
            indicator.transform.localScale = Vector3.one;
        }
    }
}
