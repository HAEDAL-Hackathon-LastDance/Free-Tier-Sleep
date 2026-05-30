using UnityEngine;

// 무게 텍스트 (Heavy Spam)
// - 하늘에서 수직으로 일정 속도로 낙하
// - 플레이어 충돌 시 일정 시간 동안 플레이어 중력 배수 증가 (PlayerController.ApplyGravityDebuff)
// - 플레이어 외 충돌 또는 화면 밖 이탈 시 풀로 반환
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HeavySpam : MonoBehaviour
{
    [Header("Fall Settings")]
    [Tooltip("수직 낙하 속도")]
    public float fallSpeed = 6f;

    [Header("Debuff Settings")]
    [Tooltip("플레이어에게 적용할 중력 배수")]
    public float gravityMultiplier = 2f;

    [Tooltip("디버프 지속 시간 (초)")]
    public float debuffDuration = 3f;

    [Header("Cleanup")]
    [Tooltip("카메라 하단에서 이만큼 더 아래로 벗어나면 자동 회수")]
    public float despawnMarginBelowCamera = 3f;

    [Tooltip("ObjectPooler에 등록된 풀 태그")]
    public string poolTag = "HeavySpam";

    private Rigidbody2D rb;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 일정 속도로 수직 낙하시키기 위해 자체 중력은 끄고 linearVelocity로 제어
        rb.gravityScale = 0f;
    }

    private void OnEnable()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // 프리즈 중에는 낙하 정지 (속도 0으로)
        if (FreezeManager.IsFrozen)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        // 프리즈 해제 직후 다시 낙하 속도 회복
        if (rb != null && rb.linearVelocity.y > -0.01f) rb.linearVelocity = Vector2.down * fallSpeed;

        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize - despawnMarginBelowCamera;
        if (transform.position.y < cameraBottomY)
        {
            ReturnToPool();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 충돌 효과 무시
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && !player.IsInvincible)
            {
                player.ApplyGravityDebuff(debuffDuration, gravityMultiplier);
            }
            ReturnToPool();
        }
        else
        {
            // 발판, 바닥 등 다른 콜라이더에 부딪혀도 사라짐
            ReturnToPool();
        }
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
