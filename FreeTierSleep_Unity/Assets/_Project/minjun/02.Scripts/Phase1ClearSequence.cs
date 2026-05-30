using System.Collections;
using UnityEngine;

// Phase 1 클리어 연출 총괄 (트리거 전용)
// - 진행도 100% 도달 시 ProgressBarUI가 TriggerClear() 호출
// - 상태 텍스트 교체 → 0.9초 후 씬 동결 → GameOverFreezePopup.Show() 위임
// - 팝업 UI/애니메이션은 GameOverFreezePopup이 담당
public class Phase1ClearSequence : MonoBehaviour
{
    public static Phase1ClearSequence Instance { get; private set; }
    public static bool IsCleared { get; private set; }

    private void Awake()
    {
        Instance = this;
        IsCleared = false;
        Time.timeScale = 1f;
    }

    public void TriggerClear()
    {
        if (IsCleared) return;
        IsCleared = true;
        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        // ── 0.3초 후: 상태 텍스트 교체 ──
        yield return new WaitForSecondsRealtime(0.3f);

        ProgressBarUI bar = Object.FindFirstObjectByType<ProgressBarUI>();
        if (bar != null) bar.SetClearStatusText("경계 접속 중...");

        // ── 추가 0.9초 후: 씬 동결 + 팝업 표시 ──
        yield return new WaitForSecondsRealtime(0.9f);

        Time.timeScale = 0f;
        GameOverFreezePopup.Instance?.Show();
    }
}
