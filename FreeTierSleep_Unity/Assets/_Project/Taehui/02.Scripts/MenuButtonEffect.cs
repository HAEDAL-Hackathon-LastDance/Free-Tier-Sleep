using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Taehui
{
    /// <summary>
    /// 마우스 호버 시 텍스트 글리치 연출 및 레트로 오디오 피드백을 제공하는 컴포넌트
    /// </summary>
    public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private float glitchSpeed = 0.05f; // 글리치 문자 교체 속도
        [SerializeField] private float glitchVolume = 0.15f; // 호버 효과음 크기

        private TextMeshProUGUI tmpText;
        private string originalText;
        private Coroutine glitchCoroutine;
        private AudioSource audioSource;
        private AudioClip hoverSFX;

        // 글리치에 사용될 터미널용 무작위 노이즈 문자 세트
        private static readonly char[] GlitchChars = { '#', '@', '$', '%', '&', '*', '0', '1', '_', '?', '!', '[', ']' };

        private void Awake()
        {
            tmpText = GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                originalText = tmpText.text;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = glitchVolume;

            hoverSFX = ProceduralAudioHelper.CreateHoverSound();
        }

        private void OnDisable()
        {
            ResetText();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (audioSource != null && hoverSFX != null)
            {
                audioSource.PlayOneShot(hoverSFX);
            }

            if (glitchCoroutine != null)
                StopCoroutine(glitchCoroutine);

            glitchCoroutine = StartCoroutine(GlitchRoutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetText();
        }

        private void ResetText()
        {
            if (glitchCoroutine != null)
            {
                StopCoroutine(glitchCoroutine);
                glitchCoroutine = null;
            }

            if (tmpText != null && !string.IsNullOrEmpty(originalText))
            {
                tmpText.text = originalText;
            }
        }

        private IEnumerator GlitchRoutine()
        {
            if (tmpText == null || string.IsNullOrEmpty(originalText)) yield break;

            char[] textBuffer = originalText.ToCharArray();

            while (true)
            {
                // 원본 글자 중 약 25% 가량을 무작위 글리치 문자로 뒤바꿈
                for (int i = 0; i < textBuffer.Length; i++)
                {
                    if (originalText[i] == ' ') 
                    {
                        textBuffer[i] = ' ';
                        continue;
                    }

                    if (Random.value < 0.25f)
                    {
                        textBuffer[i] = GlitchChars[Random.Range(0, GlitchChars.Length)];
                    }
                    else
                    {
                        textBuffer[i] = originalText[i];
                    }
                }

                tmpText.text = new string(textBuffer);
                yield return new WaitForSeconds(glitchSpeed);
            }
        }
    }
}
