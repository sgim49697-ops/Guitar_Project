using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class TransportControls : MonoBehaviour
{
    [SerializeField] private PracticeSessionController sessionController;
    [SerializeField] private BPMControl bpmControl;
    [SerializeField] private UIManager uiManager;

    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private ChordSelector chordSelector; // null이면 기본 진행 사용

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UnlockWebAudio();
#endif

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopClicked);

        // 시작 시 Stop 버튼 비활성화
        if (stopButton != null)
            stopButton.interactable = false;
    }

    /// <summary>
    /// Play 버튼 클릭
    /// </summary>
    private void OnPlayClicked()
    {
        // WebGL에서 오디오 컨텍스트 잠금 해제 (브라우저 정책)
#if UNITY_WEBGL && !UNITY_EDITOR
        UnlockWebAudio();
#endif

        int bpm = bpmControl.GetCurrentBPM();

        if (chordSelector != null)
        {
            List<string> selected = chordSelector.GetSelectedChords();
            if (selected == null || selected.Count == 0)
            {
                Debug.LogWarning("[TransportControls] 재생목록이 비어 있습니다. 상단 코드 버튼을 눌러 코드를 추가하세요.");
                return;
            }

            sessionController.StartCustomSession(selected, bpm);
        }
        else
        {
            sessionController.StartDefaultSession(bpm);
        }

        // 버튼 상태 전환
        if (playButton != null) playButton.interactable = false;
        if (stopButton != null) stopButton.interactable = true;
    }

    /// <summary>
    /// Stop 버튼 클릭
    /// </summary>
    private void OnStopClicked()
    {
        sessionController.StopSession();
        uiManager.ResetIndicators();

        // 버튼 상태 전환
        if (playButton != null) playButton.interactable = true;
        if (stopButton != null) stopButton.interactable = false;
    }
}
