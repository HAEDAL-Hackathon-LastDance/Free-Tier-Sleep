using UnityEngine;
using UnityEditor;

public class GlitchWaveSetupTool : EditorWindow
{
    [MenuItem("Tools/Free Tier Sleep/Glitch Wave Setup")]
    public static void ShowWindow()
    {
        GetWindow<GlitchWaveSetupTool>("Glitch Wave Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Glitch Wave Effect Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Glitch Material"))
        {
            CreateGlitchMaterial();
        }
        
        if (GUILayout.Button("Setup Glitch Test Object"))
        {
            SetupTestObject();
        }
    }

    private void CreateGlitchMaterial()
    {
        Shader glitchShader = Shader.Find("Custom/GlitchWave");

        if (glitchShader != null)
        {
            Material mat = new Material(glitchShader);
            string path = "Assets/GlitchWaveMaterial.mat";
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GlitchWaveSetupTool] Material created at {path}");
            EditorGUIUtility.PingObject(mat);
        }
        else
        {
            Debug.LogError("[GlitchWaveSetupTool] Could not find GlitchWave shader. Please check the shader name.");
        }
    }

    private void SetupTestObject()
    {
        GameObject testObj = new GameObject("GlitchWave_TestObject");
        GlitchWaveTest testScript = testObj.AddComponent<GlitchWaveTest>();
        
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/GlitchWaveMaterial.mat");
        if (mat != null)
        {
            testScript.glitchMaterial = mat;
        }
        
        Selection.activeGameObject = testObj;
        Debug.Log("[GlitchWaveSetupTool] Test object created in the scene.");
    }
}
