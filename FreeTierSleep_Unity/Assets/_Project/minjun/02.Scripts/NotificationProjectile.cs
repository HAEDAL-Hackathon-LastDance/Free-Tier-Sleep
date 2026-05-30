using UnityEngine;

// 알림창 탄환 (Notification Projectile)
// - 활성화 시점에 플레이어 위치를 향한 방향을 캡처하고 일정 가속도로 유도 비행
// - 플레이어 명중 시 강한 단발성 하향 임펄스 (기획서 디버깅 가이드 #3에 따라 SpringJoint2D 미사용)
// - 일정 수명 또는 화면 밖 이탈 시 풀로 반환
[RequireComponent(typeof(Collider2D))]
public class NotificationProjectile : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("비행 속도")]
    public float flightSpeed = 7f;

    [Tooltip("유도 강도 (0 = 직선 발사, 값이 클수록 강하게 플레이어를 추적)")]
    [Range(0f, 5f)]
    public float homingStrength = 1f;

    [Header("Hit Settings")]
    [Tooltip("플레이어를 아래로 끌어내리는 순간 임펄스 크기")]
    public float downwardImpulse = 25f;

    [Header("Cleanup")]
    [Tooltip("최대 생존 시간 (초)")]
    public float lifetime = 5f;

    [Tooltip("ObjectPooler에 등록된 풀 태그")]
    public string poolTag = "NotificationProjectile";

    private Vector2 currentDirection;
    private Transform playerTransform;
    private float spawnTime;
    private Collider2D col;

    // 풀에서 자주 활성화되는 발사체가 매번 FindGameObjectWithTag 하지 않도록 static 1회 캐싱
    // 씬 전환 시 Unity가 reference를 무효화하므로 null 체크로 안전
    private static Transform sharedPlayerTransform;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        spawnTime = Time.time;

        if (sharedPlayerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) sharedPlayerTransform = playerObj.transform;
        }

        playerTransform = sharedPlayerTransform;
        if (playerTransform != null)
        {
            currentDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        }
        else
        {
            currentDirection = Vector2.left; // 폴백
        }
    }

    private void Update()
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 비행 정지

        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
            return;
        }

        // 유도: 현재 방향을 매 프레임 플레이어 쪽으로 보간
        if (playerTransform != null && homingStrength > 0f)
        {
            Vector2 desired = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            currentDirection = Vector2.Lerp(currentDirection, desired, homingStrength * Time.deltaTime).normalized;
        }

        transform.position += (Vector3)(currentDirection * flightSpeed * Time.deltaTime);

        // 진행 방향으로 회전 (스프라이트가 +X 방향을 바라보도록 그려졌다고 가정)
        if (currentDirection.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 충돌 효과 무시
        if (!other.CompareTag("Player")) return;

        PlayerController playerCtrl = other.GetComponent<PlayerController>();
        if (playerCtrl == null || !playerCtrl.IsInvincible)
        {
            Rigidbody2D playerRb = other.attachedRigidbody;
            if (playerRb != null)
            {
                // 기존 Y속도를 0으로 만든 뒤 단발성 강한 하향 임펄스
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
                playerRb.AddForce(Vector2.down * downwardImpulse, ForceMode2D.Impulse);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
