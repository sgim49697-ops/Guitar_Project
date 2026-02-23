using UnityEngine;

[RequireComponent(typeof(TimingEngine))]
public class MetronomeAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;    // 일반 비트 클릭
    [SerializeField] private AudioClip accentSound;   // 1박 강세 클릭
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;

    private TimingEngine timingEngine;

    // 더블 버퍼링: 끊김 없는 스케줄링을 위해 AudioSource 2개 사용
    private AudioSource[] sources;
    private int sourceIndex;

    void Awake()
    {
        timingEngine = GetComponent<TimingEngine>();

        sources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].volume = volume;
        }
    }

    void OnEnable()
    {
        timingEngine.OnBeat += ScheduleClick;
    }

    void OnDisable()
    {
        timingEngine.OnBeat -= ScheduleClick;
    }

    /// <summary>
    /// DSP 시간에 맞춰 클릭 사운드를 정밀하게 스케줄링
    /// Play() 대신 PlayScheduled()를 사용해야 오차가 없음
    /// </summary>
    private void ScheduleClick(int beatIndex)
    {
        AudioClip clip = (beatIndex == 0) ? accentSound : clickSound;
        if (clip == null) return;

        double scheduledTime = timingEngine.GetNextBeatDspTime();

        AudioSource src = sources[sourceIndex % 2];
        src.clip = clip;
        src.volume = volume;
        src.PlayScheduled(scheduledTime);

        sourceIndex++;
    }

    public void SetVolume(float vol)
    {
        volume = Mathf.Clamp01(vol);
        foreach (var src in sources)
        {
            src.volume = volume;
        }
    }
}
