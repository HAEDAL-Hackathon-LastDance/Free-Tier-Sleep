using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZonePulse : MonoBehaviour
{
    [Header("Glitch Settings")]
    [Tooltip("글리치 텍스처가 갱신되는 주기 (초)")]
    public float updateInterval = 0.2f;

    [Tooltip("글리치 텍스처의 가로 해상도 (낮을수록 픽셀아트 느낌)")]
    public int textureWidth = 64;

    [Tooltip("글리치 텍스처의 세로 해상도")]
    public int textureHeight = 64;

    [Header("Performance")]
    [Tooltip("Awake에서 미리 생성해둘 글리치 프레임 수. 매 프레임 픽셀 재계산 대신 이 중에서 순환 재생 → CPU/GPU 부하 대폭 감소")]
    public int prebakedFrameCount = 8;
    
    [Header("Color Settings")]
    [Tooltip("기본적으로 깔리는 위험 영역의 색상")]
    public Color mainDangerColor = new Color(0.8f, 0f, 0f, 0.6f);
    
    [Tooltip("가로줄 글리치에 사용될 다양한 색상들")]
    public Color[] glitchColors = new Color[] {
        Color.red, Color.magenta, Color.yellow, Color.black, Color.white, Color.cyan
    };

    private SpriteRenderer spriteRenderer;
    private Sprite[] prebakedSprites;   // Awake에서 한 번만 굽고 이후엔 참조만 교체
    private int currentFrameIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // 사전 렌더링: N장의 글리치 텍스처를 만들어두고 순환 재생
        prebakedSprites = new Sprite[Mathf.Max(2, prebakedFrameCount)];
        Color[] pixels = new Color[textureWidth * textureHeight];

        for (int i = 0; i < prebakedSprites.Length; i++)
        {
            Texture2D tex = new Texture2D(textureWidth, textureHeight);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            FillGlitchPixels(pixels);
            tex.SetPixels(pixels);
            tex.Apply();

            prebakedSprites[i] = Sprite.Create(tex, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 100f);
        }

        spriteRenderer.sprite = prebakedSprites[0];
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            // 매 프레임 픽셀 재계산 없이 sprite 참조만 교체 (사실상 무료)
            currentFrameIndex = (currentFrameIndex + 1) % prebakedSprites.Length;
            spriteRenderer.sprite = prebakedSprites[currentFrameIndex];
        }
    }

    // 한 프레임 분량의 글리치 픽셀을 pixels 배열에 채워넣음 (할당 없음, 재사용 가능)
    private void FillGlitchPixels(Color[] pixels)
    {
        for (int y = 0; y < textureHeight; y++)
        {
            bool isGlitchRow = Random.value > 0.4f; // 60% 확률로 글리치 줄

            Color rowColor = mainDangerColor;
            int streakStart = 0;
            int streakLength = textureWidth;

            if (isGlitchRow)
            {
                rowColor = glitchColors[Random.Range(0, glitchColors.Length)];
                rowColor.a = Random.Range(0.6f, 1f);
                streakStart = Random.Range(0, textureWidth / 2);
                streakLength = Random.Range(textureWidth / 4, textureWidth);
            }

            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;
                if (isGlitchRow && x >= streakStart && x < streakStart + streakLength)
                {
                    pixels[index] = rowColor;
                }
                else
                {
                    Color baseCol = mainDangerColor;
                    baseCol.a = Random.Range(0.3f, 0.7f);
                    pixels[index] = baseCol;
                }
            }
        }
    }

    // 동적 생성 텍스처/스프라이트/머티리얼은 GC 대상이 아니므로 명시적으로 해제
    private void OnDestroy()
    {
        if (prebakedSprites != null)
        {
            foreach (var s in prebakedSprites)
            {
                if (s == null) continue;
                if (s.texture != null) Destroy(s.texture);
                Destroy(s);
            }
        }

        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            Destroy(spriteRenderer.material);
        }
    }
}
