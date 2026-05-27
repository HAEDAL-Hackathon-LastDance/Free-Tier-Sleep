using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InstantKill : MonoBehaviour
{
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
        // 충돌한 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the Data Garbage Collector! Instant Kill.");
            
            // 1. 플레이어 조작 잠금
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // 2. 물리 연산 정지 (허공에 멈추거나 바닥으로 떨어지게 둠)
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; // 물리 충돌 및 중력 연산 완전 정지
            }
            
            // TODO: 게임 오버 UI 호출, 사망 애니메이션 재생 또는 씬 재시작 로직 추가
        }
    }
}
