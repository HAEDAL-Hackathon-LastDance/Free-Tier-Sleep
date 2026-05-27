using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f; // 1초마다 데미지를 줌
    private float attackTimer = 100f;   // 첫 타격은 닿자마자 바로 때리도록 큰 숫자로 시작
    [Header("Enemy Settings")]
    public float moveSpeed = 1.0f;     // 일정 속도로 직진 이동
    public int attackDamage = 10;      // 코어에 줄 데미지

    [Header("Target Tracking")]
    public Transform coreTransform;    // 중앙 코어 Transform 참조
    void Start()
    {
        // [중요] 게임 시작 시, 씬에 있는 "Player"라는 태그를 가진 진짜 오브젝트를 찾아서 타겟으로 삼아라!
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
        if (realPlayer != null)
        {
            coreTransform = realPlayer.transform;
        }
    }
    void Update()
    {
        // 코어가 게임에 존재할 때만 이동
        if (coreTransform != null)
        {
            // 명세서 요구사항: Vector2.MoveTowards 사용
            // 현재 위치에서 코어 위치를 향해 moveSpeed만큼 일정하게 직진
            transform.position = Vector2.MoveTowards(transform.position, coreTransform.position, moveSpeed * Time.deltaTime);
        }
        // 매 프레임마다 타이머 시간에 deltaTime을 더해줌
        attackTimer += Time.deltaTime;
    }

    // Enter 대신 Stay를 쓰면 닿아있는 내내 계속 실행됨!
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 타이머가 쿨타임(1초)을 넘겼을 때만 데미지를 줌
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);

                    // ?? 플레이어 오브젝트에 붙어있는 AudioSource(에러음)를 찾아 재생해라!
                    AudioSource playerAudio = other.GetComponent<AudioSource>();
                    if (playerAudio != null)
                    {
                        playerAudio.Play();
                    }

                    attackTimer = 0f; // 때렸으니까 타이머를 다시 0으로 초기화!
                }
            }
        }
    }
}
