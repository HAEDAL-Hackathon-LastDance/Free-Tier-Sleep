using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
    [Header("Kill Events")]
    [Tooltip("플레이어가 데미지를 입었을 때 호출될 이벤트 (UI 띄우기, 사운드 재생 등)")]
    public UnityEvent onKill;

    private void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 콜라이더는 반드시 Trigger여야 함
        Collider2D col = GetComponent<Collider2D>();
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
                // TakeDamage 내부에서 무적 상태를 체크하므로 연속 호출되어도 안전함
                playerController.TakeDamage();
            }
            
            // 등록된 외부 이벤트 실행
            onKill?.Invoke();
        }
    }
}
