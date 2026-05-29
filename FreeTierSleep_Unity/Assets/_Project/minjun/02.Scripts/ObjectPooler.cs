using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton
    public static ObjectPooler Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(this.transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // 큐에 사용 가능한 오브젝트가 없으면 새로 생성 (유연한 대처)
        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                GameObject newObj = Instantiate(pool.prefab);
                newObj.SetActive(false);
                newObj.transform.SetParent(this.transform);
                poolDictionary[tag].Enqueue(newObj);
            }
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        // 봇 피드백 반영: 과도한 로그 출력은 에디터/개발 빌드에서만 작동하도록 제한
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"ReturnToPool 호출됨: 태그={tag}, 오브젝트={obj.name}");
#endif

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        // [민준님의 기존 방어 로직] 이미 비활성화된 객체라면 중복 반환(Double Enqueue) 방지
        if (!obj.activeSelf)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"{obj.name}은(는) 이미 비활성화 상태입니다. 중복 반환을 방지합니다.");
#endif
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{obj.name} 비활성화 및 큐에 반환 완료.");
#endif
    }
}
