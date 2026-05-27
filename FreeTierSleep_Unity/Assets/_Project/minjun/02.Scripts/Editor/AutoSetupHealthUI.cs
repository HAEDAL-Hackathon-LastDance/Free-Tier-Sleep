using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AutoSetupHealthUI : Editor
{
    [MenuItem("Tools/Free Tier Sleep/Auto Setup Health UI")]
    public static void SetupHealthUI()
    {
        // 1. Canvas 찾기 또는 생성
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. EventSystem 확인
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 3. Health UI 컨테이너 생성
        GameObject healthPanel = new GameObject("HealthUI_Panel");
        healthPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = healthPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1); // 좌측 상단 앵커
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20); // 여백
        panelRect.sizeDelta = new Vector2(300, 100);

        HorizontalLayoutGroup layout = healthPanel.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 10;
        layout.childControlHeight = false;
        layout.childControlWidth = false;

        // 4. HealthUIManager 컴포넌트 추가
        HealthUIManager uiManager = healthPanel.AddComponent<HealthUIManager>();
        uiManager.heartImages = new Image[3];

        // 5. 하트 이미지 3개 생성
        for (int i = 0; i < 3; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}");
            heartObj.transform.SetParent(healthPanel.transform, false);
            
            Image heartImage = heartObj.AddComponent<Image>();
            heartImage.color = Color.red; // 임시로 빨간색 사각형으로 표시
            
            RectTransform heartRect = heartObj.GetComponent<RectTransform>();
            heartRect.sizeDelta = new Vector2(50, 50); // 하트 크기

            uiManager.heartImages[i] = heartImage;
        }

        EditorUtility.SetDirty(canvas);
        Debug.Log("체력 UI 자동 세팅이 완료되었습니다! 씬을 저장해 주세요.");
    }
}
