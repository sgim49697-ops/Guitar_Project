using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 코드 정보 패널 - AI 서버에서 코드 설명 가져오기
/// </summary>
public class ChordInfoPanel : MonoBehaviour
{
    [SerializeField] private AIClient aiClient;
    [SerializeField] private TextMeshProUGUI chordNameText;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TextMeshProUGUI tipsText;
    [SerializeField] private TextMeshProUGUI similarChordsText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private GameObject errorPanel;

    private string currentChord;

    /// <summary>
    /// 코드 정보 요청
    /// </summary>
    public void ShowChordInfo(string chordName)
    {
        currentChord = chordName;
        chordNameText.text = chordName;

        // 로딩 표시
        loadingIndicator.SetActive(true);
        errorPanel.SetActive(false);
        explanationText.text = "코드 정보를 가져오는 중...";
        tipsText.text = "";
        similarChordsText.text = "";

        // API 호출
        aiClient.ExplainChord(chordName, OnChordInfoReceived, OnError);
    }

    /// <summary>
    /// API 응답 성공
    /// </summary>
    private void OnChordInfoReceived(AIClient.ChordExplainResponse response)
    {
        loadingIndicator.SetActive(false);

        // 설명
        explanationText.text = response.explanation;

        // 팁
        if (response.tips != null && response.tips.Length > 0)
        {
            tipsText.text = "<b>팁:</b>\n";
            foreach (var tip in response.tips)
            {
                tipsText.text += $"• {tip}\n";
            }
        }

        // 비슷한 코드
        if (response.similar_chords != null && response.similar_chords.Length > 0)
        {
            similarChordsText.text = "<b>비슷한 코드:</b> " + string.Join(", ", response.similar_chords);
        }
    }

    /// <summary>
    /// API 오류 처리
    /// </summary>
    private void OnError(string error)
    {
        loadingIndicator.SetActive(false);
        errorPanel.SetActive(true);
        explanationText.text = $"오류: {error}";

        Debug.LogError($"[ChordInfoPanel] {error}");
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
