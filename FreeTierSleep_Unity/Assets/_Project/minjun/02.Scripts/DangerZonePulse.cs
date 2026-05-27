using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZonePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("가장 어두울 때의 색상 (진한 피색)")]
    public Color minColor = new Color(0.6f, 0f, 0f, 0.6f);
    
    [Tooltip("가장 밝을 때의 색상 (밝은 네온 레드)")]
    public Color maxColor = new Color(1f, 0.2f, 0.2f, 1f);
    
    [Tooltip("깜빡이는 속도")]
    public float pulseSpeed = 10f;

    [Header("Shake Settings")]
    [Tooltip("떨림의 강도")]
    public float shakeAmount = 0.15f;
    
    [Tooltip("떨림의 속도")]
    public float shakeSpeed = 30f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalPos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialLocalPos = transform.localPosition;
    }

    private void Update()
    {
        // 1. 날카로운 경고등 깜빡임 (Strobe/Alarm effect)
        // Sin 파동을 4제곱하여 짧고 강렬하게 번쩍이도록 처리
        float sinValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float t = Mathf.Pow(sinValue, 4f);
        
        spriteRenderer.color = Color.Lerp(minColor, maxColor, t);

        // 2. 불안정한 느낌을 주기 위한 덜덜 떨리는(Shake) 효과
        // PerlinNoise를 사용하여 불규칙하고 거친 떨림 생성
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount;
        float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeAmount;
        
        transform.localPosition = initialLocalPos + new Vector3(shakeX, shakeY, 0f);
    }
}
