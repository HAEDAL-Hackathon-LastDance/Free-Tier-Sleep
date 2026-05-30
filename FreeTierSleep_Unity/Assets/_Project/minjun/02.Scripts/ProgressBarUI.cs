using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 진행도 UI — 화면 상단에 '다운로드 진행률' 슬라이더를 표시
// - 플레이어 고도 × progressSpeedMultiplier 배율로 0~99% 선형 증가
// - 99.0% → 99.1%: crawlSpeed로 크롤
// - 99.1% → 99.9%: fineCrawlSpeed로 크롤 (별도 조정 가능)
// - 99.9% 도달 → 잠깐 멈춤 → 99.91%부터 2자리 소수 표시로 100.00%까지 크롤
public class ProgressBarUI : MonoBehaviour
{
    [Header("진행도 설정")]
    [Tooltip("LevelGenerator.targetAltitude와 동일하게 맞출 것")]
    public float targetAltitude = 1000f;

    [Tooltip("0→99% 구간 진행 배율 (기본 3 = 목표 고도 1/3 지점에서 99%에 도달)")]
    public float progressSpeedMultiplier = 3f;

    [Header("크롤 속도")]
    [Tooltip("99.0% → 99.1% 구간 크롤 속도 (%/초)")]
    public float crawlSpeed = 0.12f;

    [Tooltip("99.1% → 100.00% 구간 크롤 속도 (%/초) — 99.9% 이후 2자리 소수 단계도 이 속도 사용")]
    public float fineCrawlSpeed = 0.06f;

    [Tooltip("99.0%에 처음 도달했을 때 멈추는 시간 (초)")]
    public float pauseAt99Duration = 1.5f;

    [Tooltip("99.9%에 도달했을 때 멈추는 시간 (초)")]
    public float pauseAt999Duration = 1.2f;

    [Header("폰트")]
    [Tooltip("한글 지원 TMP 폰트 에셋 (둥근모꼴 SDF 등). 비워두면 TMP 기본 폰트 사용")]
    [SerializeField] private TMP_FontAsset koreanFont;

    [Header("색상")]
    public Color normalFillColor = new Color(0.2f, 0.85f, 0.4f, 1f);
    public Color crawlFillColor  = new Color(1f, 0.65f, 0.1f, 1f);
    public Color panelBgColor    = new Color(0.04f, 0.04f, 0.08f, 0.9f);

    private PlayerController player;
    private Slider   progressSlider;
    private Image    fillImage;
    private TMP_Text percentText;
    private TMP_Text statusText;

    private float displayPct = 0f;
    private float crawlPct   = 99f;
    private bool  inCrawl    = false;

    private static readonly string[] crawlMessages =
    {
        "ERROR DETECTED...",
        "REASSEMBLING PACKETS...",
        "RECOVERING DATA...",
        "RETRYING (1/99)...",
        "DEFRAGMENTING...",
        "CONNECTION UNSTABLE...",
    };

    private void Start()
    {
        player = Object.FindFirstObjectByType<PlayerController>();
        BuildUI();
    }

    private void Update()
    {
        if (player == null) return;

        if (!inCrawl)
        {
            // progressSpeedMultiplier 배율 적용 — 더 빠르게 99%에 도달
            float linearPct = Mathf.Clamp01(
                player.transform.position.y / targetAltitude * progressSpeedMultiplier
            ) * 100f;

            if (linearPct >= 99f)
            {
                inCrawl    = true;
                displayPct = 99f;
                crawlPct   = 99f;
                StartCoroutine(CrawlRoutine());
            }
            else
            {
                displayPct = linearPct;
            }
        }

        progressSlider.value = displayPct / 100f;

        // 99.91% 이상: 2자리 소수 / 99% 이상: 1자리 소수 / 미만: 정수
        if (displayPct >= 99.905f)
            percentText.text = displayPct.ToString("F2") + "%";
        else if (displayPct >= 99f)
            percentText.text = displayPct.ToString("F1") + "%";
        else
            percentText.text = Mathf.FloorToInt(displayPct) + "%";
    }

    private IEnumerator CrawlRoutine()
    {
        fillImage.color = crawlFillColor;
        statusText.text = "ERROR DETECTED...";

        // ── 단계 1: 99.0% 정지 후 → 99.1% (crawlSpeed) ──
        yield return new WaitForSeconds(pauseAt99Duration);

        while (crawlPct < 99.1f)
        {
            crawlPct  += crawlSpeed * Time.deltaTime;
            crawlPct   = Mathf.Min(crawlPct, 99.1f);
            displayPct = crawlPct;
            statusText.text = "REASSEMBLING PACKETS...";
            yield return null;
        }
        crawlPct = displayPct = 99.1f;

        // ── 단계 2: 99.1% → 99.9% (fineCrawlSpeed) ──
        int prevStep = -1;
        while (crawlPct < 99.9f)
        {
            crawlPct  += fineCrawlSpeed * Time.deltaTime;
            crawlPct   = Mathf.Min(crawlPct, 99.9f);
            displayPct = crawlPct;

            // 0.2% 단계마다 상태 메시지 순환
            int step = Mathf.FloorToInt((crawlPct - 99.1f) / 0.2f);
            if (step != prevStep)
            {
                prevStep = step;
                statusText.text = crawlMessages[(step + 2) % crawlMessages.Length];
            }

            yield return null;
        }
        crawlPct = displayPct = 99.9f;

        // ── 99.9% 도달: 잠깐 멈추고 2자리 소수 단계로 전환 ──
        statusText.text = "RETRYING (1/99)...";
        yield return new WaitForSeconds(pauseAt999Duration);

        // ── 단계 3: 99.91% → 100.00% (fineCrawlSpeed, 2자리 소수) ──
        crawlPct = displayPct = 99.91f; // 99.9%가 됐을 때 다시 99.91%부터
        prevStep = -1;

        while (crawlPct < 100f)
        {
            crawlPct  += fineCrawlSpeed * Time.deltaTime;
            crawlPct   = Mathf.Min(crawlPct, 100f);
            displayPct = crawlPct;

            int step = Mathf.FloorToInt((crawlPct - 99.91f) / 0.02f);
            if (step != prevStep)
            {
                prevStep = step;
                statusText.text = crawlMessages[step % crawlMessages.Length];
            }

            yield return null;
        }

        displayPct      = 100f;
        percentText.text = "100.00%";
        statusText.text  = "DOWNLOAD COMPLETE";
        StartCoroutine(FlashFill());
        Phase1ClearSequence.Instance?.TriggerClear();
    }

    // 100% 달성 순간 채움 바를 흰색으로 번쩍인 후 서서히 초록으로 복귀
    private IEnumerator FlashFill()
    {
        // 흰색 유지 0.4초
        fillImage.color = Color.white;
        yield return new WaitForSecondsRealtime(0.4f);

        // 흰색 → 초록 0.5초 동안 lerp 페이드
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fillImage.color = Color.Lerp(Color.white, normalFillColor, elapsed / duration);
            yield return null;
        }
        fillImage.color = normalFillColor;
    }

    // Phase1ClearSequence 가 상태 텍스트를 교체할 때 사용
    public void SetClearStatusText(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    private void BuildUI()
    {
        // koreanFont(둥근모꼴 SDF)가 Inspector에서 할당되면 우선 사용, 없으면 기본 폰트 폴백
        TMP_FontAsset defaultFont = koreanFont != null ? koreanFont : TMP_Settings.defaultFontAsset;

        // Canvas는 씬 계층에 UICanvas로 별도 배치 — 여기서는 UI 요소만 생성
        // ─── 패널 (화면 상단 고정) ───
        var panelGO  = new GameObject("Panel");
        panelGO.transform.SetParent(transform, false);

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = panelBgColor;

        var panelRT          = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin    = new Vector2(0f, 1f);
        panelRT.anchorMax    = new Vector2(1f, 1f);
        panelRT.pivot        = new Vector2(0.5f, 1f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta    = new Vector2(0f, 78f);

        // ─── 상태 텍스트 ───
        var statusGO  = new GameObject("StatusText");
        statusGO.transform.SetParent(panelGO.transform, false);
        statusText = statusGO.AddComponent<TextMeshProUGUI>();
        statusText.font      = defaultFont;
        statusText.text      = "DOWNLOADING...";
        statusText.fontSize   = 18f;
        statusText.color      = new Color(0.55f, 1f, 0.55f, 1f);
        statusText.alignment  = TextAlignmentOptions.Left;
        statusText.fontStyle  = FontStyles.Bold;

        var statusRT          = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin    = new Vector2(0f, 1f);
        statusRT.anchorMax    = new Vector2(0.75f, 1f);
        statusRT.pivot        = new Vector2(0f, 1f);
        statusRT.anchoredPosition = new Vector2(14f, -6f);
        statusRT.sizeDelta    = new Vector2(0f, 24f);

        // ─── 슬라이더 ───
        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(panelGO.transform, false);

        progressSlider             = sliderGO.AddComponent<Slider>();
        progressSlider.minValue    = 0f;
        progressSlider.maxValue    = 1f;
        progressSlider.value       = 0f;
        progressSlider.interactable = false;
        progressSlider.transition  = Selectable.Transition.None;
        progressSlider.direction   = Slider.Direction.LeftToRight;

        var sliderRT    = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0f, 0f);
        sliderRT.anchorMax = new Vector2(1f, 1f);
        sliderRT.offsetMin = new Vector2(14f, 8f);
        sliderRT.offsetMax = new Vector2(-14f, -30f);

        // 슬라이더 배경
        var bgGO   = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImg  = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.14f, 1f);
        var bgRT        = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin  = Vector2.zero;
        bgRT.anchorMax  = Vector2.one;
        bgRT.offsetMin  = Vector2.zero;
        bgRT.offsetMax  = Vector2.zero;
        progressSlider.targetGraphic = bgImg;

        // Fill Area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var faRT       = fillAreaGO.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.offsetMin = new Vector2(3f, 3f);
        faRT.offsetMax = new Vector2(-3f, -3f);

        // Fill
        var fillGO   = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        fillImage    = fillGO.AddComponent<Image>();
        fillImage.color = normalFillColor;
        var fillRT       = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.sizeDelta = Vector2.zero;
        progressSlider.fillRect = fillRT;

        // ─── 퍼센트 텍스트 (슬라이더 중앙 오버레이) ───
        var pctGO   = new GameObject("PercentText");
        pctGO.transform.SetParent(sliderGO.transform, false);
        percentText = pctGO.AddComponent<TextMeshProUGUI>();
        percentText.font      = defaultFont;
        percentText.text      = "0%";
        percentText.fontSize   = 22f;
        percentText.fontStyle  = FontStyles.Bold;
        percentText.color      = Color.white;
        percentText.alignment  = TextAlignmentOptions.Center;

        var pctRT      = pctGO.GetComponent<RectTransform>();
        pctRT.anchorMin = Vector2.zero;
        pctRT.anchorMax = Vector2.one;
        pctRT.offsetMin = Vector2.zero;
        pctRT.offsetMax = Vector2.zero;
    }
}
