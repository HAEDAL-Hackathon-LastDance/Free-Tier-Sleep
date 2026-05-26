using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera mainCamera;
    
    [Header("Generation Settings")]
    public float targetAltitude = 1000f; // 최종 목표 고도
    public float spawnYThreshold = 15f;  // 플레이어 기준 위로 얼마만큼 미리 생성할지
    public float minXClamp = -7f;        // 화면 좌측 한계선
    public float maxXClamp = 7f;         // 화면 우측 한계선

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;
    
    // 활성화된 발판을 추적하여 카메라 아래로 가면 회수하기 위한 큐
    private Queue<GameObject> activePlatforms = new Queue<GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 초기 시작 발판 위치 설정 (플레이어 시작 위치 근처)
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

        // 플레이어가 목표 고도에 도달했는지 체크
        if (player.position.y >= targetAltitude)
        {
            isLevelComplete = true;
            Debug.Log("목표 고도 도달! 클리어 컷신으로 전환 필요.");
            // TODO: 플레이어 조작 잠금 및 컷신 전환 로직 호출
            return;
        }

        // 플레이어의 Y좌표를 기준으로 일정 높이(spawnYThreshold) 이내에 발판이 부족하면 추가 생성
        if (player.position.y + spawnYThreshold > lastPlatformPos.y)
        {
            SpawnNextPlatform();
        }

        // 카메라 아래로 지나친 발판 회수 (메모리 최적화)
        DespawnOldPlatforms();
    }

    private void SpawnNextPlatform()
    {
        // 난이도 조절: 고도가 높아질수록 발판 사이의 Y축 거리가 멀어짐 (최대 5f까지 증가)
        float progressRatio = Mathf.Clamp01(player.position.y / targetAltitude);
        float currentMinY = Mathf.Lerp(2.0f, 3.5f, progressRatio);
        float currentMaxY = Mathf.Lerp(3.5f, 5.0f, progressRatio);

        // 기획서 문제점 1 해결: 이전 발판(lastPlatformPos) 기준으로 타이트한 난수 적용
        float randomXOffset = Random.Range(-4f, 4f);
        float randomYOffset = Random.Range(currentMinY, currentMaxY);

        float nextX = lastPlatformPos.x + randomXOffset;
        float nextY = lastPlatformPos.y + randomYOffset;

        // X축이 화면 밖으로 무한정 나가지 않도록 Clamp 처리
        nextX = Mathf.Clamp(nextX, minXClamp, maxXClamp);

        lastPlatformPos = new Vector2(nextX, nextY);

        // ObjectPooler를 이용해 발판 스폰
        GameObject platform = ObjectPooler.Instance.SpawnFromPool("Platform", lastPlatformPos, Quaternion.identity);
        
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

            // 데드락 방지: 발판이 다른 스크립트(예: 시한부 팝업)에 의해 이미 파괴되거나 null이 된 경우
            if (oldestPlatform == null)
            {
                activePlatforms.Dequeue();
                continue;
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
