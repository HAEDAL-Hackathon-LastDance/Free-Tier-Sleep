using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    
    [Header("Generation Settings")]
    public float targetAltitude = 1000f; // 최종 목표 고도
    public float spawnYThreshold = 15f;  // 플레이어 기준 위로 얼마만큼 미리 생성할지
    public float minXClamp = -7f;        // 화면 좌측 한계선
    public float maxXClamp = 7f;         // 화면 우측 한계선

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;

    void Start()
    {
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
    }

    private void SpawnNextPlatform()
    {
        // 기획서 문제점 1 해결: 이전 발판(lastPlatformPos) 기준으로 타이트한 난수 적용
        float randomXOffset = Random.Range(-4f, 4f);
        float randomYOffset = Random.Range(2f, 3.5f);

        float nextX = lastPlatformPos.x + randomXOffset;
        float nextY = lastPlatformPos.y + randomYOffset;

        // X축이 화면 밖으로 무한정 나가지 않도록 Clamp 처리
        nextX = Mathf.Clamp(nextX, minXClamp, maxXClamp);

        lastPlatformPos = new Vector2(nextX, nextY);

        // ObjectPooler를 이용해 발판 스폰 (태그는 "Platform"으로 가정)
        // 팝업창 발판 프리팹이 풀에 등록되어 있어야 합니다.
        GameObject platform = ObjectPooler.Instance.SpawnFromPool("Platform", lastPlatformPos, Quaternion.identity);
        
        if (platform == null)
        {
            Debug.LogWarning("Platform 스폰 실패. ObjectPooler 설정을 확인하세요.");
        }
    }
}
