using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Camera mainCamera;
    public RisingDataFlood dataFlood; // 글리치(DataFlood) 참조 추가

    [Header("Generation Settings")]
    public float targetAltitude = 1000f; // 최종 목표 고도
    public float spawnYThreshold = 15f;  // 카메라 기준 위로 얼마만큼 미리 생성할지
    public float minXClamp = -7f;        // 화면 좌측 한계선
    public float maxXClamp = 7f;         // 화면 우측 한계선
    public float despawnMarginY = 15f;   // 카메라 아래로 발판이 사라지기까지의 여유 공간

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;
    private Queue<GameObject> activePlatforms = new Queue<GameObject>();

    // Start를 코루틴으로 변경하여 ObjectPooler 초기화를 기다림
    IEnumerator Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        lastPlatformPos = new Vector2(0f, -2f);

        // ObjectPooler의 Start()가 먼저 실행되어 풀이 준비될 수 있도록 한 프레임 대기
        yield return null;

        // 시작 시 기본 발판 5개 미리 생성
        for (int i = 0; i < 5; i++)
        {
            SpawnNextPlatform();
        }
    }

    void Update()
    {
        if (isLevelComplete) return;

        if (cameraTransform.position.y >= targetAltitude)
        {
            isLevelComplete = true;
            return;
        }

        // 봇의 제안 반영: 하드코딩 10f -> spawnYThreshold 변수 적용
        if (cameraTransform.position.y + spawnYThreshold > lastPlatformPos.y)
        {
            SpawnNextPlatform();
        }
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
        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize - 2f;

        // 봇의 제안 반영: 파도의 상단 살점을 정확히 계산하기 위해 Collider2D bounds 이용
        float floodTopY = float.MinValue;
        GameObject dataFlood = GameObject.FindWithTag("DataFlood");
        if (dataFlood != null)
        {
            Collider2D floodCollider = dataFlood.GetComponent<Collider2D>();
            if (floodCollider != null)
            {
                // 파도의 가장 윗면 좌표를 기준점으로 삼음
                floodTopY = floodCollider.bounds.max.y;
            }
        }

        // 카메라 하단과 파도 상단 중 더 높은 곳을 기준선으로 설정
        float finalDespawnLine = Mathf.Max(cameraBottomY, floodTopY);

        while (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms.Peek();

            if (oldestPlatform == null || !oldestPlatform.activeInHierarchy)
            {
                activePlatforms.Dequeue();
                continue;
            }

            // 최적화: 최종 기준선보다 아래에 있는 발판은 가차 없이 풀로 회수
            if (oldestPlatform.transform.position.y < finalDespawnLine)
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
