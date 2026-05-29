using UnityEngine;
using System.Collections;
using TMPro;
// 씬 전환 라이브러리(SceneManagement)는 엔딩 씬 안 쓰니까 뺐습니다!

public class GameManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform coreTransform;

    [Header("Spawn Variables")]
    public float spawnDelay = 2.0f;
    public float minSpawnDelay = 0.8f;
    public float delayDecreaseRate = 0.15f;
    public int enemiesPerWave = 7;

    public float spawnPadding = 2.0f;

    [Header("Wave Status")]
    public int currentWave = 1;
    public int enemyCount = 0;

    [Header("Player Speed Decay")]
    public Player_Movement playerMovement;
    public float speedDecreasePerWave = 0.2f;
    public float minPlayerSpeed = 1.0f;

    [Header("Timer & Clear Settings")]
    public TextMeshProUGUI timerText; // AM 03:00을 띄울 텍스트
    public GameObject clearPanel;     // "Clear!" 글자가 적힌 화면 패널
    private float elapsedTime = 0f;
    private float totalTime = 180f;   // 180초 (3분)
    private bool isCleared = false;

    void Start()
    {
        // 시작할 때 클리어 화면 숨겨두기
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }

        StartCoroutine(WaveRoutine());
    }

    void Update()
    {
        // 클리어 상태면 시간 흐르는 것도 멈춤!
        if (isCleared) return;

        elapsedTime += Time.deltaTime;

        int startHour = 3;
        int currentMinute = Mathf.FloorToInt(elapsedTime);

        int displayHour = startHour + (currentMinute / 60);
        int displayMinute = currentMinute % 60;

        if (timerText != null)
        {
            timerText.text = string.Format("AM {0:D2}:{1:D2}", displayHour, displayMinute);
        }

        // 180초(AM 06:00) 달성 시 클리어 함수 실행!
        if (elapsedTime >= totalTime)
        {
            ClearGame();
        }
    }

    IEnumerator WaveRoutine()
    {
        // ⭐️ 클리어 상태가 아닐 때만 계속 스폰하도록 수정!
        while (!isCleared)
        {

            for (int i = 0; i < enemiesPerWave; i++)
            {
                // 스폰 도중에 클리어 시간이 되면 즉시 스폰 멈춤!
                if (isCleared) yield break;

                SpawnEnemy();
                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitForSeconds(3.0f);

            currentWave++;
            enemiesPerWave += 5;

            if (spawnDelay > minSpawnDelay)
            {
                spawnDelay -= delayDecreaseRate;
                if (spawnDelay < minSpawnDelay) spawnDelay = minSpawnDelay;
            }

            // 웨이브마다 플레이어 이동 속도 점감
            if (playerMovement != null)
            {
                playerMovement.speed = Mathf.Max(
                    minPlayerSpeed,
                    playerMovement.speed - speedDecreasePerWave
                );
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedPrefab = enemyPrefabs[randomIndex];

        float minX = -10f;
        float maxX = 10f;
        float minY = -6f;
        float maxY = 6f;

        Vector2 spawnPosition = Vector2.zero;
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                spawnPosition.x = Random.Range(minX, maxX);
                spawnPosition.y = maxY + spawnPadding;
                break;
            case 1:
                spawnPosition.x = Random.Range(minX, maxX);
                spawnPosition.y = minY - spawnPadding;
                break;
            case 2:
                spawnPosition.x = minX - spawnPadding;
                spawnPosition.y = Random.Range(minY, maxY);
                break;
            case 3:
                spawnPosition.x = maxX + spawnPadding;
                spawnPosition.y = Random.Range(minY, maxY);
                break;
        }

        // 계산된 화면 밖 좌표에서 스폰
        GameObject newEnemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        if (newEnemy.TryGetComponent<StandardEnemy>(out var enemy))
            enemy.coreTransform = coreTransform;

        enemyCount++;
    }

    // ⭐️ 씬 전환 싹 빼고 패널만 띄우는 클리어 함수!
    void ClearGame()
    {
        isCleared = true;

        // 1. 씬에 있는 모든 몬스터 삭제
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // 2. Clear! 화면 띄우기 (여기서 게임은 평화롭게 정지됨!)
        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
    }
}