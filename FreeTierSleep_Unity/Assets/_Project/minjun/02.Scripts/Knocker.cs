using UnityEngine;

// 넉백 바이러스 (Knocker)
// - 지정 범위를 좌우로 단순 왕복 정찰
// - 플레이어 접촉 시 데미지 없이 접촉 반대 방향의 대각선 아래로 임펄스 가함
[RequireComponent(typeof(Collider2D))]
public class Knocker : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("좌우로 왕복할 최대 거리 (시작 위치 기준)")]
    public float patrolRange = 3f;

    [Tooltip("이동 속도")]
    public float patrolSpeed = 2f;

    [Header("Knockback Settings")]
    [Tooltip("플레이어에게 가할 임펄스 크기")]
    public float knockbackForce = 18f;

    [Tooltip("옆면 충돌 시 위쪽으로 살짝 올려줄 최소 Y 성분 (0이면 순수 수평)")]
    [Range(0f, 0.6f)]
    public float sideHitUpwardBias = 0.3f;

    [Tooltip("넉백 후 수평 이동 입력 차단 시간 (초)")]
    public float knockbackStunDuration = 0.25f;

    private Vector2 anchor;     // 풀에서 꺼낼 때마다의 시작 위치
    private int direction = 1;  // 1: 오른쪽, -1: 왼쪽
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        anchor = transform.position;
        direction = 1;
        if (spriteRenderer != null) spriteRenderer.flipX = false;
    }

    private void Update()
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 정지
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);

        float offset = transform.position.x - anchor.x;
        if (offset >= patrolRange && direction > 0)
        {
            direction = -1;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
        }
        else if (offset <= -patrolRange && direction < 0)
        {
            direction = 1;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 충돌 효과 무시
        if (!other.CompareTag("Player")) return;

        PlayerController playerCtrl = other.GetComponent<PlayerController>();
        if (playerCtrl != null && playerCtrl.IsInvincible) return; // 무적 중에는 넉백 무시

        Rigidbody2D playerRb = other.attachedRigidbody;
        if (playerRb == null) return;

        // 트리거에서는 collision normal 대신 Knocker 중심 → 플레이어 중심 방향으로 넉백 방향 결정
        Vector2 knockDir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

        // 옆면 충돌(수평에 가까운 방향)일 때 위쪽 바이어스 추가
        if (Mathf.Abs(knockDir.y) < sideHitUpwardBias)
            knockDir = new Vector2(knockDir.x, sideHitUpwardBias).normalized;

        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        // 넉백 후 짧은 시간 동안 이동 입력 무시 → 키를 누르고 있어도 다시 밀려오지 않음
        playerCtrl?.ApplyExternalKnockback(knockbackStunDuration);
    }
}
