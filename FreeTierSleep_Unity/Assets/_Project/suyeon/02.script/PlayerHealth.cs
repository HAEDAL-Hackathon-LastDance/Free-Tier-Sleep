using UnityEngine;
using System.Collections; // ⭐️ 코루틴(IEnumerator)을 쓰기 위해 꼭 추가!
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 50;
    private int currentHP;

    [Header("UI References")]
    public TextMeshProUGUI hpText;
    public GameObject gameOverPanel;

    // ⭐️ 추가: 플레이어의 색깔을 바꿀 컴포넌트
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        Time.timeScale = 1.0f;
        currentHP = maxHP;
        UpdateHPUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ⭐️ 보너스: 만약 유니티에서 실수로 SpriteRenderer를 안 끌어다 놔도, 자기가 알아서 찾게 만드는 안전장치!
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return; // 이미 체력이 0이면 무시

        currentHP -= damage;

        // ⭐️ 1차 추가: 체력이 마이너스가 되면 0으로 고정! (음수 방지)
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        UpdateHPUI();
        Debug.Log("Ouch! Remaining HP: " + currentHP);

        // ⭐️ 바로 여기! 데미지를 입었을 때 화면 깜빡임 실행!
        StartCoroutine(FlashRed());

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
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // 재시작 버튼 구현
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ⭐️ 추가: 빨갛게 변했다가 돌아오는 마법의 함수
    // ⭐️ 수정: 두 번 연속으로 빨갛게 깜빡이는 마법의 함수!
    IEnumerator FlashRed()
    {
        // --- 첫 번째 깜빡임 ---
        spriteRenderer.color = Color.red;       // 빨갛게!
        yield return new WaitForSeconds(0.1f);  // 0.1초 대기
        spriteRenderer.color = Color.white;     // 원래 색으로!

        yield return new WaitForSeconds(0.1f);  // 0.1초 대기 (이게 있어야 깜빡이는 게 보여!)

        // --- 두 번째 깜빡임 ---
        spriteRenderer.color = Color.red;       // 다시 빨갛게!
        yield return new WaitForSeconds(0.1f);  // 0.1초 대기
        spriteRenderer.color = Color.white;     // 완전 원래 색으로 복구!
    }
}