using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZonePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("가장 어두울 때의 색상 (진한 피색)")]
    public Color minColor = new Color(0.8f, 0f, 0f, 0.8f);
    
    [Tooltip("가장 밝을 때의 색상 (순수한 레드)")]
    public Color maxColor = new Color(1f, 0f, 0f, 1f);
    
    [Tooltip("깜빡이는 속도")]
    public float pulseSpeed = 12f;

    [Header("Shake Settings")]
    [Tooltip("떨림의 강도")]
    public float shakeAmount = 0.2f;
    
    [Tooltip("떨림의 속도")]
    public float shakeSpeed = 40f;

    [Header("Pattern Settings")]
    [Tooltip("패턴의 밀도/크기 조절 (1보다 작으면 패턴이 커지고 굵어짐, 1보다 크면 빼곡해짐)")]
    public float patternScale = 0.5f;
    
    [Tooltip("패턴을 강제로 굵게(Bold) 만드는 수치. 얇은 선을 두껍게 겹쳐서 렌더링합니다. (0이면 사용 안 함, 0.05~0.15 권장)")]
    public float patternThickness = 0.08f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalPos;
    private SpriteRenderer[] boldRenderers;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialLocalPos = transform.localPosition;

        // DangerZone만의 패턴 크기를 조절하기 위해 머티리얼 인스턴스 접근
        if (spriteRenderer.material != null)
        {
            // 텍스처 타일링을 조절하여 패턴을 굵게/크게 또는 빼곡하게 만듦
            spriteRenderer.material.mainTextureScale = new Vector2(patternScale, patternScale);
        }

        // Faux Bold (가짜 굵기) 효과 생성: 상하좌우로 미세하게 겹쳐서 얇은 선을 두껍게 만듦
        if (patternThickness > 0f)
        {
            boldRenderers = new SpriteRenderer[4];
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(patternThickness, 0, 0),
                new Vector3(-patternThickness, 0, 0),
                new Vector3(0, patternThickness, 0),
                new Vector3(0, -patternThickness, 0)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject child = new GameObject($"BoldOverlay_{i}");
                child.transform.SetParent(transform);
                child.transform.localPosition = offsets[i];
                child.transform.localScale = Vector3.one;

                SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = spriteRenderer.sprite;
                sr.material = spriteRenderer.material; // 공유 머티리얼 사용
                sr.sortingLayerID = spriteRenderer.sortingLayerID;
                sr.sortingOrder = spriteRenderer.sortingOrder;
                sr.drawMode = spriteRenderer.drawMode;
                sr.size = spriteRenderer.size;
                
                boldRenderers[i] = sr;
            }
        }
    }

    private void Update()
    {
        // 1. 날카로운 경고등 깜빡임 (Strobe/Alarm effect)
        float sinValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float t = Mathf.Pow(sinValue, 2f);
        
        Color currentColor = Color.Lerp(minColor, maxColor, t);
        spriteRenderer.color = currentColor;

        // 굵기용으로 생성된 자식 스프라이트들도 색상 동기화
        if (boldRenderers != null)
        {
            for (int i = 0; i < boldRenderers.Length; i++)
            {
                if (boldRenderers[i] != null)
                {
                    boldRenderers[i].color = currentColor;
                }
            }
        }

        // 2. 불안정한 느낌을 주기 위한 덜덜 떨리는(Shake) 효과
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount;
        float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeAmount;
        
        transform.localPosition = initialLocalPos + new Vector3(shakeX, shakeY, 0f);
    }
}
