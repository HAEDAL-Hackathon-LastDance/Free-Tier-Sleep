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
            isTriggered = true;
            StartCoroutine(DecayRoutine());
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
            spriteRenderer.color = isRed ? Color.white : Color.red;
            isRed = !isRed;
            
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 시간이 다 되면 풀로 반환
        ObjectPooler.Instance.ReturnToPool("Platform", gameObject);
    }
}
