using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace Taehui.Editor
{
    /// <summary>
    /// Phase 1 -> Phase 2 전환 컷신 씬(Scene_Transition)을 클릭 한 번으로 조립하고 빌드 세팅에 등록하는 에디터 스크립트
    /// </summary>
    public class PhaseTransitionAutoSetup : EditorWindow
    {
        [MenuItem("Tools/Taehui/Setup Transition Scene - AUTO")]
        public static void FullAutoTransitionSetup()
        {
            // 1. 새로운 씬 생성
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "Scene_Transition";
            }

            // 2. 폰트 에셋 로드 (DungGeunMo SDF)
            TMP_FontAsset koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/suyeon/03. art/DungGeunMo SDF.asset");
            if (koreanFont == null)
            {
                string[] guids = AssetDatabase.FindAssets("DungGeunMo SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            // 3. 메인 카메라 조율
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = Object.FindFirstObjectByType<Camera>();
            }
            if (mainCam != null)
            {
                mainCam.tag = "MainCamera";
                mainCam.backgroundColor = Color.black;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                
                // URP 포스트 프로세싱 렌더 기능 명시적 활성화
                var cameraData = mainCam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (cameraData == null)
                {
                    cameraData = mainCam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                }
                cameraData.renderPostProcessing = true;
            }

            // 4. 캔버스 및 반응형 스케일러 설정
            GameObject canvasObj = new GameObject("TransitionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 5. 비디오 플레이어 및 비디오 클립 오토 바인딩
            GameObject vpObj = new GameObject("VideoPlayerObject");
            VideoPlayer vp = vpObj.AddComponent<VideoPlayer>();
            vp.playOnAwake = false;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.aspectRatio = VideoAspectRatio.FitHorizontally;
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.skipOnDrop = true;
            vp.waitForFirstFrame = false;

            VideoClip endingClip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/_Project/Taehui/04.Animations/Ending_Swap.mp4");
            if (endingClip != null)
            {
                vp.clip = endingClip;
            }

            // 비디오 클립 H.264 강제 트랜스코딩 세팅 적용 (코덱 불일치로 인한 디코더 멈춤/오류 방지)
            string videoPath = "Assets/_Project/Taehui/04.Animations/Ending_Swap.mp4";
            VideoClipImporter videoImporter = AssetImporter.GetAtPath(videoPath) as VideoClipImporter;
            if (videoImporter != null)
            {
                VideoImporterTargetSettings settings = videoImporter.GetTargetSettings("Standalone");
                if (settings != null)
                {
                    if (!settings.enableTranscoding || settings.codec != VideoCodec.H264)
                    {
                        settings.enableTranscoding = true;
                        settings.codec = VideoCodec.H264;
                        videoImporter.SetTargetSettings("Standalone", settings);
                        videoImporter.SaveAndReimport();
                        AssetDatabase.Refresh();
                    }
                }
            }

            // URP 대응 비디오 렌더용 RawImage UI 생성
            GameObject videoRawImageObj = new GameObject("VideoRawImage");
            videoRawImageObj.transform.SetParent(canvasObj.transform, false);
            RawImage rawImg = videoRawImageObj.AddComponent<RawImage>();
            rawImg.color = Color.white;
            SetFullStretch(videoRawImageObj.GetComponent<RectTransform>());
            videoRawImageObj.transform.SetAsFirstSibling();

            // 6. 독백용 암전 패널 및 텍스트 빌드
            GameObject monologuePanel = new GameObject("MonologuePanel");
            monologuePanel.transform.SetParent(canvasObj.transform, false);
            Image monoBg = monologuePanel.AddComponent<Image>();
            monoBg.color = Color.black;
            SetFullStretch(monologuePanel.GetComponent<RectTransform>());

            GameObject monoTextObj = new GameObject("MonologueText");
            monoTextObj.transform.SetParent(monologuePanel.transform, false);
            RectTransform monoTextRect = monoTextObj.AddComponent<RectTransform>();
            monoTextRect.anchorMin = new Vector2(0.1f, 0.3f);
            monoTextRect.anchorMax = new Vector2(0.9f, 0.7f);
            monoTextRect.anchoredPosition = Vector2.zero;
            monoTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI monoTmp = monoTextObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) monoTmp.font = koreanFont;
            monoTmp.text = "";
            monoTmp.fontSize = 32;
            monoTmp.lineSpacing = 15f;
            monoTmp.color = Color.white;
            monoTmp.alignment = TextAlignmentOptions.Center;
            monologuePanel.SetActive(false);

            // 7. 시스템 에러 지지직 패널
            GameObject errorPanel = new GameObject("ErrorPanel");
            errorPanel.transform.SetParent(canvasObj.transform, false);
            Image errBg = errorPanel.AddComponent<Image>();
            SetFullStretch(errorPanel.GetComponent<RectTransform>());
            errorPanel.AddComponent<GraphicCollapseEffect>();
            errorPanel.SetActive(false);

            // 가독성 개선용 반투명 검정 배후 패널
            GameObject errorTextBackObj = new GameObject("ErrorTextBacker");
            errorTextBackObj.transform.SetParent(errorPanel.transform, false);
            RectTransform backRect = errorTextBackObj.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 0.35f);
            backRect.anchorMax = new Vector2(1f, 0.65f);
            backRect.anchoredPosition = Vector2.zero;
            backRect.sizeDelta = Vector2.zero;
            Image backImage = errorTextBackObj.AddComponent<Image>();
            backImage.color = new Color(0f, 0f, 0f, 0.95f);

            // 에러 텍스트
            GameObject errTextObj = new GameObject("ErrorText");
            errTextObj.transform.SetParent(errorTextBackObj.transform, false);
            RectTransform errTextRect = errTextObj.AddComponent<RectTransform>();
            errTextRect.anchorMin = Vector2.zero;
            errTextRect.anchorMax = Vector2.one;
            errTextRect.anchoredPosition = Vector2.zero;
            errTextRect.sizeDelta = new Vector2(-100, -40);

            TextMeshProUGUI errTmp = errTextObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) errTmp.font = koreanFont;
            errTmp.text = "";
            errTmp.fontSize = 38;
            errTmp.textWrappingMode = TextWrappingModes.Normal;
            errTmp.fontSizeMin = 20;
            errTmp.fontSizeMax = 38;
            errTmp.enableAutoSizing = true; // 화면 밖 넘침을 방지하기 위한 오토 사이징 활성화
            errTmp.lineSpacing = 12f;
            errTmp.color = new Color(0.95f, 0.1f, 0.1f, 1f);
            errTmp.alignment = TextAlignmentOptions.Center;

            // 8. 씬 전환용 페이드오버레이
            GameObject overlayObj = new GameObject("FadeOverlay");
            overlayObj.transform.SetParent(canvasObj.transform, false);
            Image overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.color = Color.black;
            CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0;
            overlayGroup.blocksRaycasts = false;
            SetFullStretch(overlayObj.GetComponent<RectTransform>());

            // 9. URP Volume 연결
            string profilePath = "Assets/_Project/Taehui/05.Prefabs/IntroVolumeProfile.asset";
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            GameObject volumeObj = new GameObject("GlobalVolume");
            Volume globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            if (volumeProfile != null)
            {
                globalVolume.profile = volumeProfile;
            }

            // 10. 전환 컨트롤러 조립 및 필드 할당
            GameObject controllerObj = new GameObject("PhaseTransitionController");
            PhaseTransitionController controller = controllerObj.AddComponent<PhaseTransitionController>();
            
            AssignField(controller, "videoPlayer", vp);
            AssignField(controller, "videoRawImage", rawImg);
            AssignField(controller, "videoDuration", 10f);
            AssignField(controller, "monologuePanel", monologuePanel);
            AssignField(controller, "monologueText", monoTmp);
            AssignField(controller, "errorPanel", errorPanel);
            AssignField(controller, "errorText", errTmp);
            AssignField(controller, "blackOverlay", overlayGroup);
            AssignField(controller, "postProcessVolume", globalVolume);

            // 11. 빌드 세팅에 추가 및 저장
            AddTransitionSceneToBuildSettings();

            Debug.Log("★ PHASE TRANSITION SCENE AUTO SETUP COMPLETE ★\nTransition scene built successfully for Phase 1 -> Phase 2 transition!");
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private static void AssignField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(target, value);
        }

        private static void AddTransitionSceneToBuildSettings()
        {
            string scenePath = "Assets/_Project/Taehui/01.Scenes/Scene_Transition.unity";
            var scene = EditorSceneManager.GetActiveScene();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, scenePath);

            var currentScenes = EditorBuildSettings.scenes;
            bool exists = false;
            foreach (var s in currentScenes)
            {
                if (s.path == scenePath) exists = true;
            }

            if (!exists)
            {
                var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
                System.Array.Copy(currentScenes, newScenes, currentScenes.Length);
                newScenes[currentScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
            }
        }
    }
}
