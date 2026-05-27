
using UnityEngine;
using TMPro; // TextMeshPro UI�� �����ϱ� ���� ���̺귯��
using UnityEngine.SceneManagement; // ���� �ٽ� �ҷ����� ���� ���̺귯��

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 50;
    private int currentHP;

    [Header("UI References")]
    public TextMeshProUGUI hpText;        // ȭ�鿡 ��� ü�� ����
    public GameObject gameOverPanel;      // ���� �� ���ӿ��� ȭ��

    void Start()
    {
        // ���� ���� �� �ʱ�ȭ 
        Time.timeScale = 1.0f;            // �ð��� ���������� �帣�� ����
        currentHP = maxHP;
        UpdateHPUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // ó���� ���ӿ��� â �����
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return; // �̹� ü���� 0�̸� ����

        currentHP -= damage;
        UpdateHPUI();
        Debug.Log("Ouch! Remaining HP: " + currentHP);

        // ü�� 0 ���� �� Game Over ó��
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
        Debug.Log("���� ����!");
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true); // ���ӿ��� â ����

        Time.timeScale = 0f; // �ð�(���� ����)�� ������ ����
    }

    // ����� ��ư ����
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}