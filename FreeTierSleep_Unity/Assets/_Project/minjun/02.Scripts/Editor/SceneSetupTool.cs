using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/Free Tier Sleep/1. 애니메이터 FSM 자동 생성 (클립 기반)")]
    public static void CreateAnimatorController()
    {
        string path = "Assets/_Project/PlayerAnimator.controller";
        
        // 1. 애니메이터 컨트롤러 생성
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        
        // 2. 파라미터 추가
        controller.AddParameter("isRun", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("yVelocity", AnimatorControllerParameterType.Float);

        // 3. 클립 찾기
        AnimationClip idleClip = FindClip("idle");
        AnimationClip runClip = FindClip("run");
        AnimationClip jumpClip = FindClip("jump");
        AnimationClip fallClip = FindClip("fall");

        var rootStateMachine = controller.layers[0].stateMachine;

        // 4. 상태(State) 생성
        var idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;

        var runState = rootStateMachine.AddState("Run");
        runState.motion = runClip;

        var jumpState = rootStateMachine.AddState("Jump");
        jumpState.motion = jumpClip;

        var fallState = rootStateMachine.AddState("Fall");
        fallState.motion = fallClip;

        // 5. 트랜지션(화살표) 설정 (조건 완화)
        
        // Idle <-> Run
        var idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.If, 0, "isRun");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0f;

        var runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRun");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0f;

        // Any State -> Jump
        var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
        anyToJump.AddCondition(AnimatorConditionMode.Greater, 0.01f, "yVelocity");
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0f;
        anyToJump.canTransitionToSelf = false;

        // Jump -> Fall
        var jumpToFall = jumpState.AddTransition(fallState);
        jumpToFall.AddCondition(AnimatorConditionMode.Less, 0.1f, "yVelocity");
        jumpToFall.hasExitTime = false;
        jumpToFall.duration = 0.1f;

        // Fall -> Idle
        var fallToIdle = fallState.AddTransition(idleState);
        fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
        fallToIdle.hasExitTime = false;
        fallToIdle.duration = 0f;

        // 6. 플레이어에게 할당
        GameObject player = GameObject.Find("Player_Auto");
        if (player == null) player = GameObject.Find("Player");
        
        if (player != null)
        {
            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
        }

        AssetDatabase.SaveAssets();
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        Debug.Log("[성공] 애니메이션 FSM 생성 및 저장이 완료되었습니다.");
    }

    [MenuItem("Tools/Free Tier Sleep/2. 최종 조립 (콜라이더/센서/크기 맞춤)")]
    public static void FinalizePlayer()
    {
        GameObject player = GameObject.Find("Player_Auto");
        if (player == null) player = GameObject.Find("Player");
        if (player == null) return;

        Undo.RecordObject(player, "Finalize Player");

        // 1. 크기 조절 (0.3배)
        player.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            BoxCollider2D col = player.GetComponent<BoxCollider2D>();
            if (col == null) col = player.AddComponent<BoxCollider2D>();
            col.size = sr.sprite.bounds.size;
            col.offset = sr.sprite.bounds.center;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                // 이제 발바닥에서 시작하므로 거리는 아주 짧게(0.1) 설정
                pc.groundCheckDistance = 0.1f;
                pc.groundCheckSize = new Vector2(col.size.x * 0.7f, 0.1f);
                pc.groundLayer = LayerMask.GetMask("Ground");
                pc.jumpForce = 10f; 
            }
        }
        
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        Debug.Log("[성공] 캐릭터 크기(0.3) 및 센서 설정이 완료되었으며 씬에 저장되었습니다.");
    }

    private static AnimationClip FindClip(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"{name} t:AnimationClip");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }
        return null;
    }
}
