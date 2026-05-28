using System.Collections;
using UnityEngine;
using TMPro;

namespace Taehui
{
    /// <summary>
    /// 텍스트를 한 글자씩 출력하는 타이핑 효과 클래스
    /// </summary>
    public class TypingEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.1f;
        
        private TMP_Text textComponent;
        private Coroutine typingCoroutine;
        private AudioSource audioSource;
        private AudioClip typeClip;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            // 오디오 소스 추가 혹은 참조
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.2f; // 너무 시끄럽지 않게 볼륨 조절

            // 절차적 사운드 미리 로드
            typeClip = ProceduralAudioHelper.CreateTypeSound();
        }

        public void Play(string message, System.Action onComplete = null, bool append = false)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            
            typingCoroutine = StartCoroutine(TypeText(message, onComplete, append));
        }

        private IEnumerator TypeText(string message, System.Action onComplete, bool append)
        {
            if (!append)
            {
                textComponent.text = "";
            }
            else
            {
                textComponent.text += "\n"; // 이전 대사 끝에 줄바꿈을 덧붙임
            }

            foreach (char letter in message.ToCharArray())
            {
                textComponent.text += letter;
                
                // 공백이 아닐 때만 타자기 소리 재생
                if (letter != ' ' && audioSource != null && typeClip != null)
                {
                    audioSource.PlayOneShot(typeClip);
                }
                
                yield return new WaitForSeconds(typingSpeed);
            }

            typingCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
