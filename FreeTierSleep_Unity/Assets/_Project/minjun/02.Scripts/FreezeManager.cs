using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 전역 프리즈 상태 매니저
// - 프리즈 아이템 획득 시 일정 시간 동안 화면을 파랗게 덮고 모든 적/발판의 동작을 정지시킴
// - 적/발판 스크립트는 FreezeManager.IsFrozen만 체크하면 됨
// - 씬에 미리 배치할 필요 없이 첫 접근 시 자동 생성됨
public class FreezeManager : MonoBehaviour
{
    private static FreezeManager _instance;

    // 외부에서 동작 무시 여부만 체크할 수 있도록 static 노출
    public static bool IsFrozen => _instance != null && _instance._isFrozen;

    [Header("Visual Settings")]
    [Tooltip("프리즈 시 화면을 덮는 오버레이 색상 (알파는 fade target)")]
    public Color overlayColor = new Color(0.2f, 0.6f, 1f, 0.35f);

    [Tooltip("프리즈 시작/종료 시 페이드 시간 (초)")]
    public float fadeDuration = 0.3f;

    private bool _isFrozen;
    private Image overlayImage;
    private Coroutine freezeCoroutine;

    public static FreezeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("FreezeManager");
                _instance = obj.AddComponent<FreezeManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        CreateOverlayUI();
    }

    // 씬의 기존 Canvas에 풀스크린 오버레이 Image를 추가
    // 기존 Canvas가 없을 때만 자체 Canvas를 폴백으로 생성
    private void CreateOverlayUI()
    {
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        Transform parent;

        if (existingCanvas != null)
        {
            parent = existingCanvas.transform;
        }
        else
        {
            GameObject canvasObj = new GameObject("FreezeOverlayCanvas");
            canvasObj.transform.SetParent(transform);
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            parent = canvasObj.transform;
        }

        GameObject imgObj = new GameObject("FreezeOverlay");
        imgObj.transform.SetParent(parent, false);
        imgObj.transform.SetAsLastSibling();

        overlayImage = imgObj.AddComponent<Image>();
        overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        overlayImage.raycastTarget = false;

        RectTransform rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // 외부(FreezeItem 등)에서 호출
    public void StartFreeze(float duration)
    {
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _isFrozen = true;
        EnemySpawner.Instance?.ClearAllToPool();
        yield return Fade(0f, overlayColor.a, fadeDuration);

        yield return new WaitForSeconds(Mathf.Max(0f, duration - fadeDuration * 2f));

        yield return Fade(overlayColor.a, 0f, fadeDuration);
        _isFrozen = false;
        freezeCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        if (overlayImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / time);
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, a);
            yield return null;
        }
        overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, to);
    }
}
