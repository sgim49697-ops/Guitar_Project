using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChordSelector : MonoBehaviour
{
    [SerializeField] private ChordDatabase chordDatabase;
    [SerializeField] private PracticeSessionController sessionController;
    [SerializeField] private ChordProgressionPanel progressionPanel;
    [SerializeField] private BPMControl bpmControl;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown[] chordDropdowns; // 4개 드롭다운 (4마디 진행)
    [SerializeField] private Button applyButton;

    private List<string> chordNames;

    void Start()
    {
        chordNames = chordDatabase.GetAllChordNames();

        // 드롭다운 초기화
        foreach (var dropdown in chordDropdowns)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(chordNames);
        }

        // 기본 진행 설정: C(0), G(1), Am(2), F(3)
        SetDefaultSelection();

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyClicked);
        }
    }

    private void SetDefaultSelection()
    {
        string[] defaults = { "C", "G", "Am", "F" };

        for (int i = 0; i < chordDropdowns.Length && i < defaults.Length; i++)
        {
            int idx = chordNames.IndexOf(defaults[i]);
            if (idx >= 0)
            {
                chordDropdowns[i].value = idx;
            }
        }
    }

    /// <summary>
    /// 적용 버튼 클릭: 선택된 코드 진행으로 세션 시작
    /// </summary>
    private void OnApplyClicked()
    {
        // 현재 세션 중지
        sessionController.StopSession();

        // 선택된 코드 수집
        List<string> selectedChords = new List<string>();
        foreach (var dropdown in chordDropdowns)
        {
            selectedChords.Add(chordNames[dropdown.value]);
        }

        // UI 업데이트
        progressionPanel.SetProgression(selectedChords);

        // 새 세션 시작
        int bpm = bpmControl.GetCurrentBPM();
        sessionController.StartCustomSession(selectedChords, bpm);
    }

    /// <summary>
    /// 현재 선택된 코드 진행 반환
    /// </summary>
    public List<string> GetSelectedChords()
    {
        List<string> selected = new List<string>();
        foreach (var dropdown in chordDropdowns)
        {
            selected.Add(chordNames[dropdown.value]);
        }
        return selected;
    }
}
