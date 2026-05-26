using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class LevelSetupTool : EditorWindow
{
    [MenuItem("Tools/Free Tier Sleep/Phase 1 레벨 자동 세팅")]
    public static void SetupPhase1Level()
    {
        // 1. 프리팹 저장 폴더 확인 및 생성
        string folderPath = "Assets/_Project/minjun/Prefabs";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            // 폴더가 없으면 상위 폴더부터 차례로 생성
            if (!AssetDatabase.IsValidFolder("Assets/_Project/minjun"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "minjun");
            }
            AssetDatabase.CreateFolder("Assets/_Project/minjun", "Prefabs");
        }

        // 2. PopupPlatform 프리팹 생성
        string prefabPath = folderPath + "/PopupPlatform.prefab";
        GameObject prefabObj = null;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            GameObject tempPlatform = new GameObject("PopupPlatform");
            tempPlatform.AddComponent<SpriteRenderer>();
            tempPlatform.AddComponent<BoxCollider2D>();
            
            prefabObj = PrefabUtility.SaveAsPrefabAsset(tempPlatform, prefabPath);
            DestroyImmediate(tempPlatform);
            Debug.Log("PopupPlatform 프리팹이 생성되었습니다: " + prefabPath);
        }
        else
        {
            prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log("기존 PopupPlatform 프리팹을 사용합니다.");
        }

        // 3. ObjectPooler 세팅
        ObjectPooler pooler = FindObjectOfType<ObjectPooler>();
        if (pooler == null)
        {
            GameObject poolerObj = new GameObject("ObjectPooler");
            pooler = poolerObj.AddComponent<ObjectPooler>();
            
            pooler.pools = new List<ObjectPooler.Pool>();
            ObjectPooler.Pool platformPool = new ObjectPooler.Pool
            {
                tag = "Platform",
                prefab = prefabObj,
                size = 20
            };
            pooler.pools.Add(platformPool);
            Debug.Log("ObjectPooler가 씬에 생성 및 세팅되었습니다.");
        }

        // 4. LevelManager 세팅
        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();
        if (levelGen == null)
        {
            GameObject levelManagerObj = new GameObject("LevelManager");
            levelGen = levelManagerObj.AddComponent<LevelGenerator>();

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                levelGen.mainCamera = mainCam;
                levelGen.cameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogWarning("Main Camera를 찾을 수 없어 LevelGenerator에 할당하지 못했습니다.");
            }

            levelGen.targetAltitude = 1000f;
            levelGen.spawnYThreshold = 15f;
            levelGen.minXClamp = -7f;
            levelGen.maxXClamp = 7f;
            
            Debug.Log("LevelManager가 씬에 생성 및 세팅되었습니다.");
        }

        // 5. 씬 변경사항 저장
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Phase 1 레벨 세팅이 완료되고 씬이 저장되었습니다. 이제 이 에디터 스크립트를 삭제하셔도 됩니다.");
        }
    }
}
