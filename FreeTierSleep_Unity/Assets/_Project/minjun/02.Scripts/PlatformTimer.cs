using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlatformTimer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isTriggered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    // 풀에서 꺼내질 때(활성화) 상태 초기화
    private void OnEnable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        isTriggered = false;
    }

    // 풀로 반환될 때(비활성화) 진행 중인 코루틴 강제 종료
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerTimer(collision);
    }

    // 플레이어가 발판 위에 머물고 있을 때도 체크하여 확실하게 작동하도록 보장
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryTriggerTimer(collision);
    }

    private void TryTriggerTimer(Collision2D collision)
    {
        // 플레이어와 충돌했고, 아직 트리거되지 않았다면
        if (!isTriggered && collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            // 1. 플레이어가 위로 상승 중일 때는 무시 (원웨이 플랫폼 통과 중)
            if (playerRb != null && playerRb.linearVelocity.y <= 0.05f)
            {
                // 2. 플레이어의 발바닥이 발판의 윗면보다 확실히 위에 있을 때만 인정
                float playerBottom = collision.collider.bounds.min.y;
                float platformTop = col.bounds.max.y;

                // 오차 범위를 고려하여 -0.15f 정도 여유를 둠
                if (playerBottom >= platformTop - 0.15f)
                {
                    // 3. 충돌 지점의 법선(Normal) 벡터를 확인하여 위에서 아래로 밟았는지 최종 체크
                    if (collision.contacts.Length > 0 && collision.contacts[0].normal.y < -0.5f)
                    {
                        isTriggered = true;
                        StartCoroutine(DecayRoutine());
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // DataFlood와 닿으면 즉시 풀로 반환
        if (other.CompareTag("DataFlood"))
        {
            StopAllCoroutines();
            // 발판 스크립트가 붙은 최상위 GameObject를 풀로 안전하게 반환
            ObjectPooler.Instance.ReturnToPool("Platform", this.gameObject);
        }
    }

    private IEnumerator DecayRoutine()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        float blinkInterval = 0.15f;
        bool isRed = false;

        // 1.5초 동안 붉은색과 원래 색을 깜빡임
        while (elapsed < duration)
        {
            // 시간이 지날수록 깜빡이는 속도가 빨라지도록 연출 (긴장감 부여)
            if (elapsed > 1.0f) 
            {
                blinkInterval = 0.05f; // 매우 빠름
            }
            else if (elapsed > 0.5f) 
            {
                blinkInterval = 0.1f;  // 중간 빠름
            }

            spriteRenderer.color = isRed ? Color.white : Color.red;
            isRed = !isRed;
            
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 시간이 다 되면 풀로 반환 (파괴되어 플레이어가 떨어짐)
        ObjectPooler.Instance.ReturnToPool("Platform", gameObject);
    }
}
