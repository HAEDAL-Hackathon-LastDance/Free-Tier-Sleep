using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Taehui;

namespace Taehui.Editor
{
    public class IntroSceneAutoSetup : EditorWindow
    {
        [MenuItem("Tools/Taehui/SUPER FULL AUTO - Setup Intro Scene")]
        public static void FullAutoSetup()
        {
            // 1. 광고용 팝업 프리팹 4종 생성 및 이미지 연결
            string[] adSpritePaths = new string[]
            {
                "Assets/_Project/Taehui/03.Art/ad_spam_01.png",
                "Assets/_Project/Taehui/03.Art/ad_spam_02.png",
                "Assets/_Project/Taehui/03.Art/ad_spam_03.png",
                "Assets/_Project/Taehui/03.Art/ad_spam_04.png"
            };

            GameObject[] adPrefabs = new GameObject[adSpritePaths.Length];
            for (int i = 0; i < adSpritePaths.Length; i++)
            {
                string prefabPath = $"Assets/_Project/Taehui/05.Prefabs/AdPopup_0{i + 1}.prefab";
                adPrefabs[i] = CreateAdPopupPrefab(prefabPath, adSpritePaths[i], $"AdPopup_0{i + 1}");
            }

            // 2. 새로운 씬 생성
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "Scene_Intro";
            }

            // 3. 폰트 에셋 찾기 (DungGeunMo SDF 우선)
            TMP_FontAsset koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/suyeon/03. art/DungGeunMo SDF.asset");
            if (koreanFont == null)
            {
                string[] guids = AssetDatabase.FindAssets("DungGeunMo SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            // 4. 캔버스 구성
            GameObject canvasObj = new GameObject("IntroCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.01f, 0.01f, 0.03f, 1f);
            SetFullStretch(bgObj.GetComponent<RectTransform>());

            // 주인공 픽셀 아트 캐릭터 배치 (UI Canvas 내부)
            GameObject playerObj = new GameObject("PlayerCharacter");
            playerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform playerRect = playerObj.AddComponent<RectTransform>();
            playerRect.sizeDelta = new Vector2(128, 128); // 32x32 크기를 보기 좋게 4배 확대
            playerRect.anchoredPosition = new Vector2(0, -50); // 화면 중앙에서 약간 하단
            Image playerImage = playerObj.AddComponent<Image>();
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Taehui/03.Art/player_sleeping.png");
            if (playerSprite != null)
            {
                playerImage.sprite = playerSprite;
            }
            playerImage.color = Color.white;

            // 광케이블(랜선) UI 연출 라인
            GameObject cableObj = new GameObject("LanCable");
            cableObj.transform.SetParent(playerObj.transform, false);
            RectTransform cableRect = cableObj.AddComponent<RectTransform>();
            cableRect.anchorMin = new Vector2(0.5f, 0.5f);
            cableRect.anchorMax = new Vector2(0.5f, 1f);
            cableRect.anchoredPosition = new Vector2(10, 0); // 머리 뒤 위치 조정
            cableRect.sizeDelta = new Vector2(6, 600); // 굵기 6px
            cableRect.pivot = new Vector2(0.5f, 0f);
            Image cableImage = cableObj.AddComponent<Image>();
            cableImage.color = new Color(0.05f, 0.12f, 0.15f, 1f); // 짙은 네온빛 남청색 베이스 케이블
            
            // 데이터 발광 패킷 흐름 연출 적용 (docs/05 기획 기반)
            cableObj.AddComponent<CableDataFlow>();

            // 광고 영역
            GameObject adAreaObj = new GameObject("AdSpawnArea");
            adAreaObj.transform.SetParent(canvasObj.transform, false);
            RectTransform adRect = adAreaObj.AddComponent<RectTransform>();
            SetFullStretch(adRect);
            AdPopupManager adManager = adAreaObj.AddComponent<AdPopupManager>();
            AssignField(adManager, "adPrefabs", adPrefabs);
            AssignField(adManager, "spawnArea", adRect);

            // 타이핑 대사창 패널 (가독성 향상용 반투명 배경)
            GameObject panelObj = new GameObject("TypingPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.04f);
            panelRect.anchorMax = new Vector2(0.92f, 0.26f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.02f, 0.02f, 0.04f, 0.85f); // 어두운 85% 투명도

            // 타이핑 텍스트 (패널의 자식으로 구성)
            GameObject textObj = new GameObject("TypingText");
            textObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) tmp.font = koreanFont;
            
            tmp.fontSize = 28;
            tmp.color = Color.white; // 완벽한 화이트로 또렷하게 표시
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "CONNECTING...";
            TypingEffect typingEffect = textObj.AddComponent<TypingEffect>();
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(-40, -20); // 상하좌우 여백을 주어 패널 테두리를 벗어나지 않게 함

            // 페이드 오버레이
            GameObject overlayObj = new GameObject("FadeOverlay");
            overlayObj.transform.SetParent(canvasObj.transform, false);
            Image overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.color = Color.black;
            CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0;
            overlayGroup.blocksRaycasts = false;
            SetFullStretch(overlayObj.GetComponent<RectTransform>());

            // URP Volume 및 프로파일 생성 & 설정
            string profilePath = "Assets/_Project/Taehui/05.Prefabs/IntroVolumeProfile.asset";
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (volumeProfile == null)
            {
                volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                
                // Chromatic Aberration 추가 및 초기값 설정
                var chromatic = volumeProfile.Add<ChromaticAberration>(true);
                chromatic.intensity.Override(0f);
                
                // Lens Distortion 추가 및 초기값 설정
                var lens = volumeProfile.Add<LensDistortion>(true);
                lens.intensity.Override(0f);
                lens.scale.Override(1f);
                
                string folder = Path.GetDirectoryName(profilePath);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                AssetDatabase.CreateAsset(volumeProfile, profilePath);
            }
            
            GameObject volumeObj = new GameObject("GlobalVolume");
            Volume globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.profile = volumeProfile;

            // 5. 메인 컨트롤러
            GameObject controllerObj = new GameObject("IntroSceneController");
            IntroSceneController controller = controllerObj.AddComponent<IntroSceneController>();
            AssignField(controller, "typingEffect", typingEffect);
            AssignField(controller, "adPopupManager", adManager);
            AssignField(controller, "blackOverlay", overlayGroup);
            AssignField(controller, "postProcessVolume", globalVolume);

            AddCurrentSceneToBuildSettings();

            Debug.Log("★ SUPER FULL AUTO SETUP COMPLETE ★\n4 custom ad prefabs created, player sprite placed, and URP Volume configured!");
        }

        private static GameObject CreateAdPopupPrefab(string path, string spritePath, string name)
        {
            if (File.Exists(path))
            {
                // 프리팹이 이미 존재할 경우, 스프라이트 정보 갱신을 위해 로드 후 갱신
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Image imgComp = existing.GetComponent<Image>();
                Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (imgComp != null && spr != null)
                {
                    imgComp.sprite = spr;
                    imgComp.color = Color.white;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 200);
            Image img = go.AddComponent<Image>();
            
            // 생성된 사이버펑크 팝업 이미지 연결
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                img.sprite = sprite;
            }
            img.color = Color.white;

            // 팝업 광고 내부의 부가적인 네온 스타일 테두리나 디자인을 위해 텍스트는 제외
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
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

        private static void AddCurrentSceneToBuildSettings()
        {
            string scenePath = "Assets/_Project/Taehui/01.Scenes/Scene_Intro.unity";
            var scene = EditorSceneManager.GetActiveScene();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, scenePath);

            var scenes = EditorBuildSettings.scenes;
            bool exists = false;
            foreach (var s in scenes) if (s.path == scenePath) exists = true;
            if (!exists)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                System.Array.Copy(scenes, newScenes, scenes.Length);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
            }
        }
    }
}
