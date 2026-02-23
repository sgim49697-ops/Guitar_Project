// FretMarker.cs - 지판 마커 시각 상태 머신 (Preview/Active/Transitioning/Inactive)
using UnityEngine;

public class FretMarker : MonoBehaviour
{
    // ── 상태 정의 ──────────────────────────────────────────────────────────
    public enum State { Inactive, Active, Preview, Transitioning, Settling }

    public State CurrentState => currentState;

    // ── 필드 ───────────────────────────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private FretboardConfig config;

    private State currentState = State.Inactive;
    private State targetState  = State.Inactive;   // Transitioning 완료 후 도달할 상태

    private int stringIndex;
    private int fretIndex;

    // Transitioning 용
    private Color  transitionFromColor;
    private Color  transitionToColor;
    private float  transitionDuration;
    private float  transitionElapsed;

    // Active 펄스 용
    private float glowPhase;

    // Settling 용 (시안 → 노란색 1.5초 전환)
    private float settleElapsed;
    private const float SettleDuration = 1.5f;

    // Preview 용
    private float previewPhase;
    private float previewFadeElapsed;
    private float previewFadeDuration;
    private bool  previewFadingIn;
    private Color previewFadeFrom;

    // ── 초기화 ─────────────────────────────────────────────────────────────
    public void Initialize(int stringIdx, int fretIdx, FretboardConfig cfg)
    {
        stringIndex = stringIdx;
        fretIndex   = fretIdx;
        config      = cfg;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = config.inactiveMarkerColor;
        transform.localScale = Vector3.one * config.markerSize;

        currentState = State.Inactive;
    }

    // ── 공개 API (기존 호환) ───────────────────────────────────────────────
    /// <summary>즉시 활성/비활성 전환. 기존 HighlightChord 흐름 호환.</summary>
    public void SetActive(bool active)
    {
        StopTransition();

        if (active)
        {
            currentState = State.Active;
            glowPhase    = 0f;
            spriteRenderer.color = config.activeMarkerColor;
            transform.localScale = Vector3.one * config.markerSize;
        }
        else
        {
            currentState = State.Inactive;
            spriteRenderer.color = config.inactiveMarkerColor;
            transform.localScale = Vector3.one * config.markerSize;
        }
    }

    /// <summary>
    /// 다음 코드 예고 (노란색 포함 모든 상태에서 dim cyan으로 페이드).
    /// 겹치는 위치의 노란색도 새 위치 dim cyan과 동시에 전환된다.
    /// </summary>
    public void SetPreview()
    {
        if (currentState == State.Preview) return;

        // Transitioning/Settling 중이면 즉시 확정 후 진입
        if (currentState == State.Transitioning || currentState == State.Settling)
            StopTransition();

        // Active 상태면 UpdateActivePulse가 매 프레임 yellow로 덮어쓰므로
        // previewFadeFrom은 glowColor(yellow)로 명시
        previewFadeFrom = (currentState == State.Active)
            ? config.glowColor
            : spriteRenderer.color;

        currentState        = State.Preview;
        previewPhase        = 0f;
        previewFadingIn     = true;
        previewFadeElapsed  = 0f;
        previewFadeDuration = 0.25f;
    }

    /// <summary>예고 해제. Preview 상태일 때만 Inactive로 페이드.</summary>
    public void ClearPreview()
    {
        if (currentState != State.Preview) return;

        // Preview → Inactive 단기 페이드
        StartTransition(
            from:     spriteRenderer.color,
            to:       config.inactiveMarkerColor,
            duration: 0.15f,
            next:     State.Inactive
        );
    }

    /// <summary>
    /// 크로스페이드로 Active 상태로 전환.
    /// Inactive 상태에서 진입 시 투명 시안에서 밝은 시안으로 페이드인.
    /// Preview 상태에서 진입 시 현재 dim 시안에서 이어서 페이드인.
    /// </summary>
    public void CrossfadeToActive(float duration)
    {
        // previewMarkerColor(dim cyan, alpha~0.45)에서 출발
        // - 겹치는 위치(노란색): 즉시 dim cyan으로 교체 → 밝은 시안으로 페이드인
        // - 새 위치(비활성/preview): 동일한 dim cyan 기준점에서 페이드인
        // → 모든 위치가 동일한 타이밍에 같은 밝기의 시안으로 점등 시작
        StartTransition(
            from:     config.previewMarkerColor,
            to:       config.activeMarkerColor,
            duration: Mathf.Max(duration, 0.05f),
            next:     State.Active
        );
    }

    /// <summary>
    /// 크로스페이드로 Inactive 상태로 전환.
    /// Active 상태에서 진입 시 노란색(glowColor)에서 흐려지는 효과.
    /// </summary>
    public void CrossfadeToInactive(float duration)
    {
        // Active → glowColor(노란색)에서 출발해 "불 꺼지듯" 흐려지는 효과
        Color fromColor = (currentState == State.Active)
            ? config.glowColor
            : spriteRenderer.color;

        StartTransition(
            from:     fromColor,
            to:       config.inactiveMarkerColor,
            duration: Mathf.Max(duration, 0.05f),
            next:     State.Inactive
        );
    }

    // ── Update ─────────────────────────────────────────────────────────────
    void Update()
    {
        switch (currentState)
        {
            case State.Inactive:
                // 고정 — 아무 처리 없음
                break;

            case State.Active:
                UpdateActivePulse();
                break;

            case State.Preview:
                UpdatePreview();
                break;

            case State.Transitioning:
                UpdateTransition();
                break;

            case State.Settling:
                UpdateSettling();
                break;
        }
    }

    // ── 상태별 업데이트 ────────────────────────────────────────────────────
    private void UpdateActivePulse()
    {
        // 색상: 노란색(glowColor) 고정 — 시안↔노란 진동 제거
        // 시안은 CrossfadeToActive 진입 시 fade-in 구간에서만 잠깐 보임
        spriteRenderer.color = config.glowColor;

        // 스케일: 1.0 → 1.05 미세 펄스 (색이 고정이므로 리듬감 유지)
        glowPhase += Time.deltaTime * 9f;
        float scalePulse = (Mathf.Sin(glowPhase * 0.5f) + 1f) * 0.5f;
        float scale = config.markerSize * Mathf.Lerp(1.0f, 1.05f, scalePulse);
        transform.localScale = Vector3.one * scale;
    }

    private void UpdatePreview()
    {
        // 페이드인 처리 (최초 0.25f)
        if (previewFadingIn)
        {
            previewFadeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(previewFadeElapsed / previewFadeDuration);
            spriteRenderer.color = Color.Lerp(previewFadeFrom, config.previewMarkerColor, SmoothStep(t));

            if (t >= 1f)
                previewFadingIn = false;

            return;
        }

        // 페이드인 완료 후: 느린 opacity 펄스 (alpha 70%~100%, 속도 1.5f)
        previewPhase += Time.deltaTime * 1.5f;
        float alphaPulse = (Mathf.Sin(previewPhase) + 1f) * 0.5f; // 0~1
        Color pc = config.previewMarkerColor;
        float alpha = Mathf.Lerp(pc.a * 0.7f, pc.a, alphaPulse);
        spriteRenderer.color = new Color(pc.r, pc.g, pc.b, alpha);
    }

    private void UpdateTransition()
    {
        transitionElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(transitionElapsed / transitionDuration);
        spriteRenderer.color = Color.Lerp(transitionFromColor, transitionToColor, SmoothStep(t));

        if (t >= 1f)
        {
            // 전환 완료 — targetState로 진입
            currentState = targetState;

            if (currentState == State.Active)
            {
                // 시안 fade-in 완료 → Settling(시안→노란 1.5초)으로 진입
                currentState  = State.Settling;
                settleElapsed = 0f;
                glowPhase     = 0f;
                spriteRenderer.color = config.activeMarkerColor; // 시안에서 출발
            }
            else if (currentState == State.Inactive)
            {
                transform.localScale = Vector3.one * config.markerSize;
            }
        }
    }

    // ── 내부 유틸 ──────────────────────────────────────────────────────────
    private void StartTransition(Color from, Color to, float duration, State next)
    {
        currentState          = State.Transitioning;
        targetState           = next;
        transitionFromColor   = from;
        transitionToColor     = to;
        transitionDuration    = duration;
        transitionElapsed     = 0f;
        spriteRenderer.color  = from;
    }

    private void UpdateSettling()
    {
        // 시안(activeMarkerColor) → 노란색(glowColor) 1.5초 부드러운 전환
        settleElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(settleElapsed / SettleDuration);
        spriteRenderer.color = Color.Lerp(config.activeMarkerColor, config.glowColor, SmoothStep(t));

        // 스케일 펄스는 Settling 중에도 유지
        glowPhase += Time.deltaTime * 9f;
        float scalePulse = (Mathf.Sin(glowPhase * 0.5f) + 1f) * 0.5f;
        transform.localScale = Vector3.one * config.markerSize * Mathf.Lerp(1.0f, 1.05f, scalePulse);

        if (t >= 1f)
        {
            spriteRenderer.color = config.glowColor;
            currentState = State.Active;
        }
    }

    private void StopTransition()
    {
        // 진행 중인 전환이 있으면 즉시 목적지 색상으로 확정
        if (currentState == State.Transitioning)
        {
            spriteRenderer.color = transitionToColor;
            currentState = targetState;
            // Active 목적지면 Settling으로 진입
            if (currentState == State.Active)
            {
                currentState  = State.Settling;
                settleElapsed = 0f;
                glowPhase     = 0f;
                spriteRenderer.color = config.activeMarkerColor;
            }
        }
        else if (currentState == State.Settling)
        {
            // Settling 중 강제 종료 시 즉시 노란색 확정
            spriteRenderer.color = config.glowColor;
            currentState = State.Active;
        }
    }

    private static float SmoothStep(float t) => t * t * (3f - 2f * t);
}
