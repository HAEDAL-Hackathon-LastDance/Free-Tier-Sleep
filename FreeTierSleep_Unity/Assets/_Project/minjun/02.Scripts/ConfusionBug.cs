using System.Collections;
using UnityEngine;

// 혼란 팝업 (Confusion Bug)
// - 허공의 특정 위치에 정지해 있는 홀로그램형 트리거
// - 플레이어 접촉 시 일정 시간 동안 좌우 입력을 반전시킴 (PlayerController.ApplyInputReverse)
// - 연속 트리거 방지를 위한 짧은 쿨다운 적용
[RequireComponent(typeof(Collider2D))]
public class ConfusionBug : MonoBehaviour
{
    [Header("Confusion Settings")]
    [Tooltip("조작 반전 지속 시간 (초)")]
    public float confusionDuration = 2f;

    [Tooltip("한 번 트리거된 후 다시 발동할 수 있을 때까지의 쿨다운")]
    public float retriggerCooldown = 3f;

    private Collider2D col;
    private bool isOnCooldown = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        // 홀로그램이므로 물리 충돌이 아닌 트리거로 동작
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        isOnCooldown = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (FreezeManager.IsFrozen) return; // 프리즈 중에는 효과 무시
        if (isOnCooldown) return;
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !player.IsInvincible)
        {
            player.ApplyInputReverse(confusionDuration);
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(retriggerCooldown);
        isOnCooldown = false;
    }
}
