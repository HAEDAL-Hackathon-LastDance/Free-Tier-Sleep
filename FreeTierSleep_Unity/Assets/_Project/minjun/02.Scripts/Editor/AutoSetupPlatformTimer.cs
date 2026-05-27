using UnityEngine;
using UnityEditor;

public class AutoSetupPlatformTimer : Editor
{
    [MenuItem("Tools/Free Tier Sleep/Auto Setup Platform Timer")]
    public static void SetupPlatformTimer()
    {
        // 1. 씬에서 ObjectPooler 찾기
        ObjectPooler pooler = Object.FindFirstObjectByType<ObjectPooler>();
        if (pooler == null)
        {
            Debug.LogError("씬에서 ObjectPooler를 찾을 수 없습니다.");
            return;
        }

        // 2. "Platform" 태그를 가진 프리팹 찾기
        GameObject platformPrefab = null;
        foreach (var pool in pooler.pools)
        {
            if (pool.tag == "Platform")
            {
                platformPrefab = pool.prefab;
                break;
            }
        }

        if (platformPrefab == null)
        {
            Debug.LogError("ObjectPooler에서 'Platform' 태그를 가진 프리팹을 찾을 수 없습니다.");
            return;
        }

        // 3. 프리팹 에셋 경로 가져오기
        string assetPath = AssetDatabase.GetAssetPath(platformPrefab);
        if (string.IsNullOrEmpty(assetPath))
        {
            // 프리팹이 아니라 씬에 있는 일반 오브젝트가 할당된 경우
            if (platformPrefab.GetComponent<PlatformTimer>() == null)
            {
                platformPrefab.AddComponent<PlatformTimer>();
                EditorUtility.SetDirty(platformPrefab);
                Debug.Log("씬의 발판 오브젝트에 PlatformTimer를 추가했습니다.");
            }
            else
            {
                Debug.Log("해당 발판 오브젝트에 이미 PlatformTimer가 붙어있습니다.");
            }
            return;
        }

        // 4. 프리팹 편집 모드를 열어서 컴포넌트 추가 후 저장
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
        {
            GameObject prefabRoot = editingScope.prefabContentsRoot;
            
            if (prefabRoot.GetComponent<PlatformTimer>() == null)
            {
                prefabRoot.AddComponent<PlatformTimer>();
                Debug.Log($"[{platformPrefab.name}] 프리팹에 PlatformTimer 스크립트를 성공적으로 추가했습니다!");
            }
            else
            {
                Debug.Log($"[{platformPrefab.name}] 프리팹에 이미 PlatformTimer가 붙어있습니다.");
            }
        }
    }
}
