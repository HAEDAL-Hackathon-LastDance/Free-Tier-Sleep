using System.Collections;
using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f;
    private float attackTimer = 100f;
    
    // 수연님 추가: 적이 공격(흔들림) 중인지 체크하는 스위치!
    private bool isAttacking = false;

    [Header("Enemy Settings")]
    public float moveSpeed = 2.0f;
    public int attackDamage = 8;

    [Header("Target Tracking")]
    public Transform coreTransform;

    // ariwr님 추가: 사망(페이드아웃) 처리용 변수
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
        // 죽는 애니메이션 중이면 로직 정지 (ariwr)
        if (_isDying) return;

        // 코어가 존재하고, 공격 중이 아닐 때만 이동 (수연)
        if (coreTransform != null && !isAttacking)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, coreTransform.position, moveSpeed * Time.deltaTime);
        }

        // 공격 중이 아닐 때만 쿨타임 증가 (수연)
        if (!isAttacking)
        {
            attackTimer += Time.deltaTime;
        }
        
        // 드로잉 선 충돌 감지 (ariwr)
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
        // 죽는 중이면 데미지 판정 무시 (ariwr)
        if (_isDying) return;

        // 공격 중이 아닐 때만 새로운 공격 시작 가능 (수연)
        if (other.CompareTag("Player") && !isAttacking)
        {
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
                AudioSource playerAudio = other.GetComponent<AudioSource>(); 

                if (playerHP != null)
                {
                    // 즉시 데미지 대신 흔들림 코루틴 실행 (수연)
                    StartCoroutine(ShakeAndAttack(playerHP, playerAudio));
                }
            }
        }
    }

    // 3번 미션: 잠깐 멈춰서 부들부들 흔들리고 데미지를 주는 코루틴 (수연)
    IEnumerator ShakeAndAttack(PlayerHealth playerHP, AudioSource playerAudio)
    {
        isAttacking = true; 
        attackTimer = 0f;   

        Vector3 originalPos = transform.position;

        for (int i = 0; i < 5; i++)
        {
            // 흔들리는 도중에 선에 맞아 죽으면 코루틴 즉시 종료 (안전장치 통합)
            if (_isDying) yield break; 

            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
            yield return new WaitForSeconds(0.05f); 
        }

        if (_isDying) yield break; 

        transform.position = originalPos;

        // 흔들림 연출 끝난 후 데미지 넣기
        playerHP.TakeDamage(attackDamage);

        if (playerAudio != null)
        {
            playerAudio.Play();
        }

        isAttacking = false; 
    }
}