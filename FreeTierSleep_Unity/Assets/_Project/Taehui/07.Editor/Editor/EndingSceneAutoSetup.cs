using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace Taehui.Editor
{
    /// <summary>
    /// 최종 엔딩 씬(Scene_Ending)을 아침 페이드인 및 텍스처 깨짐 연출 구조로 원클릭 조립하고 빌드 세팅에 주입하는 에디터 스크립트
    /// </summary>
    public class EndingSceneAutoSetup : EditorWindow
    {
        [MenuItem("Tools/Taehui/Setup Ending Scene - AUTO")]
        public static void FullAutoEndingSetup()
        {
            AssetDatabase.Refresh(); // 신규 추가된 이미지 파일 재탐색 및 강제 로딩

            // 1. 새로운 씬 생성
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "Scene_Ending";
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
            GameObject canvasObj = new GameObject("EndingCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 5. 아침 연출 이미지 패널 (ending_waking_up.png)
            string imagePath = "Assets/_Project/Taehui/03.Art/ending_waking_up.png";
            
            // 이미지 에셋을 강제로 임포트하고 캐시를 명시적으로 최신화합니다.
            AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceUpdate);
            
            TextureImporter textureImporter = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (textureImporter != null)
            {
                bool needReimport = false;
                if (textureImporter.textureType != TextureImporterType.Default)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    needReimport = true;
                }
                if (needReimport)
                {
                    textureImporter.SaveAndReimport();
                    AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.Refresh();
            Texture morningTexture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
            if (morningTexture == null)
            {
                Debug.LogError($"[EndingSceneAutoSetup] 기상 이미지({imagePath}) 로딩에 실패했습니다. 에셋 파일 존재 여부 및 포맷을 확인해 주세요.");
            }

            GameObject morningPanel = new GameObject("MorningImage");
            morningPanel.transform.SetParent(canvasObj.transform, false);
            RawImage morningImage = morningPanel.AddComponent<RawImage>();
            morningImage.color = Color.white;
            if (morningTexture != null)
            {
                morningImage.texture = morningTexture;
            }
            
            // 이미지 찌그러짐을 방지하고 종횡비를 유지하도록 AspectRatioFitter 장착
            AspectRatioFitter fitter = morningPanel.AddComponent<AspectRatioFitter>();
            fitter.aspectRatio = 16f / 9f;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            // Grayscale UI 전용 셰이더 기반 마테리얼 동적 생성 및 할당 (채도 페이드 보장용)
            Shader grayscaleShader = Shader.Find("UI/GrayscaleUI");
            if (grayscaleShader != null)
            {
                Material grayscaleMat = new Material(grayscaleShader);
                grayscaleMat.SetFloat("_EffectAmount", 1.0f); // 초기 상태 흑백(1.0) 설정
                morningImage.material = grayscaleMat;
            }
            else
            {
                Debug.LogWarning("[EndingSceneAutoSetup] UI/GrayscaleUI shader를 찾지 못했습니다. 셰이더 에셋 임포트 상태를 확인해 주세요.");
            }

            SetFullStretch(morningPanel.GetComponent<RectTransform>());

            // 6. 화이트 페이드오버레이
            GameObject whiteOverlayObj = new GameObject("WhiteFadeOverlay");
            whiteOverlayObj.transform.SetParent(canvasObj.transform, false);
            Image whiteImage = whiteOverlayObj.AddComponent<Image>();
            whiteImage.color = Color.white;
            CanvasGroup whiteOverlayGroup = whiteOverlayObj.AddComponent<CanvasGroup>();
            whiteOverlayGroup.alpha = 1f;
            SetFullStretch(whiteOverlayObj.GetComponent<RectTransform>());

            // 7. 디지털 그래픽 붕괴 글리치 패널
            GameObject glitchPanel = new GameObject("GlitchPanel");
            glitchPanel.transform.SetParent(canvasObj.transform, false);
            Image glitchImage = glitchPanel.AddComponent<Image>();
            SetFullStretch(glitchPanel.GetComponent<RectTransform>());
            glitchPanel.AddComponent<GraphicCollapseEffect>();
            glitchPanel.SetActive(false);

            // 7-2. 수면 기기 재연결 절망 루프 이미지 패널 (dragged_back.png)
            string draggedBackPath = "Assets/_Project/Taehui/03.Art/dragged_back.png";
            AssetDatabase.ImportAsset(draggedBackPath, ImportAssetOptions.ForceUpdate);
            
            TextureImporter draggedImporter = AssetImporter.GetAtPath(draggedBackPath) as TextureImporter;
            if (draggedImporter != null)
            {
                bool needReimport = false;
                if (draggedImporter.textureType != TextureImporterType.Default)
                {
                    draggedImporter.textureType = TextureImporterType.Default;
                    needReimport = true;
                }
                if (needReimport)
                {
                    draggedImporter.SaveAndReimport();
                    AssetDatabase.ImportAsset(draggedBackPath, ImportAssetOptions.ForceUpdate);
                }
            }

            Texture draggedTexture = AssetDatabase.LoadAssetAtPath<Texture>(draggedBackPath);
            if (draggedTexture == null)
            {
                Debug.LogError($"[EndingSceneAutoSetup] 절망 루프 이미지({draggedBackPath}) 로딩에 실패했습니다.");
            }

            GameObject draggedPanel = new GameObject("DraggedBackImage");
            draggedPanel.transform.SetParent(canvasObj.transform, false);
            RawImage draggedBackImage = draggedPanel.AddComponent<RawImage>();
            draggedBackImage.color = Color.white;
            if (draggedTexture != null)
            {
                draggedBackImage.texture = draggedTexture;
            }
            
            // 비율 유지 장착
            AspectRatioFitter draggedFitter = draggedPanel.AddComponent<AspectRatioFitter>();
            draggedFitter.aspectRatio = 16f / 9f;
            draggedFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            // 흑백 마테리얼 주입
            if (grayscaleShader != null)
            {
                Material dragMat = new Material(grayscaleShader);
                dragMat.SetFloat("_EffectAmount", 1.0f); // 완전 흑백 고정
                draggedBackImage.material = dragMat;
            }

            SetFullStretch(draggedPanel.GetComponent<RectTransform>());
            draggedPanel.SetActive(false);

            // 8. DOS CMD 터미널 패널 구성
            GameObject cmdPanel = new GameObject("CMDPanel");
            cmdPanel.transform.SetParent(canvasObj.transform, false);
            Image cmdBg = cmdPanel.AddComponent<Image>();
            cmdBg.color = Color.black;
            SetFullStretch(cmdPanel.GetComponent<RectTransform>());

            GameObject cmdTextObj = new GameObject("CMDText");
            cmdTextObj.transform.SetParent(cmdPanel.transform, false);
            RectTransform cmdTextRect = cmdTextObj.AddComponent<RectTransform>();
            cmdTextRect.anchorMin = new Vector2(0.05f, 0.05f);
            cmdTextRect.anchorMax = new Vector2(0.95f, 0.95f);
            cmdTextRect.anchoredPosition = Vector2.zero;
            cmdTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI cmdTmp = cmdTextObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) cmdTmp.font = koreanFont;
            cmdTmp.text = "";
            cmdTmp.fontSize = 36;
            cmdTmp.lineSpacing = 24f;
            cmdTmp.color = new Color(0f, 0.9f, 0.5f, 1f); // 터미널 그린색
            cmdTmp.alignment = TextAlignmentOptions.TopLeft;
            cmdPanel.SetActive(false);

            // 9. 씬 전환용 검은 페이드오버레이
            GameObject overlayObj = new GameObject("BlackFadeOverlay");
            overlayObj.transform.SetParent(canvasObj.transform, false);
            Image overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.color = Color.black;
            CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0;
            overlayGroup.blocksRaycasts = false;
            SetFullStretch(overlayObj.GetComponent<RectTransform>());

            // 10. URP Volume 연결
            string profilePath = "Assets/_Project/Taehui/05.Prefabs/IntroVolumeProfile.asset";
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            GameObject volumeObj = new GameObject("GlobalVolume");
            Volume globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.weight = 1f; // 볼륨 강도를 확실하게 100%로 설정
            globalVolume.priority = 99f; // 가장 높은 우선순위로 겹치지 않고 렌더링되게 설정
            if (volumeProfile != null)
            {
                globalVolume.profile = volumeProfile;
            }

            // 11. 엔딩 메인 컨트롤러 조립 및 필드 할당
            GameObject controllerObj = new GameObject("EndingSceneController");
            EndingSceneController controller = controllerObj.AddComponent<EndingSceneController>();
            
            AssignField(controller, "morningImage", morningImage);
            AssignField(controller, "whiteFadeOverlay", whiteOverlayGroup);
            AssignField(controller, "glitchPanel", glitchPanel);
            AssignField(controller, "draggedBackImage", draggedBackImage);
            AssignField(controller, "cmdPanel", cmdPanel);
            AssignField(controller, "cmdText", cmdTmp);
            AssignField(controller, "blackOverlay", overlayGroup);
            AssignField(controller, "postProcessVolume", globalVolume);

            // 12. 빌드 세팅에 추가 및 저장
            AddEndingSceneToBuildSettings();

            Debug.Log("★ ENDING SCENE AUTO SETUP COMPLETE ★\nTrue Ending scene built successfully with morning wake visual and texture glitch transition!");
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

        private static void AddEndingSceneToBuildSettings()
        {
            string scenePath = "Assets/_Project/Taehui/01.Scenes/Scene_Ending.unity";
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
