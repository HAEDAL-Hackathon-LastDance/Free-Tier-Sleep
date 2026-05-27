using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Camera mainCamera;
    
    [Header("Generation Settings")]
    public float targetAltitude = 1000f; // 최종 목표 고도
    public float spawnYThreshold = 15f;  // 카메라 기준 위로 얼마만큼 미리 생성할지 (기본값 유지, 조건문에서는 10f 사용)
    public float minXClamp = -7f;        // 화면 좌측 한계선
    public float maxXClamp = 7f;         // 화면 우측 한계선

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;
    
    // 활성화된 발판을 추적하여 카메라 아래로 가면 회수하기 위한 큐
    private Queue<GameObject> activePlatforms = new Queue<GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 초기 시작 발판 위치 설정
        lastPlatformPos = new Vector2(0f, -2f);
        
        // 시작 시 기본 발판 몇 개 미리 생성
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
            // TODO: 조작 잠금 및 컷신 전환 로직 호출
            return;
        }

        // 카메라의 Y좌표를 기준으로 10f 이내에 발판이 부족하면 추가 생성
        if (cameraTransform.position.y + 10f > lastPlatformPos.y)
        {
            SpawnNextPlatform();
        }

        // 카메라 아래로 지나친 발판 회수 (메모리 최적화)
        DespawnOldPlatforms();
    }

    private void SpawnNextPlatform()
    {
        // 난이도 조절: 고도가 높아질수록 발판 사이의 Y축 거리가 멀어짐
        float progressRatio = Mathf.Clamp01(cameraTransform.position.y / targetAltitude);
        
        // 1. Y축 간격 대폭 확대 (시작값 3.5f, 최대 6.0f 기반으로 조정)
        float currentMinY = Mathf.Lerp(3.5f, 5.0f, progressRatio);
        float currentMaxY = Mathf.Lerp(6.0f, 7.5f, progressRatio);

        // 2. 강제 지그재그(Zig-Zag) 패턴 적용
        float nextX;
        if (lastPlatformPos.x < 0)
        {
            // 이전 발판이 왼쪽이면 다음은 오른쪽으로
            nextX = Random.Range(2f, maxXClamp);
        }
        else
        {
            // 이전 발판이 오른쪽(또는 중앙)이면 다음은 왼쪽으로
            nextX = Random.Range(minXClamp, -2f);
        }

        float randomYOffset = Random.Range(currentMinY, currentMaxY);
        float nextY = lastPlatformPos.y + randomYOffset;

        // 3. X축이 화면 밖으로 무한정 나가지 않도록 Clamp 처리 (기존 유지)
        nextX = Mathf.Clamp(nextX, minXClamp, maxXClamp);

        // ObjectPooler를 이용해 발판 스폰 (새로운 위치 파라미터 전달)
        GameObject platform = ObjectPooler.Instance.SpawnFromPool("Platform", new Vector2(nextX, nextY), Quaternion.identity);
        
        // 스폰 직후 기준점을 최신으로 갱신
        lastPlatformPos = new Vector2(nextX, nextY);
        
        if (platform != null)
        {
            activePlatforms.Enqueue(platform);
        }
        else
        {
            Debug.LogWarning("Platform 스폰 실패. ObjectPooler 설정을 확인하세요.");
        }
    }

    private void DespawnOldPlatforms()
    {
        // 카메라 하단 경계 계산 (여유분 2f 추가)
        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize - 2f;

        // while 루프를 통해 한 프레임에 여러 개의 발판을 동시에 수거 가능하도록 처리
        while (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms.Peek();

            // [방어 로직] 발판이 파괴되었거나, 기믹(PlatformTimer)에 의해 이미 비활성화(풀 반환)된 경우
            if (oldestPlatform == null || !oldestPlatform.activeInHierarchy)
            {
                activePlatforms.Dequeue();
                continue; // 풀러를 중복 호출하지 않고 다음 큐 요소로 넘어감
            }

            // 큐의 가장 오래된 발판이 카메라 하단보다 아래에 있는지 확인
            if (oldestPlatform.transform.position.y < cameraBottomY)
            {
                activePlatforms.Dequeue();
                ObjectPooler.Instance.ReturnToPool("Platform", oldestPlatform);
            }
            else
            {
                // 가장 오래된 발판이 아직 화면 안에 있다면 루프 종료 (나머지도 화면 안에 있음)
                break;
            }
        }
    }
}
