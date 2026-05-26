using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Wave Settings")]
    // ?? GameObject 하나가 아니라 배열[]로 변경! 여기에 에너미 3종류를 넣을 거야.
    public GameObject[] enemyPrefabs;
    public Transform coreTransform;

    [Header("Spawn Variables")]
    public float spawnDelay = 2.0f;
    public float minSpawnDelay = 0.5f;
    public float delayDecreaseRate = 0.2f;
    public int enemiesPerWave = 5;

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

        // ?? 0, 1, 2번 배열 중 랜덤으로 하나를 선택해
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedPrefab = enemyPrefabs[randomIndex];

        Vector2 randomPos = new Vector2(Random.Range(-10f, 10f), Random.Range(-6f, 6f));

        // 선택된 프리팹으로 스폰!
        GameObject newEnemy = Instantiate(selectedPrefab, randomPos, Quaternion.identity);

        newEnemy.GetComponent<StandardEnemy>().coreTransform = coreTransform;
        enemyCount++;
    }
}
