using UnityEngine;
using UnityEditor;

public class AutoAssignDataFlood : Editor
{
    [MenuItem("Tools/Free Tier Sleep/Auto Assign DataFlood")]
    public static void AssignDataFlood()
    {
        // 씬에서 LevelGenerator와 RisingDataFlood 컴포넌트를 찾습니다.
        LevelGenerator levelGenerator = Object.FindFirstObjectByType<LevelGenerator>();
        RisingDataFlood dataFlood = Object.FindFirstObjectByType<RisingDataFlood>();

        if (levelGenerator == null)
        {
            Debug.LogError("씬에서 LevelGenerator를 찾을 수 없습니다.");
            return;
        }

        if (dataFlood == null)
        {
            Debug.LogError("씬에서 RisingDataFlood를 찾을 수 없습니다.");
            return;
        }

        // 자동 할당 진행
        levelGenerator.dataFlood = dataFlood;
        
        // 변경 사항이 저장될 수 있도록 Dirty 체크
        EditorUtility.SetDirty(levelGenerator);
        
        Debug.Log("LevelGenerator에 RisingDataFlood 할당이 완료되었습니다! 씬을 저장해 주세요.");
    }
}
