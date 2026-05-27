using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
    [Header("Kill Events")]
    [Tooltip("플레이어가 데미지를 입었을 때 호출될 이벤트 (UI 띄우기, 사운드 재생 등)")]
    public UnityEvent onKill;

    [Header("Danger Zone Settings")]
    [Tooltip("시각적인 즉사 구역(하단 1/3)보다 실제 즉사 판정을 얼마나 더 아래로 내릴지 (여유 공간)")]
    public float dangerZoneMargin = 0.5f;

    [Header("Bounce Settings")]
    [Tooltip("글리치 상단에 닿아 데미지를 입었을 때 위로 튕겨 오르는 힘")]
    public float bounceForce = 18f;

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
                
                // 시각적 1/3 지점에서 margin만큼 더 아래로 내려서 실제 즉사 판정선을 만듦
                float dangerThreshold = bottomY + (height / 3f) - dangerZoneMargin;

                // 플레이어의 위치가 실제 즉사 판정선 이하인지 확인 (즉사 구역)
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
                    // 상단 구역 (일반 데미지)
                    if (playerController.TakeDamage())
                    {
                        // 데미지를 입었을 때 위로 강하게 튕겨 오르게 하여 복구 기회 제공
                        playerController.BounceUp(bounceForce);
                        onKill?.Invoke();
                    }
                }
            }
        }
    }
}
