using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Phase 1 사망 시 화면 동결 후 표시할 게임오버 팝업
// - PlayerController.Die()에서 glitchDuration 후 Show() 호출
// - R키 또는 재시도 버튼 → 씬 리로드, 종료 버튼 → 메인 메뉴
public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [Header("팝업 루트 (SetActive 토글 대상)")]
    [SerializeField] private GameObject popupRoot;

    [Header("애니메이션 참조")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private RectTransform borderTransform;

    [Header("런타임 갱신 텍스트")]
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("씬 전환")]
    [Tooltip("메인 메뉴 씬 이름")]
    [SerializeField] private string mainMenuSceneName = "Scene_Intro";
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private bool isShowing;
    private bool isAnimating;
    private Vector3 targetScale;

    private void Awake()
    {
        Instance = this;
        if (borderTransform != null)
            targetScale = borderTransform.localScale;
        else
            targetScale = Vector3.one;

        if (popupRoot != null) popupRoot.SetActive(false);
    }

    private void Start()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Update()
    {
        if (!isShowing || isAnimating) return;
        if (Input.GetKeyDown(KeyCode.R))
            RetryScene();
    }

    public void Show(float survivalAltitude)
    {
        if (popupRoot == null) return;

        if (contentText != null)
        {
            contentText.text =
                "<color=#FF4444>오류 코드   </color>  :  <color=#FFFFFF>ERR_0xDEAD</color>\n" +
                $"<color=#FF4444>생존 고도   </color>  :  <color=#FFFFFF>Y = {survivalAltitude:F1}</color>\n" +
                "<color=#FF4444>루프 횟수   </color>  :  <color=#FFFFFF>#893</color>\n\n" +
                "<color=#888888>데이터 홍수에 의해 프로세스가\n종료되었습니다. 재시도합니까?</color>";
        }

        if (overlayImage != null)
            overlayImage.color = new Color(0f, 0f, 0f, 0f);
        if (borderTransform != null)
            borderTransform.localScale = targetScale * 0.8f;

        isShowing = false;
        isAnimating = true;
        popupRoot.SetActive(true);
        StartCoroutine(AnimateIn());
    }

    public void OnRetryClicked()
    {
        if (!isShowing || isAnimating) return;
        RetryScene();
    }

    public void OnQuitClicked()
    {
        if (!isShowing || isAnimating) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RetryScene()
    {
        isShowing = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator AnimateIn()
    {
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
            float t = 0f;
            const float popDur = 0.25f;
            while (t < popDur)
            {
                t += Time.unscaledDeltaTime;
                borderTransform.localScale = Vector3.Lerp(targetScale * 0.8f, targetScale * 1.05f, t / popDur);
                yield return null;
            }

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
}
