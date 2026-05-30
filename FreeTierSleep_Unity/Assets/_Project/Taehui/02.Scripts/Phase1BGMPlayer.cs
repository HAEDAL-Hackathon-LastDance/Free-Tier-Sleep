using UnityEngine;

namespace Taehui
{
    /// <summary>
    /// Phase 1 씬에 사운드(BGM)를 재생하는 스크립트.
    /// 에디터 상에서 음원 파일을 직접 드래그앤드롭으로 지정할 수도 있고,
    /// 파일이 없거나 유실된 경우에는 절차적 생성(ProceduralAudioHelper)을 통해 백업 재생합니다.
    /// </summary>
    public class Phase1BGMPlayer : MonoBehaviour
    {
        private AudioSource audioSource;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.3f;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = volume;

            if (bgmClip == null)
            {
                // Resources 또는 생성기를 통해 AudioClip 로드
                bgmClip = ProceduralAudioHelper.CreatePhase1BGM(16f);
            }

            audioSource.clip = bgmClip;
            audioSource.Play();
            Debug.Log("[Phase1BGMPlayer] Phase 1 BGM 재생 성공!");
        }
    }
}
