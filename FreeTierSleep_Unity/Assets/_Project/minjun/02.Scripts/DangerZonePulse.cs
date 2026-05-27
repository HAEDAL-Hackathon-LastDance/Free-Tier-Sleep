using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZonePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("가장 어두울 때의 색상")]
    public Color minColor = new Color(0.5f, 0f, 0f, 0.4f);
    
    [Tooltip("가장 밝을 때의 색상")]
    public Color maxColor = new Color(1f, 0f, 0f, 0.8f);
    
    [Tooltip("깜빡이는 속도")]
    public float pulseSpeed = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Sin 함수를 사용하여 0 ~ 1 사이의 값을 부드럽게 왕복
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        
        // 두 색상 사이를 보간하여 경고등처럼 깜빡이는 효과 연출
        spriteRenderer.color = Color.Lerp(minColor, maxColor, t);
    }
}
