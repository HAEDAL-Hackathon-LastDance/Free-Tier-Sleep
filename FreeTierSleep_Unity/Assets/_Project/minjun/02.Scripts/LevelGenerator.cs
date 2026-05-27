using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Camera mainCamera;
    
    [Header("Generation Settings")]
    public float targetAltitude = 1000f; // 최종 목표 고도
    public float spawnYThreshold = 15f;  // 카메라 기준 위로 얼마만큼 미리 생성할지
    public float minXClamp = -7f;        // 화면 좌측 한계선
    public float maxXClamp = 7f;         // 화면 우측 한계선

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;
    private Queue<GameObject> activePlatforms = new Queue<GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        lastPlatformPos = new Vector2(0f, -2f);
        
        // 시작 시 기본 발판 5개 미리 생성
        for (int i = 0; i < 5; i++)
        {
            SpawnNextPlatform();
        }
    }

    void Update()
    {
        if (isLevelComplete) return;

        // 목표 고도에 도달했는지 체크
        if (cameraTransform.position.y >= targetAltitude)
        {
            isLevelComplete = true;
            Debug.Log("목표 고도 도달! 클리어 컷신으로 전환 필요.");
            return;
        }

        // 카메라의 Y좌표를 기준으로 발판이 부족하면 추가 생성
        if (cameraTransform.position.y + 10f > lastPlatformPos.y)
        {
            SpawnNextPlatform();
        }

        // 카메라 아래로 지나친 발판 자동 회수 (메모리 최적화)
        DespawnOldPlatforms();
    }

    private void SpawnNextPlatform()
    {
        // 1. Y축 간격 대폭 확대 (팝업창 크기를 고려하여 위아래 겹침 절대 방지)
        float progressRatio = Mathf.Clamp01(cameraTransform.position.y / targetAltitude);
        float currentMinY = Mathf.Lerp(3.5f, 4.5f, progressRatio);
        float currentMaxY = Mathf.Lerp(4.5f, 6.0f, progressRatio);

        // 2. 강제 지그재그(Zig-Zag) 패턴 적용 (제자리 점프 노가다 완벽 차단)
        float nextX;
        if (lastPlatformPos.x < 0)
        {
            // 이전 발판이 왼쪽(음수)에 있었다면 다음은 무조건 오른쪽(양수) 영역에 스폰
            nextX = Random.Range(2f, maxXClamp);
        }
        else
        {
            // 이전 발판이 오른쪽(양수)에 있었다면 다음은 무조건 왼쪽(음수) 영역에 스폰
            nextX = Random.Range(minXClamp, -2f);
        }

        float randomYOffset = Random.Range(currentMinY, currentMaxY);
        float nextY = lastPlatformPos.y + randomYOffset;

        // 3. X축 화면 이탈 방지 경계 제한
        nextX = Mathf.Clamp(nextX, minXClamp, maxXClamp);

        // 4. [핵심] 렉 유발하는 Instantiate 대신 기존 ObjectPooler 시스템과 완벽 연동
        GameObject platform = ObjectPooler.Instance.SpawnFromPool("Platform", new Vector2(nextX, nextY), Quaternion.identity);
        lastPlatformPos = new Vector2(nextX, nextY);
        
        if (platform != null)
        {
            activePlatforms.Enqueue(platform);
        }
    }

    private void DespawnOldPlatforms()
    {
        // 카메라 하단 경계 계산
        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize - 2f;

        while (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms.Peek();

            // 이미 기믹에 의해 꺼진 발판 예외 처리
            if (oldestPlatform == null || !oldestPlatform.activeInHierarchy)
            {
                activePlatforms.Dequeue();
                continue;
            }

            // 화면 아래로 완전히 벗어난 발판만 쏙 빼서 풀로 재반환
            if (oldestPlatform.transform.position.y < cameraBottomY)
            {
                activePlatforms.Dequeue();
                ObjectPooler.Instance.ReturnToPool("Platform", oldestPlatform);
            }
            else
            {
                break;
            }
        }
    }
}