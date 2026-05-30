using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Taehui
{
    /// <summary>
    /// Phase 1 클리어 시 로드되어 Phase 2(공허) 진입 전까지의 연출을 담당하는 과도기 컷씬 컨트롤러
    /// </summary>
    public class PhaseTransitionController : MonoBehaviour
    {
        [Header("Video Player Settings")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoRawImage; 

        [Header("UI Panels")]
        [SerializeField] private GameObject monologuePanel; 
        [SerializeField] private TextMeshProUGUI monologueText;
        [SerializeField] private GameObject errorPanel; 
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Transition Settings")]
        [SerializeField] private CanvasGroup blackOverlay; 
        [SerializeField] private Volume postProcessVolume; 
        [SerializeField] private string nextSceneName = "Scene_Phase2";

        private AudioSource audioSource;
        private bool videoFinished = false;

        private void Start()
        {
            if (monologuePanel != null) monologuePanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
            if (blackOverlay != null) blackOverlay.alpha = 0f;

            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.loopPointReached += OnVideoFinished;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            StartCoroutine(TransitionSequence());
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            videoFinished = true;
        }

        private IEnumerator TransitionSequence()
        {
            RenderTexture videoRT = null;
            if (videoPlayer != null && videoRawImage != null)
            {
                videoRT = new RenderTexture(1280, 720, 16, RenderTextureFormat.ARGB32);
                videoRT.Create();
                
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = videoRT;
                videoPlayer.skipOnDrop = true;
                videoPlayer.waitForFirstFrame = true; // 첫 프레임 렌더링 대기 활성화
                
                videoRawImage.texture = videoRT;
                videoRawImage.color = Color.white;
                videoRawImage.gameObject.SetActive(true);
            }

            if (videoPlayer != null)
            {
                // [안전] 이전 상태 초기화
                videoFinished = false;
                videoPlayer.Prepare();
                
                float prepTimeout = 8.0f;
                float prepElapsed = 0f;
                while (!videoPlayer.isPrepared && prepElapsed < prepTimeout)
                {
                    prepElapsed += Time.deltaTime;
                    yield return null;
                }
                
                // [수정] 재생 직전 프레임 강제 고정
                videoPlayer.frame = 0;
                videoPlayer.Play();
                
                // 영상이 실제로 재생을 시작할 때까지 아주 잠시 대기 (이벤트 오작동 방지)
                yield return new WaitForSeconds(0.5f);
                
                // [수정] 이벤트가 발생하거나, 재생 시간이 길이를 초과하거나, 재생이 멈췄을 때 종료
                // redundant check (이벤트가 가끔 안 들어오는 유니티 고유 버그 대비)
                while (!videoFinished && videoPlayer.isPlaying)
                {
                    // 현재 재생 시간이 전체 길이의 98%를 넘었다면 종료로 간주 (안전 장치)
                    if (videoPlayer.length > 0 && videoPlayer.time >= videoPlayer.length - 0.1f)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            // 비디오 정리
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
            if (videoRawImage != null) videoRawImage.gameObject.SetActive(false);
            if (videoRT != null)
            {
                videoPlayer.targetTexture = null;
                videoRT.Release();
                Destroy(videoRT);
            }

            // --- 2단계: 암전 및 독백 ---
            if (monologuePanel != null) monologuePanel.SetActive(true);
            if (monologueText != null)
            {
                monologueText.text = "";
                yield return StartCoroutine(TypeText(monologueText, "드디어... 연결이 끊어졌다.\n완벽한 고요.", 0.08f));
                yield return new WaitForSeconds(5.0f);
            }

            // --- 3단계: 시스템 에러 경고 ---
            if (monologuePanel != null) monologuePanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(true);

            AudioClip beepClip = ProceduralAudioHelper.CreateBeepSound(1000f, 0.8f);
            AudioClip staticClip = ProceduralAudioHelper.CreateStaticNoiseSound(7f);

            if (audioSource != null)
            {
                audioSource.PlayOneShot(beepClip, 0.6f);
                audioSource.clip = staticClip;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (errorText != null)
            {
                errorText.text = "";
                yield return StartCoroutine(TypeText(errorText, "[Error] 불법적인 오프라인 전환 감지.\n약관 위반에 따른 강제 징수 프로그램(Nightmare.exe)을 실행합니다.", 0.04f));
                yield return new WaitForSeconds(5.0f);
            }

            if (audioSource != null) audioSource.Stop();
            if (errorPanel != null) errorPanel.SetActive(false);

            yield return StartCoroutine(TransitionToPhase2());
        }

        private IEnumerator TypeText(TextMeshProUGUI targetTmp, string message, float speed)
        {
            AudioClip typeSFX = ProceduralAudioHelper.CreateTypeSound();
            targetTmp.text = message;
            targetTmp.maxVisibleCharacters = 0;
            targetTmp.ForceMeshUpdate();
            int totalChars = targetTmp.textInfo.characterCount;
            for (int i = 0; i <= totalChars; i++)
            {
                targetTmp.maxVisibleCharacters = i;
                if (i > 0 && i - 1 < targetTmp.textInfo.characterInfo.Length)
                {
                    char c = targetTmp.textInfo.characterInfo[i - 1].character;
                    if (c != ' ' && c != '\n' && c != '\r' && audioSource != null && typeSFX != null) audioSource.PlayOneShot(typeSFX, 0.15f);
                }
                yield return new WaitForSeconds(speed);
            }
        }

        private IEnumerator TransitionToPhase2()
        {
            ChromaticAberration chromatic = null;
            LensDistortion lens = null;
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out chromatic);
                postProcessVolume.profile.TryGet(out lens);
            }
            AudioClip glitchClip = ProceduralAudioHelper.CreateGlitchSound(1.5f);
            if (audioSource != null && glitchClip != null) audioSource.PlayOneShot(glitchClip, 0.4f);

            float duration = 1.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                if (chromatic != null) chromatic.intensity.value = Mathf.Lerp(0f, 1f, progress);
                if (lens != null) { lens.intensity.value = Mathf.Lerp(0f, -0.6f, progress); lens.scale.value = Mathf.Lerp(1f, 1.25f, progress); }
                if (blackOverlay != null) blackOverlay.alpha = progress;
                yield return null;
            }
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
