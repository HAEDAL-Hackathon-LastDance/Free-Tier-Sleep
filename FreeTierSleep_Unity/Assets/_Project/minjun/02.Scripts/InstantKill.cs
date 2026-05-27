using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
    [Header("Kill Events")]
    [Tooltip("플레이어가 데미지를 입었을 때 호출될 이벤트 (UI 띄우기, 사운드 재생 등)")]
    public UnityEvent onKill;

    private Collider2D col;

    private void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 콜라이더는 반드시 Trigger여야 함
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // 플레이어가 글리치 안에 머무는 동안 계속 데미지 판정을 시도하도록 OnTriggerStay2D 사용
    private void OnTriggerStay2D(Collider2D other)
    {
        // 충돌한 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 글리치 영역의 하단 1/3 지점 계산
                float bottomY = col.bounds.min.y;
                float height = col.bounds.size.y;
                float dangerThreshold = bottomY + (height / 3f);

                // 플레이어의 위치가 하단 1/3 이하인지 확인 (즉사 구역)
                if (other.transform.position.y <= dangerThreshold)
                {
                    if (!playerController.isDead)
                    {
                        playerController.InstantDie();
                        onKill?.Invoke();
                    }
                }
                else
                {
                    // 상단 2/3 구역 (일반 데미지)
                    if (playerController.TakeDamage())
                    {
                        onKill?.Invoke();
                    }
                }
            }
        }
    }
}
