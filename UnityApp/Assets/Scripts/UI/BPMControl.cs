using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BPMControl : MonoBehaviour
{
    [SerializeField] private Slider bpmSlider;
    [SerializeField] private TextMeshProUGUI bpmText;
    [SerializeField] private TimingEngine timingEngine;

    private int currentBPM = 80;

    void Start()
    {
        // 저장된 BPM 불러오기
        currentBPM = LocalStorageManager.LoadBPM();

        if (bpmSlider != null)
        {
            bpmSlider.minValue = 40;
            bpmSlider.maxValue = 240;
            bpmSlider.wholeNumbers = true;
            bpmSlider.value = currentBPM;
            bpmSlider.onValueChanged.AddListener(OnBPMChanged);
        }

        UpdateDisplay();
    }

    private void OnBPMChanged(float value)
    {
        currentBPM = (int)value;
        timingEngine.BPM = currentBPM;
        UpdateDisplay();

        // BPM 저장
        LocalStorageManager.SaveBPM(currentBPM);
    }

    private void UpdateDisplay()
    {
        if (bpmText != null)
        {
            bpmText.text = $"{currentBPM} BPM";
        }
    }

    public int GetCurrentBPM() => currentBPM;
}
