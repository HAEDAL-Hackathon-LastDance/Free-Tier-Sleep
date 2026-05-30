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

            StartCoroutine(BootSequence());
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
        /// 종료 버튼 이벤트
        /// </summary>
        public void Disconnect()
        {
            if (isTransitioning) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
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
