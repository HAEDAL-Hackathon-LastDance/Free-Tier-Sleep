using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Taehui
{
    /// <summary>
    /// 화면에 무작위로 광고 팝업 프리팹을 생성하는 매니저
    /// </summary>
    public class AdPopupManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] adPrefabs;
        
        [Header("Spawn Settings")]
        [SerializeField] private RectTransform spawnArea; // 팝업이 생성될 UI 영역
        [SerializeField] private float minSpawnDelay = 0.1f;
        [SerializeField] private float maxSpawnDelay = 0.5f;
        [SerializeField] private int maxActiveAds = 30; // 화면에 동시에 표시될 최대 광고 개수 (Draw Call 최적화)

        private List<GameObject> activePopups = new List<GameObject>();
        // 프리팹 인스턴스 ID별 풀 관리 (Key: Prefab InstanceID, Value: 비활성 오브젝트 리스트)
        private Dictionary<int, List<GameObject>> poolDict = new Dictionary<int, List<GameObject>>();
        
        private bool isSpawning = false;
        private AudioSource audioSource;
        private AudioClip warningClip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.25f; // 광고 팝업이 뜰 때 너무 불쾌하지 않을 강도의 소리

            warningClip = ProceduralAudioHelper.CreateWarningSound();
        }

        public void StartSpawning()
        {
            if (isSpawning) return;
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            isSpawning = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// 씬 전환 시 메모리 누수를 막기 위해 활성/비활성 상태인 모든 팝업 오브젝트를 파괴합니다.
        /// </summary>
        public void ClearPopups()
        {
            // 활성 팝업 파괴
            foreach (var popup in activePopups)
            {
                if (popup != null) Destroy(popup);
            }
            activePopups.Clear();

            // 풀에 대기 중인 비활성 팝업 파괴
            foreach (var kvp in poolDict)
            {
                foreach (var popup in kvp.Value)
                {
                    if (popup != null) Destroy(popup);
                }
                kvp.Value.Clear();
            }
            poolDict.Clear();
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                SpawnAd();
                float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnAd()
        {
            if (adPrefabs == null || adPrefabs.Length == 0) return;

            // 1. 무작위 프리팹 선정
            int index = Random.Range(0, adPrefabs.Length);
            GameObject prefab = adPrefabs[index];
            int prefabID = prefab.GetInstanceID();

            // 2. 화면 표시 개수가 최댓값을 초과할 경우 가장 오래된 광고를 풀로 반환
            if (activePopups.Count >= maxActiveAds)
            {
                GameObject oldestAd = activePopups[0];
                activePopups.RemoveAt(0);
                
                if (oldestAd != null)
                {
                    oldestAd.SetActive(false);
                    // 어느 프리팹의 인스턴스였는지 식별용 컴포넌트나 매핑이 필요하므로 풀 딕셔너리에 보관
                    // (여기서는 간단히 태그나 내부 구조를 보며 보관하기 위해 ID를 추적하는 헬퍼 정보 활용)
                    int oldestPrefabID = GetAssociatedPrefabID(oldestAd);
                    if (oldestPrefabID != 0)
                    {
                        if (!poolDict.ContainsKey(oldestPrefabID))
                            poolDict[oldestPrefabID] = new List<GameObject>();
                        poolDict[oldestPrefabID].Add(oldestAd);
                    }
                    else
                    {
                        // 식별 불가능한 낙오 오브젝트는 예외적으로 파괴 처리
                        Destroy(oldestAd);
                    }
                }
            }

            // 3. 풀에서 오브젝트 재사용 시도
            GameObject popup = null;
            if (poolDict.ContainsKey(prefabID) && poolDict[prefabID].Count > 0)
            {
                int lastIdx = poolDict[prefabID].Count - 1;
                popup = poolDict[prefabID][lastIdx];
                poolDict[prefabID].RemoveAt(lastIdx);
                
                if (popup != null)
                {
                    popup.SetActive(true);
                }
            }

            // 4. 풀에 없으면 새로 생성
            if (popup == null)
            {
                popup = Instantiate(prefab, spawnArea);
                // 식별용 컴포넌트 추가
                AdPrefabIdentifier identifier = popup.AddComponent<AdPrefabIdentifier>();
                identifier.prefabID = prefabID;
            }

            // 5. 위치 재설정 (UI 좌표 기준 무작위 좌표)
            Rect rect = spawnArea.rect;
            float x = Random.Range(rect.xMin, rect.xMax);
            float y = Random.Range(rect.yMin, rect.yMax);
            
            popup.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            activePopups.Add(popup);

            // 팝업 경고 SFX 재생
            if (audioSource != null && warningClip != null)
            {
                audioSource.PlayOneShot(warningClip);
            }
        }

        private int GetAssociatedPrefabID(GameObject obj)
        {
            AdPrefabIdentifier idComp = obj.GetComponent<AdPrefabIdentifier>();
            return idComp != null ? idComp.prefabID : 0;
        }
    }

    /// <summary>
    /// 오브젝트 풀링 시 어떤 프리팹에서 유래했는지 추적하기 위한 경량 컴포넌트
    /// </summary>
    public class AdPrefabIdentifier : MonoBehaviour
    {
        public int prefabID;
    }
}
