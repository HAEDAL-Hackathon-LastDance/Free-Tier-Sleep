using UnityEngine;
using System.Collections; // ⭐️ 코루틴 쓰기 위해 필수!

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f; // 1초마다 데미지를 줌
    private float attackTimer = 100f;   // 첫 타격은 닿자마자 바로 때리도록 큰 숫자로 시작

    // ⭐️ 추가: 적이 공격(흔들림) 중인지 체크하는 스위치!
    private bool isAttacking = false;

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
            // ⭐️ 수정: 공격(흔들림) 중이 아닐 때만 쫓아가도록! (안 그러면 덜덜 떨면서 다가옴)
            if (!isAttacking)
            {
                transform.position = Vector2.MoveTowards(transform.position, coreTransform.position, moveSpeed * Time.deltaTime);
            }
        }

        // 매 프레임마다 타이머 시간에 deltaTime을 더해줌 (공격 중이 아닐 때만)
        if (!isAttacking)
        {
            attackTimer += Time.deltaTime;
        }
    }

    // Enter 대신 Stay를 쓰면 닿아있는 내내 계속 실행됨!
    void OnTriggerStay2D(Collider2D other)
    {
        // ⭐️ 수정: 공격 중이 아닐 때만 새로운 공격 시작 가능!
        if (other.CompareTag("Player") && !isAttacking)
        {
            // 타이머가 쿨타임(1초)을 넘겼을 때만 데미지를 줌
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
                AudioSource playerAudio = other.GetComponent<AudioSource>(); // 오디오소스 미리 찾아두기

                if (playerHP != null)
                {
                    // ⭐️ 수정: 즉시 데미지 주던 코드를 지우고, 흔들림 코루틴 실행!
                    StartCoroutine(ShakeAndAttack(playerHP, playerAudio));
                }
            }
        }
    }

    // ⭐️ 3번 미션: 잠깐 멈춰서 부들부들 흔들리고 데미지를 주는 코루틴
    IEnumerator ShakeAndAttack(PlayerHealth playerHP, AudioSource playerAudio)
    {
        isAttacking = true; // 스위치 ON! (이동 멈춤, 타이머 멈춤)
        attackTimer = 0f;   // 쿨타임은 미리 초기화

        Vector3 originalPos = transform.position;

        // 5번 동안 랜덤한 위치로 살짝씩 튕기기
        for (int i = 0; i < 5; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
            yield return new WaitForSeconds(0.05f); // 0.05초 대기
        }

        // 원래 위치로 복구
        transform.position = originalPos;

        // 흔들림 연출이 끝난 후 진짜 데미지 넣기!
        playerHP.TakeDamage(attackDamage);

        // 오디오도 데미지가 들어가는 이 타이밍에 재생!
        if (playerAudio != null)
        {
            playerAudio.Play();
        }

        isAttacking = false; // 스위치 OFF! (다시 쫓아가기 시작)
    }
}
