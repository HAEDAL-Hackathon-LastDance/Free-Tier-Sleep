using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Taehui
{
    /// <summary>
    /// 게임 시작 화면(메인 메뉴)의 흐름과 시네마틱 부팅 연출을 관리하는 컨트롤러
    /// </summary>
    public class StartSceneController : MonoBehaviour
    {
        [Header("System Panels")]
        [SerializeField] private GameObject menuPanel; // 메인 메뉴 버튼들이 있는 패널
        [SerializeField] private GameObject optionsPopup; // 설정창 팝업 게임오브젝트

        [Header("Cinematic Boot Effects")]
        [SerializeField] private TypingEffect bootTypingEffect; // 부팅 메시지 출력용 타이핑 컴포넌트
        [SerializeField] private CanvasGroup blackOverlay; // 씬 전환 페이드아웃용 오버레이
        [SerializeField] private Volume postProcessVolume; // URP 볼륨 글리치 효과용

        [Header("Loop Mode Elements")]
        [SerializeField] private TextMeshProUGUI titleText; // 타이틀 텍스트 참조
        [SerializeField] private TextMeshProUGUI accessButtonText; // 수면 접속 버튼 텍스트
        [SerializeField] private TextMeshProUGUI configButtonText; // 환경 설정 버튼 텍스트
        [SerializeField] private TextMeshProUGUI disconnectButtonText; // 연결 종료 버튼 텍스트

        [Header("Transition Settings")]
        [SerializeField] private string nextSceneName = "Scene_Intro"; // 전환할 씬 명칭
        [SerializeField] private float transitionDelay = 2.0f; // 글리치 전환 지속 시간

        private bool isTransitioning = false;

        private void Start()
        {
            // 초기 UI 상태 조율
            if (menuPanel != null) menuPanel.SetActive(false);
            if (optionsPopup != null) optionsPopup.SetActive(false);
            if (blackOverlay != null) blackOverlay.alpha = 0f;

            // [보정] 인스펙터에서 미할당 시 이름으로 자동 탐색 시도
            if (titleText == null)
            {
                GameObject go = GameObject.Find("TitleText");
                if (go != null) titleText = go.GetComponent<TextMeshProUGUI>();
            }

            // 엔딩을 조회한 루프 상태일 경우 메타 테마 적용
            if (PlayerPrefs.GetInt("EndingViewed", 0) == 1)
            {
                ApplyLoopTheme();
            }

            StartCoroutine(BootSequence());
        }

        /// <summary>
        /// 엔딩 이후 활성화되는 AI 에이전트 루프 메타 테마 적용
        /// </summary>
        private void ApplyLoopTheme()
        {
            if (titleText != null)
            {
                // 누적 루프 카운트를 불러와 타이틀에 동적으로 반영 (기본 892 + 클리어 횟수)
                int clears = PlayerPrefs.GetInt("TrueEndingLoopCount", 0);
                int currentLoopNumber = 892 + clears;

                titleText.text = $"[AI 에이전트 스트레스 테스트: {currentLoopNumber}번째 루프]";
                titleText.fontSize = 64; // 긴 텍스트 크기 조절로 줄바꿈 방지
                
                // 경고용 오렌지/적색 그라데이션 적용
                titleText.enableVertexGradient = true;
                titleText.colorGradient = new VertexGradient(
                    new Color(0.95f, 0.25f, 0.1f, 1f), // 상단 오렌지적색
                    new Color(0.95f, 0.25f, 0.1f, 1f),
                    new Color(0.6f, 0.1f, 0.05f, 1f),  // 하단 짙은 적색
                    new Color(0.6f, 0.1f, 0.05f, 1f)
                );

                // 누적 루프 카운트를 불러와 타이틀 밑에 경고 메타 텍스트를 동적으로 삽입 (사용자 요청으로 제거됨)

                StartCoroutine(FlickerTitleLoop());
            }

            // 한글 버튼 용어 갱신
            if (accessButtonText != null) accessButtonText.text = "[시뮬레이션 재시작]";
            if (configButtonText != null) configButtonText.text = "[설정 정보 분석]";
            if (disconnectButtonText != null) disconnectButtonText.text = "[프로세스 강제 킬]";
        }

        /// <summary>
        /// 루프 타이틀 텍스트 지직거림(점멸) 효과 코루틴
        /// </summary>
        private IEnumerator FlickerTitleLoop()
        {
            if (titleText == null) yield break;

            var originalGradient = titleText.colorGradient;
            var glitchedGradient = new VertexGradient(
                new Color(0.95f, 0.25f, 0.1f, 0.25f), // 희미한 알파값
                new Color(0.95f, 0.25f, 0.1f, 0.25f),
                new Color(0.6f, 0.1f, 0.05f, 0.25f),
                new Color(0.6f, 0.1f, 0.05f, 0.25f)
            );

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
                
                // 15%의 확률로 짧은 렉 유발 지직거림
                if (Random.value < 0.15f)
                {
                    float glitchTime = Random.Range(0.06f, 0.18f);
                    titleText.colorGradient = glitchedGradient;
                    yield return new WaitForSeconds(glitchTime);
                    titleText.colorGradient = originalGradient;
                }
            }
        }

        /// <summary>
        /// 단말기 부팅 가상 프롬프트 연출
        /// </summary>
        private IEnumerator BootSequence()
        {
            if (bootTypingEffect == null)
            {
                if (menuPanel != null) menuPanel.SetActive(true);
                yield break;
            }

            // 1차 부팅 시도 메시지
            bootTypingEffect.Play("[시스템] 뇌파 클라우드 동기화 시도 중...");
            yield return new WaitForSeconds(2.5f);

            // 2차 성공 및 로딩 완료 메시지 (누적 덧붙임 활용)
            bootTypingEffect.Play("[시스템] 뇌파 클라우드 동기화 완료.\n계정 상태: 무료 광고 모드 (AD-SUPPORTED SLEEP) 적용 중.", null, true);
            yield return new WaitForSeconds(3.0f);

            bootTypingEffect.Play("\n안내: 광고 의무 시청 시간이 미달될 경우 강제 기상 조치됩니다.\n수면 상태를 유지하려면 '수면 접속'을 클릭하십시오.", null, true);
            yield return new WaitForSeconds(2.0f);

            // 부팅 연출이 끝나면 비로소 메뉴 버튼 패널 활성화
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 게임 시작 버튼 이벤트
        /// </summary>
        public void AccessCloud()
        {
            if (isTransitioning) return;
            StartCoroutine(TransitionSequence());
        }

        /// <summary>
        /// 설정 버튼 이벤트
        /// </summary>
        public void ShowConfig()
        {
            if (isTransitioning) return;
            if (optionsPopup != null)
            {
                optionsPopup.SetActive(true);
            }
        }

        /// <summary>
        /// 설정창 닫기 버튼 이벤트
        /// </summary>
        public void CloseConfig()
        {
            if (optionsPopup != null)
            {
                optionsPopup.SetActive(false);
            }
        }

        /// <summary>
        /// 종료 버튼 이벤트
        /// </summary>
        public void Disconnect()
        {
            if (isTransitioning) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Application.OpenURL("about:blank");
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 인트로 씬으로 글리치 및 오디오 노이즈를 재생하며 자연스럽게 전환
        /// </summary>
        private IEnumerator TransitionSequence()
        {
            isTransitioning = true;

            ChromaticAberration chromatic = null;
            LensDistortion lens = null;

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out chromatic);
                postProcessVolume.profile.TryGet(out lens);
            }

            // 고음 글리치 SFX 재생
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.35f;

            AudioClip transitionSFX = ProceduralAudioHelper.CreateGlitchSound(transitionDelay);
            if (transitionSFX != null)
            {
                audioSource.PlayOneShot(transitionSFX);
            }

            float duration = transitionDelay;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // URP 볼륨의 색수차/왜곡 가중치 피크
                if (chromatic != null)
                {
                    chromatic.intensity.value = Mathf.Lerp(0f, 1f, progress);
                }
                if (lens != null)
                {
                    lens.intensity.value = Mathf.Lerp(0f, -0.6f, progress);
                    lens.scale.value = Mathf.Lerp(1f, 1.25f, progress);
                }

                // 페이드오버레이 불투명도 증가
                if (blackOverlay != null)
                {
                    blackOverlay.alpha = progress;
                }

                yield return null;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
