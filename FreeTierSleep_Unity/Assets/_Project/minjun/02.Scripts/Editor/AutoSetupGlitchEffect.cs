using UnityEngine;
using UnityEditor;

public class AutoSetupGlitchEffect : Editor
{
    [MenuItem("Tools/Free Tier Sleep/Auto Setup Glitch Effect")]
    public static void SetupGlitchEffect()
    {
        // 1. 씬에서 PlayerController 찾기
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("씬에서 PlayerController를 찾을 수 없습니다.");
            return;
        }

        // 2. 프로젝트 내에서 GlitchWaveMaterial 찾기
        string[] guids = AssetDatabase.FindAssets("GlitchWaveMaterial t:Material");
        if (guids.Length == 0)
        {
            Debug.LogError("프로젝트에서 'GlitchWaveMaterial'을 찾을 수 없습니다. 파일 이름이 정확한지 확인해 주세요.");
            return;
        }

        // 첫 번째로 찾은 머티리얼 로드
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Material glitchMat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

        if (glitchMat == null)
        {
            Debug.LogError("머티리얼을 로드하는 데 실패했습니다.");
            return;
        }

        // 3. PlayerController에 할당
        player.glitchMaterial = glitchMat;
        
        // 셰이더 프로퍼티 이름 설정 (일반적으로 많이 쓰이는 이름으로 임시 세팅, 필요시 셰이더에 맞게 수정)
        // 만약 셰이더 코드를 확인하셨을 때 변수명이 다르다면 이 부분을 수정하시면 됩니다.
        player.glitchPropertyName = "_Intensity"; 

        // 4. 변경 사항 저장 처리
        EditorUtility.SetDirty(player);
        
        Debug.Log($"PlayerController에 Glitch Effect 세팅이 완료되었습니다! (Material: {glitchMat.name}, Property: {player.glitchPropertyName}) 씬을 저장해 주세요.");
    }
}
