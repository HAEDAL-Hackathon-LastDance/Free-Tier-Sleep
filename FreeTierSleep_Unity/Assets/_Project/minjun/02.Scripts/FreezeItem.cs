using UnityEngine;

// 프리즈 아이템 — 일정 시간 동안 모든 적/발판의 동작을 정지시키고 화면을 파랗게 덮음
[RequireComponent(typeof(Collider2D))]
public class FreezeItem : MonoBehaviour
{
    [Tooltip("프리즈 지속 시간 (초)")]
    public float freezeDuration = 5f;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        FreezeManager.Instance.StartFreeze(freezeDuration);
        Destroy(gameObject);
    }
}
