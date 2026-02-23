// ChordProgressionPanel.cs - 코드 진행 슬롯 패널 (10개 전체 상시 표시, 세션 하이라이트 지원)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChordProgressionPanel : MonoBehaviour
{
    [SerializeField] private GameObject chordSlotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private FretboardRenderer fretboardRenderer;
    [SerializeField] private ChordDatabase chordDatabase;
    [SerializeField] private ChordSelector chordSelector;

    [Header("색상 - 메이저")]
    [SerializeField] private Color majorNormalColor = new Color(0.12f, 0.2f, 0.38f, 1f);
    [SerializeField] private Color majorActiveColor = new Color(0.1f, 0.5f, 0.95f, 1f);
    [SerializeField] private Color majorSessionColor = new Color(0.08f, 0.32f, 0.58f, 1f);

    [Header("색상 - 마이너")]
    [SerializeField] private Color minorNormalColor = new Color(0.28f, 0.1f, 0.38f, 1f);
    [SerializeField] private Color minorActiveColor = new Color(0.65f, 0.1f, 0.85f, 1f);
    [SerializeField] private Color minorSessionColor = new Color(0.42f, 0.08f, 0.55f, 1f);

    [Header("텍스트 색상")]
    [SerializeField] private Color normalTextColor = new Color(0.72f, 0.72f, 0.72f, 1f);
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color sessionTextColor = new Color(0.88f, 0.88f, 1.0f, 1f);

    // 중요도 순서로 고정 — ChordDatabase 등록 순서를 따름
    // (행 배치는 GridLayoutGroup에 위임: 5열×2행)
    private static readonly string[] CanonicalOrder =
    {
        "C", "G", "Am", "F", "D",
        "A", "E", "Dm", "Em", "Fmaj7"
    };

    private readonly List<GameObject> slots = new List<GameObject>();
    private readonly List<string> chordList = new List<string>();

    // 현재 세션에 포함된 코드 이름 집합
    private readonly HashSet<string> sessionChords = new HashSet<string>();

    private int activeIndex = -1;
    private string activeChordName = null;

    // ── 전체 코드 초기화 (세션 시작 전에도 호출) ─────────────────────────
    /// <summary>
    /// ChordDatabase의 모든 코드를 CanonicalOrder 순서로 슬롯에 표시한다.
    /// 씬 Start() 시점에 한 번 호출하면 이후 슬롯을 재생성하지 않는다.
    /// </summary>
    public void InitAllChords(ChordDatabase db)
    {
        if (db == null)
        {
            Debug.LogError("[ChordProgressionPanel] InitAllChords: ChordDatabase가 null입니다.");
            return;
        }

        // 기존 슬롯 제거
        foreach (var s in slots) Destroy(s);
        slots.Clear();
        chordList.Clear();
        sessionChords.Clear();
        activeIndex = -1;
        activeChordName = null;

        // CanonicalOrder 기준으로 DB에 존재하는 코드만 순서대로 추가
        foreach (string canonicalName in CanonicalOrder)
        {
            ChordData found = db.GetChord(canonicalName);
            if (found == null) continue; // DB에 없으면 건너뜀

            chordList.Add(canonicalName);
            GameObject slot = Instantiate(chordSlotPrefab, slotsParent);

            TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = canonicalName;
                label.color = normalTextColor;
            }

            Image bg = slot.GetComponent<Image>();
            if (bg != null)
                bg.color = IsMinorChord(canonicalName) ? minorNormalColor : majorNormalColor;

            // 클릭 → 운지 표시 + 재생목록에 추가
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                string captured = canonicalName;
                btn.onClick.AddListener(() => ShowChordFingering(captured));
            }

            slots.Add(slot);
        }

        Debug.Log($"[ChordProgressionPanel] InitAllChords 완료: {slots.Count}개 슬롯 생성");
    }

    // ── 세션 코드 하이라이트 설정 (슬롯 재생성 없음) ─────────────────────
    /// <summary>
    /// 세션에 포함된 코드 이름 목록을 받아 해당 슬롯을 세션 색상으로 표시한다.
    /// 슬롯은 재생성되지 않는다.
    /// </summary>
    public void SetSessionHighlights(List<string> chordNames)
    {
        sessionChords.Clear();
        foreach (string name in chordNames)
            sessionChords.Add(name);

        // 활성 하이라이트 초기화 후 세션 색상 적용
        activeIndex = -1;
        activeChordName = null;

        for (int i = 0; i < slots.Count; i++)
        {
            string name = i < chordList.Count ? chordList[i] : "";
            bool inSession = sessionChords.Contains(name);
            ApplySlotVisual(i, active: false, inSession: inSession);
        }
    }

    // ── 현재 재생 코드 하이라이트 (index 기반, 하위 호환) ────────────────
    /// <summary>세션 진행 배열의 인덱스로 활성 슬롯을 지정한다 (하위 호환).</summary>
    public void SetActiveChord(int index)
    {
        if (currentProgression == null || index < 0 || index >= currentProgression.Count) return;
        SetActiveChord(currentProgression[index]);
    }

    // ── 현재 재생 코드 하이라이트 (코드명 기반, 신규) ────────────────────
    /// <summary>코드 이름으로 활성 슬롯을 지정한다.</summary>
    public void SetActiveChord(string chordName)
    {
        // 이전 활성 슬롯 복원
        if (activeIndex >= 0 && activeIndex < slots.Count)
        {
            string prevName = activeIndex < chordList.Count ? chordList[activeIndex] : "";
            bool prevSession = sessionChords.Contains(prevName);
            ApplySlotVisual(activeIndex, active: false, inSession: prevSession);
        }

        // 새 활성 슬롯 탐색
        int newIndex = chordList.IndexOf(chordName);
        if (newIndex >= 0 && newIndex < slots.Count)
        {
            ApplySlotVisual(newIndex, active: true, inSession: sessionChords.Contains(chordName));
            activeIndex = newIndex;
            activeChordName = chordName;
        }
        else
        {
            activeIndex = -1;
            activeChordName = null;
        }
    }

    // ── 세션 진행 목록 캐시 (SetActiveChord(int) 하위 호환용) ────────────
    private List<string> currentProgression = null;

    /// <summary>
    /// 레거시 SetProgression 호환 레이어.
    /// StartSession 시 세션 코드 목록을 내부 캐시에만 저장하고
    /// 실제 슬롯 UI는 재생성하지 않는다.
    /// </summary>
    public void SetProgression(List<string> names)
    {
        currentProgression = new List<string>(names);
        SetSessionHighlights(names);
    }

    // ── 모든 슬롯 비활성화 ────────────────────────────────────────────────
    public void ResetAll()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            string name = i < chordList.Count ? chordList[i] : "";
            bool inSession = sessionChords.Contains(name);
            ApplySlotVisual(i, active: false, inSession: inSession);
        }
        activeIndex = -1;
        activeChordName = null;
    }

    // ── 내부: 슬롯 비주얼 적용 ───────────────────────────────────────────
    private void ApplySlotVisual(int index, bool active, bool inSession)
    {
        if (index < 0 || index >= slots.Count) return;

        GameObject slot = slots[index];
        string name = index < chordList.Count ? chordList[index] : "";
        bool minor = IsMinorChord(name);

        Image bg = slot.GetComponent<Image>();
        if (bg != null)
        {
            if (active)
                bg.color = minor ? minorActiveColor : majorActiveColor;
            else if (inSession)
                bg.color = minor ? minorSessionColor : majorSessionColor;
            else
                bg.color = minor ? minorNormalColor : majorNormalColor;
        }

        TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            if (active)
                label.color = activeTextColor;
            else if (inSession)
                label.color = sessionTextColor;
            else
                label.color = normalTextColor;
        }

        slot.transform.localScale = Vector3.one * (active ? 1.12f : 1.0f);
    }

    // ── 클릭 핸들러 ───────────────────────────────────────────────────────
    private void ShowChordFingering(string chordName)
    {
        if (fretboardRenderer != null && chordDatabase != null)
        {
            ChordData chord = chordDatabase.GetChord(chordName);
            if (chord != null) fretboardRenderer.HighlightChord(chord);
        }

        // 상단 코드 버튼 클릭 시 ChordSelector 재생목록에 순차 저장
        if (chordSelector == null)
            chordSelector = Object.FindFirstObjectByType<ChordSelector>();

        if (chordSelector != null)
            chordSelector.QueueChordFromPanel(chordName);
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────────────
    private static bool IsMinorChord(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        // "Am", "Dm", "Em" → true / "Fmaj7" → false
        return name.Length > 1
            && name.EndsWith("m")
            && !name.EndsWith("maj7");
    }
}
