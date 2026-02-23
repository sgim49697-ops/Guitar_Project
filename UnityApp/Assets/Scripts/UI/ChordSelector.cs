// ChordSelector.cs - 코드 재생목록 선택 UI (상단 코드 클릭으로 순차 저장, +/-로 슬롯 수 조절)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChordSelector : MonoBehaviour
{
    private const float ItemHeightScale = 0.66f;
    [SerializeField] private ChordDatabase chordDatabase;
    [SerializeField] private PracticeSessionController sessionController;
    [SerializeField] private ChordProgressionPanel progressionPanel;
    [SerializeField] private BPMControl bpmControl;

    [Header("컨테이너")]
    [SerializeField] private RectTransform dropdownContainer;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button clearButton;

    [Header("재생목록 설정")]
    [SerializeField, Range(1, 8)] private int initialCapacity = 4;

    private const int MinCapacity = 1;
    private const int MaxCapacity = 8;

    private readonly List<string> selectedNames = new List<string>();
    private readonly List<GameObject> slotObjects = new List<GameObject>();
    private readonly List<TextMeshProUGUI> slotLabels = new List<TextMeshProUGUI>();
    private List<string> chordNames;
    private int playlistCapacity;

    void Start()
    {
        if (chordDatabase == null)
        {
            Debug.LogError("[ChordSelector] chordDatabase가 연결되지 않았습니다!");
            return;
        }

        chordNames = chordDatabase.GetAllChordNames();
        if (chordNames == null || chordNames.Count == 0)
        {
            Debug.LogError("[ChordSelector] ChordDatabase에 코드가 없습니다.");
            return;
        }

        // 기본 목록은 비어있게 시작하고, 저장 가능한 슬롯 수만 먼저 만든다.
        playlistCapacity = Mathf.Clamp(initialCapacity, MinCapacity, MaxCapacity);
        for (int i = 0; i < playlistCapacity; i++)
        {
            CreateEmptySlot();
        }

        if (addButton) addButton.onClick.AddListener(OnAddClicked);
        if (removeButton) removeButton.onClick.AddListener(OnRemoveClicked);
        if (applyButton) applyButton.onClick.AddListener(OnApplyClicked);
        EnsureClearButton();
        if (clearButton) clearButton.onClick.AddListener(OnClearClicked);

        if (dropdownContainer != null)
        {
            var group = dropdownContainer.GetComponent<HorizontalLayoutGroup>();
            if (group != null)
            {
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = false;
                group.childForceExpandHeight = false;
            }
        }

        RefreshSlotsUI();
        RefreshCountButtons();
        ForceRebuildLayout();
    }

    private void EnsureClearButton()
    {
        if (clearButton != null) return;

        Transform buttonsRoot = transform.Find("SelectorButtons");
        if (buttonsRoot == null && applyButton != null)
            buttonsRoot = applyButton.transform.parent;
        if (buttonsRoot == null) return;

        var existing = buttonsRoot.Find("ClearBtn");
        if (existing != null)
        {
            clearButton = existing.GetComponent<Button>();
            return;
        }

        var go = new GameObject("ClearBtn");
        go.transform.SetParent(buttonsRoot, false);
        go.layer = 5;

        float width = 82f;
        float height = 48f * ItemHeightScale;
        if (applyButton != null)
        {
            var applyLE = applyButton.GetComponent<LayoutElement>();
            if (applyLE != null)
            {
                if (applyLE.preferredWidth > 0f) width = applyLE.preferredWidth;
                if (applyLE.preferredHeight > 0f) height = applyLE.preferredHeight;
            }
        }

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleHeight = 0f;

        var rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(width, height);

        go.AddComponent<Image>().color = new Color(0.40f, 0.26f, 0.10f, 1f);
        clearButton = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        textGO.layer = 5;
        var textRt = textGO.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Clear";
        tmp.fontSize = 13f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        if (applyButton != null && applyButton.transform.parent == buttonsRoot)
            go.transform.SetSiblingIndex(applyButton.transform.GetSiblingIndex() + 1);
    }

    // 외부(ChordProgressionPanel)에서 호출: 코드 재생목록에 순서대로 추가
    public bool QueueChordFromPanel(string chordName)
    {
        if (string.IsNullOrWhiteSpace(chordName)) return false;
        if (chordNames == null || chordNames.Count == 0) return false;
        if (!chordNames.Contains(chordName))
        {
            Debug.LogWarning($"[ChordSelector] 알 수 없는 코드 '{chordName}'");
            return false;
        }

        if (selectedNames.Count >= playlistCapacity)
        {
            Debug.LogWarning($"[ChordSelector] 재생목록이 가득 찼습니다. (+)로 슬롯 수를 늘리세요. ({selectedNames.Count}/{playlistCapacity})");
            return false;
        }

        selectedNames.Add(chordName);
        RefreshSlotsUI();
        ForceRebuildLayout();
        return true;
    }

    private void CreateEmptySlot()
    {
        int slotIndex = slotObjects.Count;

        var go = new GameObject($"Slot_{slotIndex}");
        go.transform.SetParent(dropdownContainer, false);
        go.layer = 5;

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 70f;
        le.preferredWidth = 70f;
        le.minHeight = 48f * ItemHeightScale;
        le.preferredHeight = 48f * ItemHeightScale;
        le.flexibleHeight = 0f;

        var rtSlot = go.GetComponent<RectTransform>();
        if (rtSlot != null)
            rtSlot.sizeDelta = new Vector2(70f, 48f * ItemHeightScale);

        go.AddComponent<Image>().color = new Color(0.08f, 0.18f, 0.28f, 1f);
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.14f, 0.30f, 0.46f);
        colors.pressedColor = new Color(0.04f, 0.10f, 0.20f);
        btn.colors = colors;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        labelGO.layer = 5;
        var lrt = labelGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 17f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        slotObjects.Add(go);
        slotLabels.Add(tmp);

        // 슬롯 클릭 시 해당 위치의 코드 제거 (뒤 항목이 앞으로 당겨짐)
        int captured = slotIndex;
        btn.onClick.AddListener(() => RemoveAt(captured));
    }

    private void RemoveAt(int index)
    {
        if (index < 0 || index >= selectedNames.Count) return;
        selectedNames.RemoveAt(index);
        RefreshSlotsUI();
        ForceRebuildLayout();
    }

    private void OnAddClicked()
    {
        if (playlistCapacity >= MaxCapacity) return;

        playlistCapacity++;
        CreateEmptySlot();
        RefreshSlotsUI();
        RefreshCountButtons();
        ForceRebuildLayout();
    }

    private void OnRemoveClicked()
    {
        if (playlistCapacity <= MinCapacity) return;

        playlistCapacity--;

        int last = slotObjects.Count - 1;
        if (last >= 0)
        {
            Destroy(slotObjects[last]);
            slotObjects.RemoveAt(last);
            slotLabels.RemoveAt(last);
        }

        if (selectedNames.Count > playlistCapacity)
        {
            selectedNames.RemoveRange(playlistCapacity, selectedNames.Count - playlistCapacity);
        }

        RefreshSlotsUI();
        RefreshCountButtons();
        ForceRebuildLayout();
    }

    private void RefreshSlotsUI()
    {
        for (int i = 0; i < slotLabels.Count; i++)
        {
            var label = slotLabels[i];
            if (label == null) continue;

            if (i < selectedNames.Count)
            {
                label.text = selectedNames[i];
                label.color = new Color(0f, 0.9f, 0.9f);
            }
            else
            {
                label.text = "-";
                label.color = new Color(0.58f, 0.65f, 0.72f);
            }
        }
    }

    private void RefreshCountButtons()
    {
        if (addButton) addButton.interactable = playlistCapacity < MaxCapacity;
        if (removeButton) removeButton.interactable = playlistCapacity > MinCapacity;
    }

    private void OnApplyClicked()
    {
        var selected = GetSelectedChords();
        if (selected.Count == 0)
        {
            Debug.LogWarning("[ChordSelector] 재생목록이 비어 있습니다. 상단 코드 버튼을 눌러 추가하세요.");
            return;
        }

        Debug.Log($"[ChordSelector] Apply → {string.Join(" → ", selected)}");
        sessionController.StopSession();
        progressionPanel.SetProgression(selected);
        sessionController.StartCustomSession(selected, bpmControl.GetCurrentBPM());
    }

    private void OnClearClicked()
    {
        selectedNames.Clear();
        RefreshSlotsUI();
        ForceRebuildLayout();
    }

    public List<string> GetSelectedChords() => new List<string>(selectedNames);

    private void ForceRebuildLayout()
    {
        if (dropdownContainer == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownContainer);

        var parent = dropdownContainer.parent as RectTransform;
        if (parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
    }
}
