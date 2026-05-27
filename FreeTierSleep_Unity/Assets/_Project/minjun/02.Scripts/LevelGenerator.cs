using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float minXClamp = -8f;
    public float maxXClamp = 8f;
    public float yOffset = 3f; // 다음 발판의 높이 간격

    private Vector3 lastPlatformPos = Vector3.zero;

    public void SpawnNextPlatform()
    {
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

        // 다음 발판의 위치 계산 (Y축으로 일정 간격 상승)
        Vector3 nextPosition = new Vector3(nextX, lastPlatformPos.y + yOffset, 0f);
        
        // TODO: 실제 프리팹 생성(Instantiate) 로직이 들어갈 자리
        // 예시: Instantiate(platformPrefab, nextPosition, Quaternion.identity);

        // 다음 스폰을 위해 마지막 위치 갱신
        lastPlatformPos = nextPosition;
    }
}
