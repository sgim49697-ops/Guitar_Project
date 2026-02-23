// PracticeSessionController.cs - 기타 연습 세션 흐름 제어 (코드 진행, 타이밍, UI 연동)
using UnityEngine;
using System.Collections.Generic;

public class PracticeSessionController : MonoBehaviour
{
    [SerializeField] private TimingEngine timingEngine;
    [SerializeField] private FretboardRenderer fretboardRenderer;
    [SerializeField] private ChordDatabase chordDatabase;
    [SerializeField] private ChordProgressionPanel progressionPanel;

    private List<TimelineEvent> currentProgression;
    private int  currentEventIndex;
    private bool isSessionActive;

    // 세션 최초 시작 여부 (첫 번째 ShowChord는 HighlightChord, 이후는 TransitionToChord)
    private bool isFirstChord;

    // 기본 코드 진행: C -> G -> Am -> F
    private readonly string[] defaultProgression = { "C", "G", "Am", "F" };

    // ── Unity 생명주기 ────────────────────────────────────────────────────
    void Awake()
    {
        Debug.LogWarning("PracticeSessionController Awake() 호출됨");
    }

    void OnEnable()
    {
        Debug.LogWarning("PracticeSessionController OnEnable() 호출됨");
    }

    void Start()
    {
        Debug.LogWarning("PracticeSessionController Start() 호출");

        Debug.LogWarning($"fretboardRenderer: {(fretboardRenderer != null ? "연결됨" : "None")}");
        Debug.LogWarning($"chordDatabase:     {(chordDatabase     != null ? "연결됨" : "None")}");
        Debug.LogWarning($"progressionPanel:  {(progressionPanel  != null ? "연결됨" : "None")}");

        // 전체 코드 슬롯을 세션 시작 전에 미리 표시
        if (progressionPanel != null && chordDatabase != null)
        {
            progressionPanel.InitAllChords(chordDatabase);
        }

        // 디버그: C 코드 하이라이트 확인
        if (fretboardRenderer != null && chordDatabase != null)
        {
            var allChords = chordDatabase.GetAllChordNames();
            Debug.LogWarning($"ChordDatabase 코드 목록: {string.Join(", ", allChords)}");

            ChordData cChord = chordDatabase.GetChord("C");
            if (cChord != null)
            {
                fretboardRenderer.HighlightChord(cChord);
                Debug.LogWarning("C 코드 하이라이트 완료");
            }
            else
            {
                Debug.LogError("C 코드를 찾을 수 없습니다");
            }
        }
        else
        {
            Debug.LogError("fretboardRenderer 또는 chordDatabase가 연결되지 않았습니다");
        }
    }

    // ── 세션 시작 ─────────────────────────────────────────────────────────
    /// <summary>
    /// 연습 세션 시작. 슬롯을 재생성하지 않고 세션 코드만 하이라이트한다.
    /// </summary>
    public void StartSession(List<TimelineEvent> progression, int bpm)
    {
        currentProgression = progression;
        currentEventIndex  = 0;
        isFirstChord       = true;

        // 슬롯 재생성 없이 세션 코드 하이라이트만 갱신
        if (progressionPanel != null)
        {
            var chordNames = new List<string>();
            foreach (var e in progression) chordNames.Add(e.chordName);
            progressionPanel.SetSessionHighlights(chordNames);
        }

        timingEngine.BPM = bpm;
        timingEngine.OnBeat          += OnBeatTick;
        timingEngine.OnMeasureStart  += OnMeasureChanged;
        timingEngine.OnLoopComplete  += OnLoopComplete;

        // 첫 번째 코드 즉시 표시 (HighlightChord)
        ShowChord(0);

        // 코드 하나당 1마디 재생
        timingEngine.StartPlayback(currentProgression.Count);
        isSessionActive = true;
    }

    // ── 세션 중지 ─────────────────────────────────────────────────────────
    public void StopSession()
    {
        if (!isSessionActive) return;

        timingEngine.StopPlayback();
        timingEngine.OnBeat          -= OnBeatTick;
        timingEngine.OnMeasureStart  -= OnMeasureChanged;
        timingEngine.OnLoopComplete  -= OnLoopComplete;

        // 마지막 코드 운지 유지 — ClearAllHighlights 호출 안 함
        isSessionActive = false;
    }

    // ── 비트 이벤트 ───────────────────────────────────────────────────────
    /// <summary>
    /// 매 비트마다 호출. 마지막 비트(beatsPerMeasure - 1)에 다음 코드 예고.
    /// </summary>
    private void OnBeatTick(int beatIndex)
    {
        if (!isSessionActive || currentProgression == null) return;

        // 마지막 비트에 다음 코드 예고
        if (beatIndex == timingEngine.BeatsPerMeasure - 1)
        {
            int nextIndex = (currentEventIndex + 1) % currentProgression.Count;
            string nextChordName = currentProgression[nextIndex].chordName;
            ChordData nextChord  = chordDatabase.GetChord(nextChordName);

            if (nextChord != null)
                fretboardRenderer.PreviewNextChord(nextChord);
        }
    }

    // ── 마디 이벤트 ───────────────────────────────────────────────────────
    private void OnMeasureChanged(int measureIndex)
    {
        currentEventIndex = measureIndex;
        ShowChord(measureIndex);
    }

    private void OnLoopComplete()
    {
        // 루프 카운터 증가 등 통계 처리 가능
    }

    // ── 코드 표시 ─────────────────────────────────────────────────────────
    /// <summary>
    /// 지판 하이라이트 + 패널 활성 슬롯을 코드 이름으로 갱신한다.
    /// 첫 코드는 즉시(HighlightChord), 이후는 크로스페이드(TransitionToChord).
    /// </summary>
    private void ShowChord(int index)
    {
        if (currentProgression == null) return;
        if (index < 0 || index >= currentProgression.Count) return;

        string    chordName = currentProgression[index].chordName;
        ChordData chord     = chordDatabase.GetChord(chordName);

        if (chord == null) return;

        if (isFirstChord)
        {
            // 세션 시작 첫 코드 — 즉시 하이라이트
            fretboardRenderer.HighlightChord(chord);
            isFirstChord = false;
        }
        else
        {
            // 이후 코드 전환 — BPM에 비례한 크로스페이드 (0.8 beat = 기존 0.4의 2배)
            float crossfadeDuration = (float)timingEngine.GetSecondsPerBeat() * 0.8f;
            fretboardRenderer.TransitionToChord(chord, crossfadeDuration);
        }

        // UI 패널 활성 슬롯 갱신
        if (progressionPanel != null)
            progressionPanel.SetActiveChord(chordName);
    }

    // ── 퍼블릭 진입점 ─────────────────────────────────────────────────────
    /// <summary>기본 진행(C-G-Am-F)으로 세션 시작 (UI 버튼용).</summary>
    public void StartDefaultSession(int bpm)
    {
        var timeline = new List<TimelineEvent>();
        foreach (string name in defaultProgression)
            timeline.Add(new TimelineEvent { chordName = name, durationInBeats = 4f });
        StartSession(timeline, bpm);
    }

    /// <summary>커스텀 진행으로 세션 시작.</summary>
    public void StartCustomSession(List<string> chordNames, int bpm)
    {
        var timeline = new List<TimelineEvent>();
        foreach (string name in chordNames)
            timeline.Add(new TimelineEvent { chordName = name, durationInBeats = 4f });
        StartSession(timeline, bpm);
    }

    public bool IsSessionActive => isSessionActive;
}
