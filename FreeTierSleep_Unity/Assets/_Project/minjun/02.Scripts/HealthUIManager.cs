using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Image[] heartImages; // 체력 UI 이미지 배열 (3개)

    private PlayerController player;

    void Start()
    {
        // 씬에서 플레이어를 찾아 이벤트 구독
        player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnHealthChanged += UpdateHearts;
            // 초기 체력 UI 동기화
            UpdateHearts(player.currentHp);
        }
        else
        {
            Debug.LogWarning("HealthUIManager: PlayerController를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHearts;
        }
    }

    // 체력 변경 시 호출되는 메서드
    private void UpdateHearts(int currentHp)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                // 현재 체력보다 인덱스가 작으면 활성화(보임), 크거나 같으면 비활성화(안 보임)
                heartImages[i].enabled = i < currentHp;
            }
        }
    }
}
