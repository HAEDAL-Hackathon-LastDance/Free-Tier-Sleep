using UnityEngine;
using UnityEngine.UI;

namespace Taehui
{
    /// <summary>
    /// 게임 내 설정(볼륨 조절 등) 팝업을 제어하는 스크립트
    /// </summary>
    public class OptionsPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button closeButton;

        private AudioSource audioSource;
        private AudioClip warningSFX;
        private AudioClip clickSFX;

        private const string VolumePrefKey = "MasterVolume";

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.25f;

            // SFX 생성
            warningSFX = ProceduralAudioHelper.CreateWarningSound();
            clickSFX = ProceduralAudioHelper.CreateHoverSound();

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        private void OnEnable()
        {
            // 팝업 열릴 때 띵 소리 경고음 연출
            if (audioSource != null && warningSFX != null)
            {
                audioSource.PlayOneShot(warningSFX);
            }

            // 볼륨 설정 불러오기 (기본값: 1.0)
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
            if (volumeSlider != null)
            {
                volumeSlider.value = savedVolume;
            }
            AudioListener.volume = savedVolume;
        }

        /// <summary>
        /// 마스터 볼륨 설정 및 PlayerPrefs 저장
        /// </summary>
        public void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumePrefKey, value);
        }

        /// <summary>
        /// 팝업 비활성화
        /// </summary>
        public void Close()
        {
            if (audioSource != null && clickSFX != null)
            {
                audioSource.PlayOneShot(clickSFX);
            }
            gameObject.SetActive(false);
        }
    }
}
