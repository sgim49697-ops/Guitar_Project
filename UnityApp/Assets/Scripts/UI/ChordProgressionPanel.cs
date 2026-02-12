using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChordProgressionPanel : MonoBehaviour
{
    [SerializeField] private GameObject chordSlotPrefab; // 코드 슬롯 프리팹
    [SerializeField] private Transform slotsParent;       // 슬롯 부모 (HorizontalLayoutGroup)

    [Header("색상")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f);
    [SerializeField] private Color activeColor = new Color(0.3f, 0.3f, 0.6f);
    [SerializeField] private Color normalTextColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color activeTextColor = Color.white;

    private List<GameObject> slots = new List<GameObject>();
    private int activeIndex = -1;

    /// <summary>
    /// 코드 진행 슬롯 생성
    /// </summary>
    public void SetProgression(List<string> chordNames)
    {
        // 기존 슬롯 제거
        foreach (var slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();

        // 새 슬롯 생성
        foreach (string name in chordNames)
        {
            GameObject slot = Instantiate(chordSlotPrefab, slotsParent);
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = name;
                text.color = normalTextColor;
            }

            Image bg = slot.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }

            slots.Add(slot);
        }

        activeIndex = -1;
    }

    /// <summary>
    /// 현재 재생 중인 코드 하이라이트
    /// </summary>
    public void SetActiveChord(int index)
    {
        // 이전 활성 슬롯 비활성화
        if (activeIndex >= 0 && activeIndex < slots.Count)
        {
            SetSlotActive(activeIndex, false);
        }

        // 새 슬롯 활성화
        if (index >= 0 && index < slots.Count)
        {
            SetSlotActive(index, true);
        }

        activeIndex = index;
    }

    private void SetSlotActive(int index, bool active)
    {
        GameObject slot = slots[index];

        Image bg = slot.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = active ? activeColor : normalColor;
        }

        TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = active ? activeTextColor : normalTextColor;
        }

        // 활성 슬롯 스케일 효과
        float targetScale = active ? 1.15f : 1.0f;
        slot.transform.localScale = Vector3.one * targetScale;
    }

    /// <summary>
    /// 모든 슬롯 비활성화
    /// </summary>
    public void ResetAll()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SetSlotActive(i, false);
        }
        activeIndex = -1;
    }
}
