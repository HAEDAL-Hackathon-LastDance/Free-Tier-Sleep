using System.Collections;
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

    [Header("Side Platform Settings")]
    [Tooltip("메인 발판과 같은 Y 부근에 추가 보조 발판이 생성될 확률 (0~1)")]
    [Range(0f, 1f)] public float sidePlatformChance = 0.7f;
    [Tooltip("메인 발판과 보조 발판이 X축으로 최소 떨어져야 하는 거리 (서로 점프 가능 범위)")]
    public float sidePlatformMinXGap = 4f;
    [Tooltip("보조 발판의 Y축 미세 오프셋 범위 (±값) — 0이면 정확히 같은 높이")]
    public float sidePlatformYJitter = 0.5f;

    [Header("Despawn Settings")]
    [Tooltip("카메라 하단에서 이만큼 더 아래로 벗어난 발판만 회수 — 이단점프 등 일시적 카메라 상승 시 발판 유지")]
    public float despawnMarginBelowCamera = 12f;

    [Header("Zone Distribution Settings")]
    [Tooltip("화면 X를 나누는 구역 수 — 클수록 발판이 고르게 분산됨 (권장 4)")]
    [Range(2, 6)] public int zoneCount = 4;

    [Header("Item Spawn Settings")]
    [Tooltip("발판 1개당 위쪽에 아이템이 생성될 확률 (0~1)")]
    [Range(0f, 1f)] public float itemSpawnChance = 0.12f;
    [Tooltip("발판 윗면에서 위로 띄울 거리")]
    public float itemSpawnYOffset = 2.8f;
    [Tooltip("하트 아이템 프리팹 (HP 회복)")]
    public GameObject heartItemPrefab;
    [Tooltip("프리즈 아이템 프리팹 (전체 정지)")]
    public GameObject freezeItemPrefab;
    [Tooltip("슈퍼점프 아이템 프리팹 (대점프 + 무적)")]
    public GameObject superJumpItemPrefab;

    // 3종 발판 풀 태그 — 씬의 ObjectPooler에 동일 이름으로 등록되어 있어야 함
    private static readonly string[] platformPoolTags =
    {
        "PopupPlatform1",
        "PopupPlatform2",
        "PopupPlatform3",
    };

    private Vector2 lastPlatformPos;
    private bool isLevelComplete = false;
    private readonly Queue<GameObject> activePlatforms = new Queue<GameObject>();
    private Collider2D cachedFloodCollider; // 매 프레임 FindWithTag 호출 방지
    private float cachedHalfCamHeight;       // orthographicSize 캐싱
    private int lastZoneIndex = -1;          // 직전 메인 발판의 구역 (-1: 초기값)
    private PlayerController cachedPlayer;   // HealOne 대상 HP 체크용

    // Start를 코루틴으로 변경하여 ObjectPooler 초기화를 기다림
    IEnumerator Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) cachedHalfCamHeight = mainCamera.orthographicSize;
        lastPlatformPos = new Vector2(0f, -2f);

        GameObject floodObj = GameObject.FindWithTag("DataFlood");
        if (floodObj != null) cachedFloodCollider = floodObj.GetComponent<Collider2D>();

        cachedPlayer = Object.FindFirstObjectByType<PlayerController>();

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
        // 1. Y축 간격
        float progressRatio = Mathf.Clamp01(cameraTransform.position.y / targetAltitude);
        float currentMinY = Mathf.Lerp(3.5f, 4.5f, progressRatio);
        float currentMaxY = Mathf.Lerp(4.5f, 6.0f, progressRatio);

        // 2. 구역 기반 X 배치 — 직전 구역 제외한 나머지 중 랜덤 선택
        int zoneIndex = PickZone(lastZoneIndex);
        float nextX = ZoneToX(zoneIndex);
        lastZoneIndex = zoneIndex;

        float nextY = lastPlatformPos.y + Random.Range(currentMinY, currentMaxY);

        // 3. ObjectPooler로 스폰 — 3종 발판 중 랜덤 선택
        string tag = platformPoolTags[Random.Range(0, platformPoolTags.Length)];
        GameObject platform = ObjectPooler.Instance.SpawnFromPool(tag, new Vector2(nextX, nextY), Quaternion.identity);
        lastPlatformPos = new Vector2(nextX, nextY);

        if (platform != null)
        {
            var timer = platform.GetComponent<PlatformTimer>();
            if (timer != null) timer.poolTag = tag;
            activePlatforms.Enqueue(platform);
            TrySpawnItemAbove(nextX, nextY);
        }

        // 4. 같은 Y 부근에 보조 발판 추가 — 메인 발판 구역과 다른 구역에 배치
        if (Random.value < sidePlatformChance)
        {
            SpawnSidePlatform(nextX, nextY, zoneIndex);
        }
    }

    // 발판 위쪽에 일정 확률로 아이템 생성. HP full이면 Heart는 다른 아이템으로 대체
    private void TrySpawnItemAbove(float platformX, float platformY)
    {
        if (Random.value >= itemSpawnChance) return;

        GameObject prefab = PickItemPrefab();
        if (prefab == null) return;

        Vector3 pos = new Vector3(platformX, platformY + itemSpawnYOffset, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }

    private GameObject PickItemPrefab()
    {
        // 참조가 끊긴 경우 재조회 (씬 로드 직후 타이밍 문제 방어)
        if (cachedPlayer == null)
            cachedPlayer = Object.FindFirstObjectByType<PlayerController>();

        int idx = Random.Range(0, 3);
        if (idx == 0)
        {
            // Heart: HP가 이미 만피면 다른 아이템으로 대체
            bool healthFull = cachedPlayer != null && cachedPlayer.currentHp >= cachedPlayer.maxHp;
            if (healthFull) return Random.value < 0.5f ? freezeItemPrefab : superJumpItemPrefab;
            return heartItemPrefab;
        }
        if (idx == 1) return freezeItemPrefab;
        return superJumpItemPrefab;
    }

    // 메인 발판 구역을 제외한 구역에 보조 발판 생성
    private void SpawnSidePlatform(float mainX, float mainY, int mainZoneIndex)
    {
        int sideZone = PickZone(mainZoneIndex);
        float sideX = ZoneToX(sideZone);

        // 구역이 인접해서 실제 X 간격이 너무 좁을 경우 스킵 (안전망)
        if (Mathf.Abs(sideX - mainX) < sidePlatformMinXGap) return;

        float sideY = mainY + Random.Range(-sidePlatformYJitter, sidePlatformYJitter);

        string tag = platformPoolTags[Random.Range(0, platformPoolTags.Length)];
        GameObject sidePlatform = ObjectPooler.Instance.SpawnFromPool(tag, new Vector2(sideX, sideY), Quaternion.identity);
        if (sidePlatform != null)
        {
            var timer = sidePlatform.GetComponent<PlatformTimer>();
            if (timer != null) timer.poolTag = tag;
            activePlatforms.Enqueue(sidePlatform);
        }
    }

    // excludeZone을 제외한 구역 중 랜덤 선택
    private int PickZone(int excludeZone = -1)
    {
        if (zoneCount <= 1) return 0;
        int picked;
        do { picked = Random.Range(0, zoneCount); }
        while (picked == excludeZone);
        return picked;
    }

    // 구역 인덱스를 화면 X 좌표로 변환 (구역 경계 근처 15% 패딩 적용)
    private float ZoneToX(int zoneIndex)
    {
        float zoneWidth = (maxXClamp - minXClamp) / zoneCount;
        float zoneMin = minXClamp + zoneIndex * zoneWidth;
        float zoneMax = zoneMin + zoneWidth;
        float padding = zoneWidth * 0.15f;
        return Random.Range(zoneMin + padding, zoneMax - padding);
    }

    private void DespawnOldPlatforms()
    {
        float cameraBottomY = mainCamera.transform.position.y - cachedHalfCamHeight - despawnMarginBelowCamera;
        // 파도 상단(또는 카메라 하단 유예선) 중 더 위쪽 라인을 기준으로 발판 회수
        // 단, 파도가 이미 발판을 삼킨 경우는 즉시 회수 (그래야 가짜 발판이 파도 안에 떠 있지 않음)
        float floodTopY = (cachedFloodCollider != null) ? cachedFloodCollider.bounds.max.y : float.MinValue;
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
                var timer = oldestPlatform.GetComponent<PlatformTimer>();
                string returnTag = timer != null ? timer.poolTag : platformPoolTags[0];
                ObjectPooler.Instance.ReturnToPool(returnTag, oldestPlatform);
            }
            else
            {
                break;
            }
        }
    }
}
