using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
    [Header("Kill Events")]
    [Tooltip("플레이어가 즉사했을 때 호출될 이벤트 (UI 띄우기, 사운드 재생 등)")]
    public UnityEvent onKill;

    private bool hasKilled = false;

    private void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 콜라이더는 반드시 Trigger여야 함
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 중복 사망 처리 방지
        if (hasKilled) return;

        // 충돌한 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            hasKilled = true;
            Debug.Log("Player hit the Data Garbage Collector! Instant Kill.");
            
            // 1. 플레이어 조작 잠금
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // 2. 물리 연산 정지 (허공에 멈추는 것이 어색하다면 simulated = false는 주석 처리하고 velocity만 0으로 만들 수도 있습니다)
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; // 물리 충돌 및 중력 연산 완전 정지
            }
            
            // 3. 등록된 외부 이벤트 실행 (게임 오버 매니저 호출 등)
            onKill?.Invoke();
        }
    }
}
