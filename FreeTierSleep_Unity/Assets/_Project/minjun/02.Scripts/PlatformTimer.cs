using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformTimer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isTriggered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // 플레이어가 밟았고, 아직 트리거되지 않았다면 타이머 시작
        if (!isTriggered && collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 발판 위에서 아래로 떨어지며 밟았는지(발이 닿았는지) 확인
            // 플레이어의 중심 Y좌표가 발판의 중심 Y좌표보다 높을 때만 인정
            if (collision.transform.position.y > transform.position.y)
            {
                isTriggered = true;
                StartCoroutine(DecayRoutine());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // DataFlood와 닿으면 즉시 풀로 반환
        if (other.CompareTag("DataFlood"))
        {
            ObjectPooler.Instance.ReturnToPool("Platform", gameObject);
        }
    }

    private IEnumerator DecayRoutine()
    {
        float duration = 1.5f;
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
