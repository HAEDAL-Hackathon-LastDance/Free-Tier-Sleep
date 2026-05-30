using UnityEngine;

// 슈퍼점프 스프링 — 밟으면 매우 높이 무적 상태로 튀어 오름
[RequireComponent(typeof(Collider2D))]
public class SuperJumpItem : MonoBehaviour
{
    [Tooltip("위로 가하는 임펄스 (일반 점프 ~10 대비 매우 큼)")]
    public float jumpForce = 50f;

    [Tooltip("점프 후 무적 + 깜빡임 지속 시간 (초)")]
    public float invincibleDuration = 2.5f;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) player.SuperJump(jumpForce, invincibleDuration);
        Destroy(gameObject);
    }
}
