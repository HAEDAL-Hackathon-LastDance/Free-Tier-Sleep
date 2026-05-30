using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Phase 1 클리어 시 화면 동결 후 표시할 종료 팝업
// - 씬 Canvas에 미리 배치된 UI 계층을 SetActive로 토글 (BuildPopup 없음)
// - Inspector에서 popupRoot 하위 오브젝트를 자유롭게 크기/위치 조정 가능
// - Space / Enter 또는 확인 버튼 클릭으로 다음 씬 전환
public class GameOverFreezePopup : MonoBehaviour
{
    public static GameOverFreezePopup Instance { get; private set; }

    [Header("팝업 루트 (SetActive 토글 대상)")]
    [SerializeField] private GameObject popupRoot;

    [Header("애니메이션 참조")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private RectTransform borderTransform;

    [Header("런타임 갱신 텍스트")]
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("씬 전환")]
    [Tooltip("다음으로 이동할 씬 이름 (비워두면 Build Index +1)")]
    [SerializeField] private string nextSceneName;
    [Tooltip("Inspector에서 연결 — 자동 OnClick 등록")]
    [SerializeField] private Button confirmButton;

    private bool isShowing;
    private bool isAnimating;
    private Vector3 targetScale;

    private void Awake()
    {
        Instance = this;
        // Inspector에서 설정한 스케일을 목표값으로 저장
        if (borderTransform != null)
            targetScale = borderTransform.localScale;
        else
            targetScale = Vector3.one;

        if (popupRoot != null) popupRoot.SetActive(false);
    }

    private void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void Update()
    {
        if (!isShowing || isAnimating) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ProceedToNextScene();
    }

    // ─────────────────────────────────────────
    // 외부 진입점 (Phase1ClearSequence 호출)
    // ─────────────────────────────────────────
    public void Show()
    {
        if (popupRoot == null) return;

        // 본문 고도 값 갱신
        if (contentText != null)
        {
            float alt = GetTargetAltitude();
            contentText.text =
                "<color=#00FF41>접속 완료율</color>  :  <color=#FFFFFF>100.00 %</color>\n" +
                $"<color=#00FF41>구역 좌표  </color>  :  <color=#FFFFFF>Y = {alt:F1}</color>\n" +
                "<color=#00FF41>암호화 해제</color>  :  ██████  <color=#FFFFFF>[완료]</color>\n\n" +
                "<color=#888888>경계 통과 경로가 확인되었습니다.\n이동 준비가 완료되었습니다.</color>";
        }

        // 애니메이션 시작 상태 리셋
        if (overlayImage != null)
            overlayImage.color = new Color(0f, 0f, 0f, 0f);
        if (borderTransform != null)
            borderTransform.localScale = targetScale * 0.8f;

        isShowing = false;
        isAnimating = true;
        popupRoot.SetActive(true);
        StartCoroutine(AnimateIn());
    }

    public void Hide()
    {
        isShowing = false;
        if (popupRoot != null) popupRoot.SetActive(false);
    }

    // Inspector의 Button OnClick에서 호출하거나 Start()에서 자동 등록
    public void OnConfirmClicked()
    {
        if (!isShowing || isAnimating) return;
        ProceedToNextScene();
    }

    private void ProceedToNextScene()
    {
        isShowing = false;
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ─────────────────────────────────────────
    // 등장 애니메이션 (timeScale=0에서도 동작)
    // ─────────────────────────────────────────
    private IEnumerator AnimateIn()
    {
        // 오버레이 페이드인 0 → 0.65
        if (overlayImage != null)
        {
            float t = 0f;
            const float fadeDur = 0.3f;
            while (t < fadeDur)
            {
                t += Time.unscaledDeltaTime;
                overlayImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / fadeDur) * 0.65f);
                yield return null;
            }
            overlayImage.color = new Color(0f, 0f, 0f, 0.65f);
        }

        if (borderTransform != null)
        {
            // 창 팝인 0.8 → 1.05 (Inspector 스케일 기준)
            float t = 0f;
            const float popDur = 0.25f;
            while (t < popDur)
            {
                t += Time.unscaledDeltaTime;
                borderTransform.localScale = Vector3.Lerp(targetScale * 0.8f, targetScale * 1.05f, t / popDur);
                yield return null;
            }

            // 탄성 복귀 1.05 → 1.0
            t = 0f;
            const float bounceDur = 0.1f;
            while (t < bounceDur)
            {
                t += Time.unscaledDeltaTime;
                borderTransform.localScale = Vector3.Lerp(targetScale * 1.05f, targetScale, t / bounceDur);
                yield return null;
            }
            borderTransform.localScale = targetScale;
        }

        isAnimating = false;
        isShowing = true;
    }

    private float GetTargetAltitude()
    {
        LevelGenerator gen = Object.FindFirstObjectByType<LevelGenerator>();
        return gen != null ? gen.targetAltitude : 1000f;
    }
}
