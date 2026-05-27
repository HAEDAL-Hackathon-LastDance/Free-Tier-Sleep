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
            
            // 플레이어의 Die 연출 메서드 호출
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Die();
            }
            
            // 등록된 외부 이벤트 실행 (게임 오버 매니저 호출 등)
            onKill?.Invoke();
        }
    }
}
