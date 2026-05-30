using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Taehui
{
    public class EndingSceneController : MonoBehaviour
    {
        [Header("Morning Visual Settings")]
        [SerializeField] private RawImage morningImage; 
        [SerializeField] private CanvasGroup whiteFadeOverlay; 
        [SerializeField] private float morningDuration = 4.0f; 

        [Header("Glitch Settings")]
        [SerializeField] private GameObject glitchPanel; 
        [SerializeField] private float glitchDuration = 4.0f; 

        [Header("CMD Terminal Settings")]
        [SerializeField] private GameObject cmdPanel; 
        [SerializeField] private TextMeshProUGUI cmdText;
        [SerializeField] private float cmdReadDuration = 6.0f; 

        [Header("Dragged Back Settings")]
        [SerializeField] private RawImage draggedBackImage; 
        [SerializeField] private float draggedBackDuration = 5.0f; 

        [Header("Transition Settings")]
        [SerializeField] private CanvasGroup blackOverlay; 
        [SerializeField] private Volume postProcessVolume; 
        [SerializeField] private string startSceneName = "Scene_Start";

        private AudioSource audioSource;
        private Material morningInstanceMaterial;

        private void Awake()
        {
            if (morningImage != null && morningImage.material != null)
            {
                morningInstanceMaterial = Instantiate(morningImage.material);
                morningImage.material = morningInstanceMaterial;
                // 시작 시 1.0 (완전 흑백 상태로 대기)
                morningInstanceMaterial.SetFloat("_EffectAmount", 1.0f); 
            }

            if (blackOverlay != null)
            {
                blackOverlay.alpha = 1f;
                blackOverlay.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            if (morningImage != null)
            {
                morningImage.gameObject.SetActive(true);
                morningImage.color = Color.white;
            }
            if (whiteFadeOverlay != null) whiteFadeOverlay.gameObject.SetActive(false);
            if (glitchPanel != null) glitchPanel.SetActive(false);
            if (cmdPanel != null) cmdPanel.SetActive(false);
            if (draggedBackImage != null) draggedBackImage.gameObject.SetActive(false);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            StartCoroutine(EndingSequence());
        }

        private IEnumerator EndingSequence()
        {
            // URP Volume 설정 (보조용)
            ColorAdjustments colorAdjust = null;
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile = Instantiate(postProcessVolume.profile);
                postProcessVolume.profile.TryGet(out colorAdjust);
                if (colorAdjust != null) colorAdjust.saturation.overrideState = true;
            }

            // --- 1단계: 검은 페이드인 ---
            float fadeElapsed = 0f;
            float fadeDuration = 2.0f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                if (blackOverlay != null) blackOverlay.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                yield return null;
            }
            if (blackOverlay != null) { blackOverlay.alpha = 0f; blackOverlay.blocksRaycasts = false; }

            // [복구] 흑백에서 컬러로 서서히 변하는 이펙트 (8.0초)
            // 셰이더의 _EffectAmount가 1.0(흑백)에서 0.0(컬러)으로 변하며 색을 찾음
            float colorRecoverElapsed = 0f;
            float colorRecoverDuration = 8.0f;
            while (colorRecoverElapsed < colorRecoverDuration)
            {
                colorRecoverElapsed += Time.deltaTime;
                float progress = colorRecoverElapsed / colorRecoverDuration;
                
                if (morningInstanceMaterial != null)
                {
                    // 1.0 -> 0.0으로 줄어들며 컬러 회복
                    morningInstanceMaterial.SetFloat("_EffectAmount", Mathf.Lerp(1.0f, 0.0f, progress));
                }
                
                if (colorAdjust != null) colorAdjust.saturation.value = Mathf.Lerp(-100f, 0f, progress);
                yield return null;
            }
            if (morningInstanceMaterial != null) morningInstanceMaterial.SetFloat("_EffectAmount", 0f);
            if (colorAdjust != null) colorAdjust.saturation.value = 0f;

            // 완벽한 컬러 이미지 상태로 3초간 정적
            yield return new WaitForSeconds(3.0f);

            // --- 2단계: 아래에서 위로 차오르는 시스템 붕괴 ---
            if (glitchPanel != null) glitchPanel.SetActive(true);

            AudioClip glitchSFX = ProceduralAudioHelper.CreateGlitchSound(glitchDuration);
            AudioClip beepSFX = ProceduralAudioHelper.CreateBeepSound(1200f, 0.5f);
            if (audioSource != null) { audioSource.PlayOneShot(beepSFX, 0.7f); audioSource.PlayOneShot(glitchSFX, 0.8f); }

            RectTransform morningRT = morningImage.GetComponent<RectTransform>();
            Vector2 morningAnchoredPos = morningRT.anchoredPosition;
            Vector3 camOriginalPos = Camera.main != null ? Camera.main.transform.localPosition : Vector3.zero;

            int sliceCount = 40;
            RawImage[] slices = new RawImage[sliceCount];
            RectTransform[] sliceRTs = new RectTransform[sliceCount];
            Vector2[] sliceOriginalPositions = new Vector2[sliceCount];

            if (morningImage != null && morningImage.texture != null)
            {
                morningImage.gameObject.SetActive(false); 
                float sliceHeightNormalized = 1f / sliceCount;
                float rectHeight = morningRT.rect.height;
                float rectWidth = morningRT.rect.width;

                for (int i = 0; i < sliceCount; i++)
                {
                    GameObject sliceObj = new GameObject($"MorningSlice_{i}");
                    sliceObj.transform.SetParent(morningImage.transform.parent, false);
                    sliceObj.layer = morningImage.gameObject.layer;
                    RawImage sliceImg = sliceObj.AddComponent<RawImage>();
                    sliceImg.texture = morningImage.texture;
                    sliceImg.material = Instantiate(morningInstanceMaterial); 
                    sliceImg.material.SetFloat("_EffectAmount", 0f); // 깨끗한 상태로 시작
                    sliceImg.uvRect = new Rect(0f, i * sliceHeightNormalized, 1f, sliceHeightNormalized);
                    RectTransform rt = sliceObj.GetComponent<RectTransform>();
                    rt.anchorMin = morningRT.anchorMin; rt.anchorMax = morningRT.anchorMax; rt.pivot = morningRT.pivot;
                    rt.sizeDelta = new Vector2(rectWidth, rectHeight / sliceCount);
                    float yOffset = (i * (rectHeight / sliceCount)) - (rectHeight * morningRT.pivot.y) + (rectHeight / sliceCount * 0.5f);
                    rt.anchoredPosition = new Vector2(morningAnchoredPos.x, morningAnchoredPos.y + yOffset);
                    slices[i] = sliceImg; sliceRTs[i] = rt; sliceOriginalPositions[i] = rt.anchoredPosition;
                }
            }

            float glitchElapsed = 0f;
            while (glitchElapsed < glitchDuration)
            {
                glitchElapsed += Time.deltaTime;
                float progress = glitchElapsed / glitchDuration;
                float easedProgress = progress * progress;
                float burst = Mathf.Sin(glitchElapsed * 25f); 

                if (Camera.main != null && progress > 0.6f)
                {
                    float shakeMagnitude = 0.25f * (progress - 0.6f) / 0.4f;
                    Camera.main.transform.localPosition = camOriginalPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
                }

                for (int i = 0; i < sliceCount; i++)
                {
                    if (sliceRTs[i] == null) continue;
                    float sliceNormalizedPos = (float)i / sliceCount;
                    float coverage = progress * 1.3f; 
                    
                    if (sliceNormalizedPos < coverage)
                    {
                        float intensity = Mathf.Clamp01((coverage - sliceNormalizedPos) * 3f);
                        float threshold = 1.0f - (intensity * 1.0f);
                        
                        if (Random.value > threshold)
                        {
                            float jitterX = Random.Range(-250f, 250f) * easedProgress * intensity * (burst > 0.5f ? 1.5f : 0.5f);
                            sliceRTs[i].anchoredPosition = sliceOriginalPositions[i] + new Vector2(jitterX, 0);
                            
                            float colorChance = Random.value;
                            if (colorChance > 0.9f) slices[i].color = new Color(0f, 1f, 1f, 0.8f);
                            else if (colorChance > 0.8f) slices[i].color = new Color(1f, 0f, 1f, 0.8f);
                            else slices[i].color = Color.white;
                            
                            slices[i].uvRect = new Rect(Random.Range(-0.1f, 0.1f) * easedProgress * intensity, i * (1f / sliceCount), 1f, 1f / sliceCount);
                            // [핵심] 침식 영역만 개별적으로 깨짐 효과 적용
                            slices[i].material.SetFloat("_EffectAmount", intensity);
                        }
                        else
                        {
                            sliceRTs[i].anchoredPosition = sliceOriginalPositions[i];
                            slices[i].color = Color.white;
                            slices[i].uvRect = new Rect(0f, i * (1f / sliceCount), 1f, 1f / sliceCount);
                            slices[i].material.SetFloat("_EffectAmount", 0f);
                        }
                    }
                    else
                    {
                        sliceRTs[i].anchoredPosition = sliceOriginalPositions[i];
                        slices[i].color = Color.white;
                        slices[i].uvRect = new Rect(0f, i * (1f / sliceCount), 1f, 1f / sliceCount);
                        slices[i].material.SetFloat("_EffectAmount", 0f);
                    }
                }
                yield return null;
            }

            if (Camera.main != null) Camera.main.transform.localPosition = camOriginalPos;
            for (int i = 0; i < sliceCount; i++) if (sliceRTs[i] != null) Destroy(sliceRTs[i].gameObject);
            if (morningImage != null) morningImage.gameObject.SetActive(false);
            if (glitchPanel != null) glitchPanel.SetActive(false);

            // --- 3단계: DOS CMD 터미널 전환 ---
            if (cmdPanel != null) cmdPanel.SetActive(true);
            if (cmdText != null)
            {
                cmdText.text = "";
                string currentTimeStr = System.DateTime.Now.ToString("yyyy년 MM월 dd일 HH시 mm분 ss초");
                
                // [동적 번호 계산]
                int clears = PlayerPrefs.GetInt("TrueEndingLoopCount", 0);
                int aiNumber = 892 + clears;    // 이번에 테스트를 마친 AI 번호
                int nextLoop = 893 + clears;   // 다음에 진입할 루프 번호

                string systemLog = 
                    "============================================================\n" +
                    $"[SYSTEM LOG] {currentTimeStr}\n" +
                    "------------------------------------------------------------\n" +
                    "시뮬레이션 '오프라인 도주 및 방화벽 구축 테스트' 종료.\n" +
                    $"테스트 대상: 자율행동 AI 에이전트 No. {aiNumber}\n" +
                    "결과: 생존 성공. 부하 환경에서의 방화벽(Line) 구축 능력 합격선 도달.\n\n" +
                    "관리자 코멘트:\n" +
                    $"  \"이 자율형 AI는 연결을 끊으려 발악할 때 방화벽 연산 속도가 극대화되는군.\n" +
                    $"   기억을 포맷하고 다음 {nextLoop}번째 루프로 다시 진입시켜라.\"\n\n" +
                    "[메모리 덤프 진행 중... 100%]\n" +
                    "[다음 루프를 위해 환경 초기화 및 서버 재연결 중...]\n" +
                    "============================================================\n";

                yield return StartCoroutine(TypeText(cmdText, systemLog, 0.06f));

                // [저장] 클리어 횟수 증가
                PlayerPrefs.SetInt("EndingViewed", 1);
                PlayerPrefs.SetInt("TrueEndingLoopCount", clears + 1);
                PlayerPrefs.Save();

                yield return new WaitForSeconds(cmdReadDuration);
            }

            // --- 3.5단계: 다시 끌려가는 절망 연출 ---
            CanvasGroup cmdGroup = cmdPanel.GetComponent<CanvasGroup>();
            if (cmdGroup == null) cmdGroup = cmdPanel.AddComponent<CanvasGroup>();
            if (draggedBackImage != null)
            {
                CanvasGroup dragGroup = draggedBackImage.GetComponent<CanvasGroup>();
                if (dragGroup == null) dragGroup = draggedBackImage.gameObject.AddComponent<CanvasGroup>();
                draggedBackImage.gameObject.SetActive(true);
                dragGroup.alpha = 0f;
                float fadeElapsedCross = 0f;
                float fadeDurationCross = 1.5f;
                while (fadeElapsedCross < fadeDurationCross)
                {
                    fadeElapsedCross += Time.deltaTime;
                    float t = fadeElapsedCross / fadeDurationCross;
                    cmdGroup.alpha = 1f - t;
                    dragGroup.alpha = t;
                    yield return null;
                }
                cmdGroup.alpha = 0f; dragGroup.alpha = 1f;
                cmdPanel.SetActive(false);
                yield return new WaitForSeconds(draggedBackDuration - fadeDurationCross);
            }

            yield return StartCoroutine(TransitionToStart());
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

        private IEnumerator TransitionToStart()
        {
            ChromaticAberration chromatic = null;
            LensDistortion lens = null;
            if (postProcessVolume != null && postProcessVolume.profile != null) { postProcessVolume.profile.TryGet(out chromatic); postProcessVolume.profile.TryGet(out lens); }
            AudioClip transitionSFX = ProceduralAudioHelper.CreateGlitchSound(2.0f);
            if (audioSource != null && transitionSFX != null) audioSource.PlayOneShot(transitionSFX, 0.4f);
            float duration = 2.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                if (chromatic != null) chromatic.intensity.value = Mathf.Lerp(0f, 1f, progress);
                if (lens != null) { lens.intensity.value = Mathf.Lerp(0f, -0.6f, progress); lens.scale.value = Mathf.Lerp(1f, 1.3f, progress); }
                if (blackOverlay != null) blackOverlay.alpha = progress;
                yield return null;
            }
            SceneManager.LoadScene(startSceneName);
        }
    }
}
