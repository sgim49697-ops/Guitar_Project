using UnityEngine;
using System;

public class TimingEngine : MonoBehaviour
{
    // 이벤트: 외부 시스템이 구독
    public event Action<int> OnBeat;           // 매 비트마다 (비트 번호 전달)
    public event Action<int> OnMeasureStart;   // 마디 시작 (마디 번호 전달)
    public event Action OnLoopComplete;        // 진행 루프 완료

    [SerializeField] private int bpm = 80;
    [SerializeField] private int beatsPerMeasure = 4;

    private bool isPlaying;
    private double nextBeatTime;       // 다음 비트의 DSP 시간
    private int currentBeat;           // 0 ~ beatsPerMeasure-1
    private int currentMeasure;
    private int totalMeasures;

    private double secondsPerBeat;

    // 오디오 버퍼 지연을 보정하기 위한 선행 스케줄링 시간
    private const double SCHEDULE_AHEAD = 0.1; // 100ms

    public int BPM
    {
        get => bpm;
        set
        {
            bpm = Mathf.Clamp(value, 40, 240);
            secondsPerBeat = 60.0 / bpm;
        }
    }

    public bool IsPlaying => isPlaying;
    public int CurrentBeat => currentBeat;
    public int CurrentMeasure => currentMeasure;
    public int BeatsPerMeasure => beatsPerMeasure;

    /// <summary>
    /// 현재 비트 내에서의 진행률 (0.0 ~ 1.0)
    /// UI 애니메이션에 사용
    /// </summary>
    public float BeatProgress => isPlaying
        ? Mathf.Clamp01((float)((AudioSettings.dspTime - (nextBeatTime - secondsPerBeat)) / secondsPerBeat))
        : 0f;

    /// <summary>
    /// 재생 시작
    /// </summary>
    public void StartPlayback(int totalMeasureCount)
    {
        totalMeasures = totalMeasureCount;
        secondsPerBeat = 60.0 / bpm;
        currentBeat = 0;
        currentMeasure = 0;

        // 다음 오디오 버퍼 경계에서 시작 (약간의 여유)
        nextBeatTime = AudioSettings.dspTime + 0.2;
        isPlaying = true;
    }

    /// <summary>
    /// 재생 중지
    /// </summary>
    public void StopPlayback()
    {
        isPlaying = false;
        currentBeat = 0;
        currentMeasure = 0;
    }

    void Update()
    {
        if (!isPlaying) return;

        double currentDspTime = AudioSettings.dspTime;

        // while 루프: 프레임 드롭 시 여러 비트를 한 번에 처리
        while (currentDspTime + SCHEDULE_AHEAD >= nextBeatTime)
        {
            // 비트 이벤트 발생
            OnBeat?.Invoke(currentBeat);

            // 마디 시작 이벤트
            if (currentBeat == 0)
            {
                OnMeasureStart?.Invoke(currentMeasure);
            }

            // 다음 비트로 전진
            currentBeat++;
            if (currentBeat >= beatsPerMeasure)
            {
                currentBeat = 0;
                currentMeasure++;

                if (currentMeasure >= totalMeasures)
                {
                    currentMeasure = 0; // 루프
                    OnLoopComplete?.Invoke();
                }
            }

            nextBeatTime += secondsPerBeat;
        }
    }

    /// <summary>
    /// 다음 비트의 DSP 시간 (오디오 스케줄링용)
    /// </summary>
    public double GetNextBeatDspTime() => nextBeatTime;

    /// <summary>
    /// 비트당 초 (오디오 스케줄링용)
    /// </summary>
    public double GetSecondsPerBeat() => secondsPerBeat;
}
