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

    [Header("Shake & Wave Settings")]
    [Tooltip("떨림의 강도")]
    public float shakeAmount = 0.2f;
    
    [Tooltip("떨림의 속도")]
    public float shakeSpeed = 40f;
    
    [Tooltip("일렁이는 크기 변화량 (패턴이 커지는 느낌)")]
    public float waveScaleAmount = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalPos;
    private Vector3 initialLocalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialLocalPos = transform.localPosition;
        initialLocalScale = transform.localScale;
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

        // 3. 크기가 커졌다 작아지며 일렁이는 효과 (패턴/글자가 커지는 느낌 연출)
        // 깜빡임 속도의 절반 속도로 부드럽게 크기가 팽창/수축함
        float wave = Mathf.Sin(Time.time * (pulseSpeed * 0.5f)) * waveScaleAmount;
        transform.localScale = initialLocalScale + new Vector3(wave, wave, 0f);
    }
}
