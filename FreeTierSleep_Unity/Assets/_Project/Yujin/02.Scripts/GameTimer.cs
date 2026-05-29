using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] private float totalTime = 120f; // AM 03:00 → 05:00 = 120초 (데모용)

    [SerializeField] private string clearSceneName = "EndingScene";

    private float elapsed = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        elapsed += Time.deltaTime;
        UpdateTimerUI();

        if (elapsed >= totalTime)
            OnClear();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        // AM 03:00 기준으로 경과 시간 더하기
        int totalSeconds = 3 * 3600 + (int)elapsed;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        timerText.text = $"AM {hours:D2}:{minutes:D2}";
    }

    void OnClear()
    {
        isRunning = false;

        // 화면에 남은 모든 적 소멸
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        // 엔딩 씬 전환 (씬 이름 확정되면 주석 해제)
        // SceneManager.LoadScene(clearSceneName);
    }

    public void StopTimer() => isRunning = false;
    public void StartTimer() => isRunning = true;
}
