using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
    [Header("Kill Events")]
    [Tooltip("플레이어가 데미지를 입었을 때 호출될 이벤트 (UI 띄우기, 사운드 재생 등)")]
    public UnityEvent onKill;

    [Header("Damage Zone Settings")]
    [Tooltip("시각적인 글리치 상단보다 실제 피격 판정을 얼마나 더 아래로 내릴지 (여유 공간)")]
    public float topMargin = 0.5f;

    [Header("Danger Zone Settings")]
    [Tooltip("시각적인 즉사 구역(하단 1/3)보다 실제 즉사 판정을 얼마나 더 아래로 내릴지 (여유 공간)")]
    public float dangerZoneMargin = 0.5f;

    [Header("Bounce Settings")]
    [Tooltip("낙하 속도와 무관하게 보장되는 최소 바운스 속도 (유닛/초) — 발판까지 닿는 최소 높이 보장")]
    public float minBounceSpeed = 30f;
    [Tooltip("바운스 속도 상한 — 극단적 낙하 시 화면 밖으로 날아가지 않도록 제한")]
    public float maxBounceSpeed = 75f;

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
                // 글리치 영역의 상단 및 하단 계산
                float topY = col.bounds.max.y;
                float bottomY = col.bounds.min.y;
                float height = col.bounds.size.y;
                
                // 상단 여유 공간(안전 구역) 계산
                float safeThreshold = topY - topMargin;

                // 플레이어가 상단 여유 공간에 머물고 있다면 데미지 판정 무시
                if (other.transform.position.y > safeThreshold)
                {
                    return;
                }

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
                        // 낙하 속도를 그대로 반전시켜 위로 쏨 — 오래 떨어질수록 높이 튕김
                        playerController.BounceUpAdaptive(minBounceSpeed, maxBounceSpeed);
                        onKill?.Invoke();
                    }
                }
            }
        }
    }
}
