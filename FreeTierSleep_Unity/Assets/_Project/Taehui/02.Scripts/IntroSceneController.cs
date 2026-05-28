using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Taehui
{
    /// <summary>
    /// 인트로 씬의 전체 흐름을 관리하는 컨트롤러
    /// </summary>
    public class IntroSceneController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TypingEffect typingEffect;
        [SerializeField] private AdPopupManager adPopupManager;
        [SerializeField] private CanvasGroup blackOverlay; // 페이드 아웃용
        [SerializeField] private Volume postProcessVolume; // URP 볼륨 효과 제어용

        [Header("Transition Settings")]
        [SerializeField] private string nextSceneName = "Phase1_Movement"; // 씬 이름 대응
        [SerializeField] private float transitionDelay = 3.0f;

        private void Start()
        {
            StartCoroutine(IntroSequence());
        }

        private IEnumerator IntroSequence()
        {
            // 1. 시스템 메시지 출력 (한 줄 띄움 적용)
            typingEffect.Play("[시스템 메시지] 요금제 갱신 실패.\n무료 광고 모드로 전환합니다.");
            yield return new WaitForSeconds(4.5f);

            // 2. 광고 팝업 폭발적 증가
            adPopupManager.StartSpawning();
            yield return new WaitForSeconds(4.0f);

            // 3. 주인공 독백 (한 줄 띄움 적용)
            typingEffect.Play("P: \"뇌를 갉아먹는 이 과잉 연결에서...\n벗어나야 한다.\"");
            yield return new WaitForSeconds(5.0f);
            
            typingEffect.Play("P: \"저 경계 너머로.\"");
            yield return new WaitForSeconds(3.5f);

            // 광고 생성 정지
            adPopupManager.StopSpawning();

            // 4. 글리치 및 전환 연출 (URP Post Processing & Glitch SFX)
            yield return StartCoroutine(GlitchTransition());

            // 5. 다음 씬 로드
            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator GlitchTransition()
        {
            ChromaticAberration chromatic = null;
            LensDistortion lens = null;

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out chromatic);
                postProcessVolume.profile.TryGet(out lens);
            }

            // 글리치 SFX 재생
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.35f;

            AudioClip glitchClip = ProceduralAudioHelper.CreateGlitchSound(transitionDelay);
            if (glitchClip != null)
            {
                audioSource.PlayOneShot(glitchClip);
            }

            float duration = transitionDelay;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // 포스트 프로세싱 왜곡 강도 스케일 업
                if (chromatic != null)
                {
                    chromatic.intensity.value = Mathf.Lerp(0f, 1f, progress);
                }
                if (lens != null)
                {
                    lens.intensity.value = Mathf.Lerp(0f, -0.7f, progress);
                    lens.scale.value = Mathf.Lerp(1f, 1.3f, progress);
                }

                // 페이드 검은화면도 점진적 적용
                if (blackOverlay != null)
                {
                    blackOverlay.alpha = progress;
                }

                yield return null;
            }
        }
    }
}
