using UnityEngine;

// 하트 아이템 — 플레이어 체력 1칸 회복 (HP full일 때는 LevelGenerator에서 다른 아이템으로 대체)
[RequireComponent(typeof(Collider2D))]
public class HeartItem : MonoBehaviour
{
    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) player.HealOne();
        Destroy(gameObject);
    }
}
