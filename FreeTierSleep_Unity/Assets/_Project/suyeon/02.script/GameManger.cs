using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform coreTransform;

    [Header("Spawn Variables")]
    public float spawnDelay = 2.0f;
    public float minSpawnDelay = 0.5f;
    public float delayDecreaseRate = 0.2f;
    public int enemiesPerWave = 5;

    // ?? 새로 추가된 여백 변수! (값이 클수록 화면에서 더 멀리서 생성됨)
    public float spawnPadding = 2.0f;

    [Header("Wave Status")]
    public int currentWave = 1;
    public int enemyCount = 0;

    void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (true)
        {
            Debug.Log($"[Wave {currentWave}] 시작! 스폰 딜레이: {spawnDelay}초");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnDelay);
            }

            Debug.Log($"[Wave {currentWave}] 종료. 3초 대기...");
            yield return new WaitForSeconds(3.0f);

            currentWave++;
            enemiesPerWave += 3;

            if (spawnDelay > minSpawnDelay)
            {
                spawnDelay -= delayDecreaseRate;
                if (spawnDelay < minSpawnDelay) spawnDelay = minSpawnDelay;
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 랜덤 에너미 선택
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedPrefab = enemyPrefabs[randomIndex];

        // ?? 유니티 카메라 오류 방지! 네가 원래 썼던 화면 크기(-10~10, -6~6)를 기준으로 화면 밖 고정!
        float minX = -10f;
        float maxX = 10f;
        float minY = -6f;
        float maxY = 6f;

        Vector2 spawnPosition = Vector2.zero;
        int side = Random.Range(0, 4); // 상, 하, 좌, 우 중 랜덤

        switch (side)
        {
            case 0: // 상 (화면 맨 위보다 더 위에서)
                spawnPosition.x = Random.Range(minX, maxX);
                spawnPosition.y = maxY + spawnPadding;
                break;
            case 1: // 하 (화면 맨 아래보다 더 아래에서)
                spawnPosition.x = Random.Range(minX, maxX);
                spawnPosition.y = minY - spawnPadding;
                break;
            case 2: // 좌 (화면 맨 왼쪽보다 더 왼쪽에서)
                spawnPosition.x = minX - spawnPadding;
                spawnPosition.y = Random.Range(minY, maxY);
                break;
            case 3: // 우 (화면 맨 오른쪽보다 더 오른쪽에서)
                spawnPosition.x = maxX + spawnPadding;
                spawnPosition.y = Random.Range(minY, maxY);
                break;
        }

        // ?? 확실하게 계산된 화면 밖 좌표(spawnPosition)로 스폰!
        GameObject newEnemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        newEnemy.GetComponent<StandardEnemy>().coreTransform = coreTransform;
        enemyCount++;
    }
}

