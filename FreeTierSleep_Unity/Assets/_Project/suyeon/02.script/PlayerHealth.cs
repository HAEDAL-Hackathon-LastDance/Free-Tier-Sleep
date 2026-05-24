
using UnityEngine;
using TMPro; // TextMeshPro UI를 조작하기 위한 라이브러리
using UnityEngine.SceneManagement; // 씬을 다시 불러오기 위한 라이브러리

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 50;
    private int currentHP;

    [Header("UI References")]
    public TextMeshProUGUI hpText;        // 화면에 띄울 체력 글자
    public GameObject gameOverPanel;      // 껐다 켤 게임오버 화면

    void Start()
    {
        // 게임 시작 시 초기화 
        Time.timeScale = 1.0f;            // 시간이 정상적으로 흐르게 세팅
        currentHP = maxHP;
        UpdateHPUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // 처음엔 게임오버 창 숨기기
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return; // 이미 체력이 0이면 무시

        currentHP -= damage;
        UpdateHPUI();
        Debug.Log("Ouch! Remaining HP: " + currentHP);

        // 체력 0 도달 시 Game Over 처리
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHPUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP;
    }

    void Die()
    {
        Debug.Log("게임 오버!");
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true); // 게임오버 창 띄우기

        Time.timeScale = 0f; // 시간(물리 연산)을 완전히 멈춤
    }

    // 재시작 버튼 구현
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}