using UnityEngine;
using System.Collections.Generic;

public class PracticeSessionController : MonoBehaviour
{
    [SerializeField] private TimingEngine timingEngine;
    [SerializeField] private FretboardRenderer fretboardRenderer;
    [SerializeField] private ChordDatabase chordDatabase;
    [SerializeField] private ChordProgressionPanel progressionPanel;

    private List<TimelineEvent> currentProgression;
    private int currentEventIndex;
    private bool isSessionActive;

    // 기본 코드 진행: C -> G -> Am -> F
    private readonly string[] defaultProgression = { "C", "G", "Am", "F" };

    /// <summary>
    /// 연습 세션 시작
    /// </summary>
    public void StartSession(List<TimelineEvent> progression, int bpm)
    {
        currentProgression = progression;
        currentEventIndex = 0;

        timingEngine.BPM = bpm;
        timingEngine.OnMeasureStart += OnMeasureChanged;
        timingEngine.OnLoopComplete += OnLoopComplete;

        // 첫 번째 코드 즉시 표시
        ShowChord(0);

        // 타이밍 시작 (코드 하나당 1마디)
        timingEngine.StartPlayback(currentProgression.Count);
        isSessionActive = true;
    }

    /// <summary>
    /// 연습 세션 중지
    /// </summary>
    public void StopSession()
    {
        if (!isSessionActive) return;

        timingEngine.StopPlayback();
        timingEngine.OnMeasureStart -= OnMeasureChanged;
        timingEngine.OnLoopComplete -= OnLoopComplete;
        fretboardRenderer.ClearAllHighlights();
        isSessionActive = false;
    }

    /// <summary>
    /// 마디가 바뀔 때 호출 (TimingEngine 이벤트)
    /// </summary>
    private void OnMeasureChanged(int measureIndex)
    {
        currentEventIndex = measureIndex;
        ShowChord(measureIndex);
    }

    /// <summary>
    /// 지판에 코드 표시
    /// </summary>
    private void ShowChord(int index)
    {
        if (index < 0 || index >= currentProgression.Count) return;

        string chordName = currentProgression[index].chordName;
        ChordData chord = chordDatabase.GetChord(chordName);

        if (chord != null)
        {
            fretboardRenderer.HighlightChord(chord);

            if (progressionPanel != null)
            {
                progressionPanel.SetActiveChord(index);
            }
        }
    }

    /// <summary>
    /// 루프 완료 시 호출
    /// </summary>
    private void OnLoopComplete()
    {
        // 루프 카운터 증가 등 통계 처리 가능
    }

    /// <summary>
    /// 기본 진행으로 세션 시작 (UI에서 호출)
    /// </summary>
    public void StartDefaultSession(int bpm)
    {
        var timeline = new List<TimelineEvent>();
        foreach (string name in defaultProgression)
        {
            timeline.Add(new TimelineEvent { chordName = name, durationInBeats = 4f });
        }
        StartSession(timeline, bpm);
    }

    /// <summary>
    /// 커스텀 진행으로 세션 시작
    /// </summary>
    public void StartCustomSession(List<string> chordNames, int bpm)
    {
        var timeline = new List<TimelineEvent>();
        foreach (string name in chordNames)
        {
            timeline.Add(new TimelineEvent { chordName = name, durationInBeats = 4f });
        }
        StartSession(timeline, bpm);
    }

    public bool IsSessionActive => isSessionActive;
}
