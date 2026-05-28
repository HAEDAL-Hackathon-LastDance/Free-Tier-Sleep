using UnityEngine;
using System.Collections;
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
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        Time.timeScale = 1.0f;
        currentHP = maxHP;
        UpdateHPUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Max(0, currentHP - damage);
        UpdateHPUI();
        Debug.Log("Ouch! Remaining HP: " + currentHP);

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHP <= 0)
            Die();
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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}
