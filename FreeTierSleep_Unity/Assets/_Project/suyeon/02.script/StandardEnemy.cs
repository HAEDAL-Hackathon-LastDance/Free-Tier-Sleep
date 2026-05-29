using System.Collections;
using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f;
    private float attackTimer = 100f;

    [Header("Enemy Settings")]
    public float moveSpeed = 1.0f;
    public int attackDamage = 10;

    [Header("Target Tracking")]
    public Transform coreTransform;

    private bool _isDying = false;
    private SpriteRenderer _sr;
    private Collider2D _col;

    void Start()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();

        // [중요] 게임 시작 시 "Player" 태그를 찾아서 타겟으로 삼기
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
        if (realPlayer != null)
            coreTransform = realPlayer.transform;
    }

    void Update()
    {
        // 죽는 애니메이션 중이면 로직 정지
        if (_isDying) return;

        if (coreTransform != null)
            transform.position = Vector2.MoveTowards(
                transform.position, coreTransform.position, moveSpeed * Time.deltaTime);

        attackTimer += Time.deltaTime;
        
        // ariwr님의 Phase 2 기능: 드로잉 선 충돌 감지
        CheckLineCollision(); 
    }

    void CheckLineCollision()
    {
        if (_col == null) return;

        // 적 콜라이더의 실제 월드 경계(바깥 선)로 감지
        Bounds b = _col.bounds;
        Collider2D[] hits = Physics2D.OverlapAreaAll(
            new Vector2(b.min.x, b.min.y),
            new Vector2(b.max.x, b.max.y)
        );

        foreach (var hit in hits)
        {
            if (hit != _col && hit.GetComponent<Stroke>() != null)
            {
                StartCoroutine(FadeAndDestroy());
                return;
            }
        }
    }

    IEnumerator FadeAndDestroy()
    {
        _isDying = true;

        float duration = 0.35f;
        float elapsed  = 0f;
        Color original = _sr != null ? _sr.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            if (_sr != null)
                _sr.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (_isDying) return;

        if (other.CompareTag("Player"))
        {
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);

                    AudioSource playerAudio = other.GetComponent<AudioSource>();
                    if (playerAudio != null)
                        playerAudio.Play();

                    attackTimer = 0f;
                }
            }
        }
    }
}