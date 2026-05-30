using System.Collections.Generic;
using UnityEngine;

// 적 자동 스포너
// - 플레이어 고도에 따라 4종 적을 순차적으로 등장시킴
// - 각 타입마다 독립적인 reduction 값으로 Y=0~1000 전 구간 선형 감소 보장
// - ObjectPooler와 연동하여 스폰/회수 처리
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Header("참조")]
    public Camera mainCamera;

    [Header("Knocker (넉백 바이러스)")]
    public string knockerPoolTag = "Knocker";
    [Tooltip("이 고도(플레이어 Y 기준)부터 스폰 시작")]
    public float knockerStartAltitude = 50f;
    [Tooltip("스폰 기본 간격 (초)")]
    public float knockerInterval = 5f;
    [Tooltip("고도 100당 간격 감소량 — (baseInterval - minInterval) / 10 으로 설정하면 Y=1000에서 minInterval에 도달")]
    public float knockerIntervalReduction = 0.35f;
    [Tooltip("카메라 상단 기준 추가 오프셋(클수록 더 위에서 스폰)")]
    public float knockerSpawnYOffset = 5f;

    [Header("HeavySpam (무게 텍스트)")]
    public string heavySpamPoolTag = "HeavySpam";
    public float heavySpamStartAltitude = 80f;
    public float heavySpamInterval = 3f;
    public float heavySpamIntervalReduction = 0.15f;
    public float heavySpamSpawnYOffset = 12f;

    [Header("ConfusionBug (혼란 팝업)")]
    public string confusionBugPoolTag = "ConfusionBug";
    public float confusionBugStartAltitude = 150f;
    public float confusionBugInterval = 7f;
    public float confusionBugIntervalReduction = 0.55f;
    public float confusionBugSpawnYOffset = 6f;

    [Header("NotificationProjectile (알림창 탄환)")]
    public string projectilePoolTag = "NotificationProjectile";
    public float projectileStartAltitude = 200f;
    public float projectileInterval = 2.5f;
    public float projectileIntervalReduction = 0.10f;
    [Tooltip("카메라 좌/우 경계 밖 추가 오프셋")]
    public float projectileSpawnXOffset = 4f;

    [Header("난이도 스케일링")]
    [Tooltip("스폰 간격 최솟값 — 이 이하로는 줄어들지 않음")]
    public float minInterval = 1.5f;

    private PlayerController player;
    private float nextKnockerTime;
    private float nextHeavySpamTime;
    private float nextConfusionBugTime;
    private float nextProjectileTime;

    // 카메라 사이즈는 런타임에 변하지 않으므로 Start에서 1회 캐싱
    private float cachedHalfCamWidth;
    private float cachedHalfCamHeight;

    // Knocker, ConfusionBug는 자체 소멸 로직이 없으므로 직접 추적해 카메라 아래로 내려가면 회수
    private readonly List<GameObject> activeKnockers = new List<GameObject>();
    private readonly List<GameObject> activeConfusionBugs = new List<GameObject>();

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        player = Object.FindFirstObjectByType<PlayerController>();

        if (mainCamera != null)
        {
            cachedHalfCamHeight = mainCamera.orthographicSize;
            cachedHalfCamWidth = cachedHalfCamHeight * mainCamera.aspect;
        }

        float now = Time.time;
        nextKnockerTime       = now + knockerInterval;
        nextHeavySpamTime     = now + heavySpamInterval;
        nextConfusionBugTime  = now + confusionBugInterval;
        nextProjectileTime    = now + projectileInterval;
    }

    private void Update()
    {
        if (player == null || player.isDead || mainCamera == null) return;
        if (FreezeManager.IsFrozen) return;

        float playerY = player.transform.position.y;

        TrySpawnKnocker(playerY);
        TrySpawnHeavySpam(playerY);
        TrySpawnConfusionBug(playerY);
        TrySpawnProjectile(playerY);

        ReturnOutOfViewEnemies();
    }

    // ─────────────────── 개별 스폰 메서드 ───────────────────

    private void TrySpawnKnocker(float playerY)
    {
        if (playerY < knockerStartAltitude || Time.time < nextKnockerTime) return;

        Vector2 pos = new Vector2(RandomScreenX(), AboveCameraTopY(knockerSpawnYOffset));
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(knockerPoolTag, pos, Quaternion.identity);
        if (obj != null) activeKnockers.Add(obj);

        nextKnockerTime = Time.time + ScaledInterval(knockerInterval, playerY, knockerIntervalReduction);
    }

    private void TrySpawnHeavySpam(float playerY)
    {
        if (playerY < heavySpamStartAltitude || Time.time < nextHeavySpamTime) return;

        Vector2 pos = new Vector2(RandomScreenX(), AboveCameraTopY(heavySpamSpawnYOffset));
        ObjectPooler.Instance.SpawnFromPool(heavySpamPoolTag, pos, Quaternion.identity);

        nextHeavySpamTime = Time.time + ScaledInterval(heavySpamInterval, playerY, heavySpamIntervalReduction);
    }

    private void TrySpawnConfusionBug(float playerY)
    {
        if (playerY < confusionBugStartAltitude || Time.time < nextConfusionBugTime) return;

        Vector2 pos = new Vector2(RandomScreenX(), AboveCameraTopY(confusionBugSpawnYOffset));
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(confusionBugPoolTag, pos, Quaternion.identity);
        if (obj != null) activeConfusionBugs.Add(obj);

        nextConfusionBugTime = Time.time + ScaledInterval(confusionBugInterval, playerY, confusionBugIntervalReduction);
    }

    private void TrySpawnProjectile(float playerY)
    {
        if (playerY < projectileStartAltitude || Time.time < nextProjectileTime) return;

        float cx = mainCamera.transform.position.x;
        float cy = mainCamera.transform.position.y;

        float spawnX = (Random.value < 0.5f) ? cx - cachedHalfCamWidth - projectileSpawnXOffset : cx + cachedHalfCamWidth + projectileSpawnXOffset;
        float spawnY = cy + Random.Range(-cachedHalfCamHeight * 0.5f, cachedHalfCamHeight * 0.5f);

        ObjectPooler.Instance.SpawnFromPool(projectilePoolTag, new Vector2(spawnX, spawnY), Quaternion.identity);

        nextProjectileTime = Time.time + ScaledInterval(projectileInterval, playerY, projectileIntervalReduction);
    }

    // ─────────────────── 유틸 ───────────────────

    private float RandomScreenX()
    {
        return mainCamera.transform.position.x + Random.Range(-cachedHalfCamWidth + 1f, cachedHalfCamWidth - 1f);
    }

    private float AboveCameraTopY(float offset)
    {
        return mainCamera.transform.position.y + cachedHalfCamHeight + offset;
    }

    // 각 타입의 baseInterval에서 고도 비례 reduction을 뺀 값, minInterval 이하로 내려가지 않음
    private float ScaledInterval(float baseInterval, float playerY, float reductionPer100)
    {
        float reduction = (playerY / 100f) * reductionPer100;
        return Mathf.Max(minInterval, baseInterval - reduction);
    }

    // 카메라 하단 아래로 사라진 Knocker/ConfusionBug 풀로 반환
    private void ReturnOutOfViewEnemies()
    {
        float despawnY = mainCamera.transform.position.y - cachedHalfCamHeight - 5f;
        CleanupList(activeKnockers, knockerPoolTag, despawnY);
        CleanupList(activeConfusionBugs, confusionBugPoolTag, despawnY);
    }

    private void CleanupList(List<GameObject> list, string tag, float despawnY)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            GameObject obj = list[i];
            if (obj == null || !obj.activeInHierarchy)
            {
                list.RemoveAt(i);
                continue;
            }
            if (obj.transform.position.y < despawnY)
            {
                ObjectPooler.Instance.ReturnToPool(tag, obj);
                list.RemoveAt(i);
            }
        }
    }

    // 프리즈 아이템 사용 시 화면의 모든 활성 적을 풀로 즉시 반환
    public void ClearAllToPool()
    {
        for (int i = activeKnockers.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeKnockers[i];
            if (obj != null && obj.activeInHierarchy)
                ObjectPooler.Instance.ReturnToPool(knockerPoolTag, obj);
        }
        activeKnockers.Clear();

        for (int i = activeConfusionBugs.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeConfusionBugs[i];
            if (obj != null && obj.activeInHierarchy)
                ObjectPooler.Instance.ReturnToPool(confusionBugPoolTag, obj);
        }
        activeConfusionBugs.Clear();

        foreach (HeavySpam spam in Object.FindObjectsByType<HeavySpam>(FindObjectsSortMode.None))
        {
            if (spam.gameObject.activeInHierarchy)
                ObjectPooler.Instance.ReturnToPool(heavySpamPoolTag, spam.gameObject);
        }

        foreach (NotificationProjectile proj in Object.FindObjectsByType<NotificationProjectile>(FindObjectsSortMode.None))
        {
            if (proj.gameObject.activeInHierarchy)
                ObjectPooler.Instance.ReturnToPool(projectilePoolTag, proj.gameObject);
        }
    }
}
