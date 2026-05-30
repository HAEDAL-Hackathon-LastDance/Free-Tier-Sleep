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
    public class StartSceneAutoSetup : EditorWindow
    {
        [MenuItem("Tools/Taehui/Setup Start Scene - AUTO")]
        public static void FullAutoStartSetup()
        {
            // 1. 새로운 씬 생성
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "Scene_Start";
            }

            // 2. 폰트 에셋 찾기 (DungGeunMo SDF 우선)
            TMP_FontAsset koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/suyeon/03. art/DungGeunMo SDF.asset");
            if (koreanFont == null)
            {
                string[] guids = AssetDatabase.FindAssets("DungGeunMo SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            // 3. 캔버스 구성
            GameObject canvasObj = new GameObject("StartCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Taehui/03.Art/title_background.png");
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.color = Color.white;
                
                // 화면 해상도 변화 시 이미지 비율 유지하며 꽉 차게 조절 (EnvelopeParent)
                AspectRatioFitter fitter = bgObj.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = (float)bgSprite.rect.width / bgSprite.rect.height;
            }
            else
            {
                bgImage.color = new Color(0.01f, 0.01f, 0.03f, 1f); // 짙은 사이버네틱 어둠
            }
            SetFullStretch(bgObj.GetComponent<RectTransform>());

            // 타이틀 텍스트 구성 (해상도 비례 정렬 및 발광 효과)
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(canvasObj.transform, false);
            RectTransform titleRect = titleTextObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.72f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleTmp = titleTextObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) titleTmp.font = koreanFont;
            titleTmp.text = "Free-Tier Sleep";
            titleTmp.fontSize = 96; // 조금 더 키움
            
            // 은은한 그라데이션 및 외곽선 이펙트 추가 (CRT 광채 느낌)
            titleTmp.enableVertexGradient = true;
            titleTmp.colorGradient = new VertexGradient(
                new Color(0.2f, 0.85f, 0.8f, 1f),   // 상단 청록빛
                new Color(0.2f, 0.85f, 0.8f, 1f),   
                new Color(0.1f, 0.6f, 0.55f, 1f),   // 하단 살짝 어두운 톤
                new Color(0.1f, 0.6f, 0.55f, 1f)
            );
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.outlineColor = new Color32(10, 30, 30, 255);
            titleTmp.outlineWidth = 0.3f;

            // 부팅 시네마틱 타이핑 텍스트
            GameObject bootTextObj = new GameObject("BootTypingText");
            bootTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI bootTmp = bootTextObj.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) bootTmp.font = koreanFont;
            bootTmp.fontSize = 32; // 글자 크기 조금 키움
            bootTmp.color = new Color(0f, 0.9f, 0.5f, 1f); // 터미널 그린 컬러
            bootTmp.alignment = TextAlignmentOptions.TopLeft;
            bootTmp.text = "";
            TypingEffect bootTypingEffect = bootTextObj.AddComponent<TypingEffect>();
            
            RectTransform bootTextRect = bootTextObj.GetComponent<RectTransform>();
            bootTextRect.anchorMin = new Vector2(0.15f, 0.38f);
            bootTextRect.anchorMax = new Vector2(0.85f, 0.68f);
            bootTextRect.anchoredPosition = Vector2.zero;
            bootTextRect.sizeDelta = Vector2.zero;

            // 설정창 팝업 패널 구성 (비활성 상태로 배치)
            GameObject optionsPopupObj = CreateOptionsPopupPanel(canvasObj.transform, koreanFont);
            optionsPopupObj.SetActive(false);
            OptionsPopup optionsPopup = optionsPopupObj.GetComponent<OptionsPopup>();

            // 메뉴 패널 구성 (해상도 비율 대응 및 겹침 방지)
            GameObject menuPanelObj = new GameObject("MenuPanel");
            menuPanelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform menuPanelRect = menuPanelObj.AddComponent<RectTransform>();
            menuPanelRect.anchorMin = new Vector2(0.35f, 0.06f);
            menuPanelRect.anchorMax = new Vector2(0.65f, 0.34f);
            menuPanelRect.anchoredPosition = Vector2.zero;
            menuPanelRect.sizeDelta = Vector2.zero;

            // 세로 정렬 그룹 레이아웃
            VerticalLayoutGroup vLayout = menuPanelObj.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 20; // 버튼 스페이싱 증가
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;

            // 3종 버튼들 생성
            Button btnAccess = CreateMenuButton(menuPanelObj.transform, "AccessCloudButton", "수면 접속", koreanFont);
            Button btnConfig = CreateMenuButton(menuPanelObj.transform, "SystemConfigButton", "환경 설정", koreanFont);
            Button btnDisconnect = CreateMenuButton(menuPanelObj.transform, "DisconnectButton", "연결 종료", koreanFont);

            // 페이드 오버레이
            GameObject overlayObj = new GameObject("FadeOverlay");
            overlayObj.transform.SetParent(canvasObj.transform, false);
            Image overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.color = Color.black;
            CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0;
            overlayGroup.blocksRaycasts = false;
            SetFullStretch(overlayObj.GetComponent<RectTransform>());

            // URP Volume 및 기존 프로파일 연결
            string profilePath = "Assets/_Project/Taehui/05.Prefabs/IntroVolumeProfile.asset";
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            GameObject volumeObj = new GameObject("GlobalVolume");
            Volume globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            if (volumeProfile != null)
            {
                globalVolume.profile = volumeProfile;
            }

            // 5. 메인 컨트롤러 구성
            GameObject controllerObj = new GameObject("StartSceneController");
            StartSceneController controller = controllerObj.AddComponent<StartSceneController>();
            AssignField(controller, "menuPanel", menuPanelObj);
            AssignField(controller, "optionsPopup", optionsPopupObj);
            AssignField(controller, "bootTypingEffect", bootTypingEffect);
            AssignField(controller, "blackOverlay", overlayGroup);
            AssignField(controller, "postProcessVolume", globalVolume);

            // 루프용 메타 필드 레퍼런스 주입
            AssignField(controller, "titleText", titleTmp);
            AssignField(controller, "accessButtonText", btnAccess.GetComponentInChildren<TextMeshProUGUI>());
            AssignField(controller, "configButtonText", btnConfig.GetComponentInChildren<TextMeshProUGUI>());
            AssignField(controller, "disconnectButtonText", btnDisconnect.GetComponentInChildren<TextMeshProUGUI>());

            // 버튼 클릭 리스너 스크립트 바인딩
            btnAccess.onClick.AddListener(controller.AccessCloud);
            btnConfig.onClick.AddListener(controller.ShowConfig);
            btnDisconnect.onClick.AddListener(controller.Disconnect);

            AddStartSceneToBuildSettings();

            Debug.Log("★ START SCENE AUTO SETUP COMPLETE ★\nTitle scene generated at Taehui/01.Scenes/Scene_Start.unity!");
        }

        private static Button CreateMenuButton(Transform parent, string name, string text, TMP_FontAsset font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            // 레이아웃 크기 명시적 지정으로 글자 겹침 및 세로 짜부러짐 방지
            LayoutElement layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 75f; // 버튼 높이 증가
            
            // UI Button 및 Image 컴포넌트
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.1f, 0.6f); // 반투명 짙은 청회색
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            // 컬러 블록 설정 (레트로 스타일)
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.05f, 0.05f, 0.1f, 0.6f);
            colors.highlightedColor = new Color(0.1f, 0.2f, 0.3f, 0.8f);
            colors.pressedColor = new Color(0.2f, 0.4f, 0.6f, 1f);
            colors.selectedColor = new Color(0.1f, 0.2f, 0.3f, 0.8f);
            colors.disabledColor = Color.gray;
            btn.colors = colors;

            // 텍스트 자식 오브젝트 구성
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = 30; // 버튼 글자 크기 키움
            tmp.color = new Color(0f, 0.9f, 0.9f, 1f); // 쨍한 네온 시안
            tmp.alignment = TextAlignmentOptions.Center;
            SetFullStretch(textObj.GetComponent<RectTransform>());

            // 호버 글리치 스크립트 및 사운드 연결
            btnObj.AddComponent<MenuButtonEffect>();

            return btn;
        }

        private static GameObject CreateOptionsPopupPanel(Transform parent, TMP_FontAsset font)
        {
            GameObject popupObj = new GameObject("OptionsPopup");
            popupObj.transform.SetParent(parent, false);
            RectTransform popRect = popupObj.AddComponent<RectTransform>();
            popRect.anchorMin = new Vector2(0.2f, 0.2f);
            popRect.anchorMax = new Vector2(0.8f, 0.8f);
            popRect.anchoredPosition = Vector2.zero;
            popRect.sizeDelta = Vector2.zero;

            Image bgImg = popupObj.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.95f); // 불투명 짙은 배경

            // 타이틀 텍스트
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(popupObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTmp.font = font;
            titleTmp.text = "::: 시스템 환경 설정 :::";
            titleTmp.fontSize = 24;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // 볼륨 조절 라벨 텍스트
            GameObject labelObj = new GameObject("VolumeLabel");
            labelObj.transform.SetParent(popupObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.15f, 0.55f);
            labelRect.anchorMax = new Vector2(0.85f, 0.65f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            if (font != null) labelTmp.font = font;
            labelTmp.text = "마스터 볼륨 데시벨 제어";
            labelTmp.fontSize = 20;
            labelTmp.color = new Color(0f, 0.9f, 0.5f, 1f);
            labelTmp.alignment = TextAlignmentOptions.Left;

            // 슬라이더 바 구성
            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(popupObj.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.15f, 0.4f);
            sliderRect.anchorMax = new Vector2(0.85f, 0.5f);
            sliderRect.anchoredPosition = Vector2.zero;
            sliderRect.sizeDelta = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            
            // 슬라이더 배경(Background)
            GameObject slBg = new GameObject("Background");
            slBg.transform.SetParent(sliderObj.transform, false);
            Image bg = slBg.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            SetFullStretch(slBg.GetComponent<RectTransform>());

            // 슬라이더 필 영역(Fill Area)
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRect = fillArea.AddComponent<RectTransform>();
            SetFullStretch(faRect);
            faRect.sizeDelta = new Vector2(-20, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fImg = fill.AddComponent<Image>();
            fImg.color = new Color(0f, 0.9f, 0.5f, 1f); // 네온 그린색 채우기
            RectTransform fRect = fill.GetComponent<RectTransform>();
            if (fRect == null) fRect = fill.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = new Vector2(1f, 1f);
            fRect.sizeDelta = Vector2.zero;

            slider.fillRect = fRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // 닫기 버튼 구성
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(popupObj.transform, false);
            RectTransform cbRect = closeBtnObj.AddComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.35f, 0.12f);
            cbRect.anchorMax = new Vector2(0.65f, 0.25f);
            cbRect.anchoredPosition = Vector2.zero;
            cbRect.sizeDelta = Vector2.zero;
            Image cbImg = closeBtnObj.AddComponent<Image>();
            cbImg.color = new Color(0.2f, 0.05f, 0.05f, 0.8f); // 짙은 붉은색 닫기 버튼
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = cbImg;
            closeBtnObj.AddComponent<MenuButtonEffect>();

            GameObject cbTxtObj = new GameObject("Text");
            cbTxtObj.transform.SetParent(closeBtnObj.transform, false);
            TextMeshProUGUI cbTxt = cbTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) cbTxt.font = font;
            cbTxt.text = "설정 저장 및 종료";
            cbTxt.fontSize = 18;
            cbTxt.color = Color.white;
            cbTxt.alignment = TextAlignmentOptions.Center;
            SetFullStretch(cbTxtObj.GetComponent<RectTransform>());

            // OptionsPopup 컴포넌트 탑재 및 필드 바인딩
            OptionsPopup optionsPopup = popupObj.AddComponent<OptionsPopup>();
            AssignField(optionsPopup, "volumeSlider", slider);
            AssignField(optionsPopup, "closeButton", closeBtn);

            return popupObj;
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

        private static void AddStartSceneToBuildSettings()
        {
            string startScenePath = "Assets/_Project/Taehui/01.Scenes/Scene_Start.unity";
            string introScenePath = "Assets/_Project/Taehui/01.Scenes/Scene_Intro.unity";

            var scene = EditorSceneManager.GetActiveScene();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, startScenePath);

            // 빌드 세팅에 추가 및 순서 정렬 (Start 씬을 최상단 Index 0으로 등록)
            var currentScenes = EditorBuildSettings.scenes;
            bool startExists = false;
            bool introExists = false;

            foreach (var s in currentScenes)
            {
                if (s.path == startScenePath) startExists = true;
                if (s.path == introScenePath) introExists = true;
            }

            // 필요한 씬 추가를 위해 배열 크기 튜닝
            int newSize = currentScenes.Length;
            if (!startExists) newSize++;
            if (!introExists) newSize++;

            var newScenes = new EditorBuildSettingsScene[newSize];
            int idx = 0;

            // StartScene을 Index 0에 배치
            newScenes[idx++] = new EditorBuildSettingsScene(startScenePath, true);

            // IntroScene을 Index 1에 배치
            newScenes[idx++] = new EditorBuildSettingsScene(introScenePath, true);

            // 나머지 씬들을 이어서 배치
            foreach (var s in currentScenes)
            {
                if (s.path == startScenePath || s.path == introScenePath) continue;
                newScenes[idx++] = s;
            }

            EditorBuildSettings.scenes = newScenes;
        }
    }
}
