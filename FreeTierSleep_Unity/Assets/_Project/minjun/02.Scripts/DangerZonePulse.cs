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

    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalPos;

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
    }

    private void Update()
    {
        // 1. 날카로운 경고등 깜빡임 (Strobe/Alarm effect)
        // 붉은색이 더 강하고 오래 유지되도록 제곱 수치를 낮춤
        float sinValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float t = Mathf.Pow(sinValue, 2f);
        
        spriteRenderer.color = Color.Lerp(minColor, maxColor, t);

        // 2. 불안정한 느낌을 주기 위한 덜덜 떨리는(Shake) 효과
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount;
        float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeAmount;
        
        transform.localPosition = initialLocalPos + new Vector3(shakeX, shakeY, 0f);
    }
}
