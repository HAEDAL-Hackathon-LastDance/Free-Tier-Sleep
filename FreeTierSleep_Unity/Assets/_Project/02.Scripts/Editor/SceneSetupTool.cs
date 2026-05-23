using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/Free Tier Sleep/Setup Test Scene")]
    public static void SetupScene()
    {
        // 1. 레이어 설정 (Ground 레이어 자동 생성)
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        bool layerExists = false;
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
            if (layerSP.stringValue == "Ground") { layerExists = true; break; }
        }

        if (!layerExists)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerSP.stringValue))
                {
                    layerSP.stringValue = "Ground";
                    tagManager.ApplyModifiedProperties();
                    Debug.Log("Ground 레이어를 생성했습니다.");
                    break;
                }
            }
        }

        // 2. 바닥 생성
        GameObject ground = GameObject.Find("Ground_Auto");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground_Auto";
            ground.transform.position = new Vector3(0, -2, 0);
            ground.transform.localScale = new Vector3(20, 1, 1);
            ground.layer = LayerMask.NameToLayer("Ground");
            
            // 2D용으로 변경
            DestroyImmediate(ground.GetComponent<BoxCollider>());
            ground.AddComponent<BoxCollider2D>();
            Debug.Log("바닥을 생성했습니다.");
        }

        // 3. 플레이어 생성
        GameObject player = GameObject.Find("Player_Auto");
        if (player == null)
        {
            player = new GameObject("Player_Auto");
            player.transform.position = Vector3.zero;

            // 비주얼 추가 (사각형)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = Vector3.zero;
            DestroyImmediate(visual.GetComponent<MeshCollider>());

            // 컴포넌트 추가
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<BoxCollider2D>();
            var playerInput = player.AddComponent<PlayerInput>();
            var controller = player.AddComponent<PlayerController>();

            // Input Actions 연결
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions != null)
            {
                playerInput.actions = actions;
                playerInput.defaultControlScheme = "Keyboard&Mouse";
                playerInput.notificationBehavior = PlayerNotifications.SendMessages;
            }

            // 컨트롤러 설정
            controller.groundLayer = LayerMask.GetMask("Ground");
            
            Debug.Log("플레이어를 생성하고 모든 설정을 완료했습니다!");
        }

        Selection.activeGameObject = player;
        SceneView.lastActiveSceneView.FrameSelected();
    }
}
