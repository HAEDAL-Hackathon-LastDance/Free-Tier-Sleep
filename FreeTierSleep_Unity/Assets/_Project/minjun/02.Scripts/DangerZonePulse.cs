using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZonePulse : MonoBehaviour
{
    [Header("Glitch Settings")]
    [Tooltip("글리치 텍스처가 갱신되는 주기 (초)")]
    public float updateInterval = 0.05f;
    
    [Tooltip("글리치 텍스처의 가로 해상도 (낮을수록 픽셀아트 느낌)")]
    public int textureWidth = 64;
    
    [Tooltip("글리치 텍스처의 세로 해상도")]
    public int textureHeight = 64;
    
    [Header("Color Settings")]
    [Tooltip("기본적으로 깔리는 위험 영역의 색상")]
    public Color mainDangerColor = new Color(0.8f, 0f, 0f, 0.6f);
    
    [Tooltip("가로줄 글리치에 사용될 다양한 색상들")]
    public Color[] glitchColors = new Color[] {
        Color.red, Color.magenta, Color.yellow, Color.black, Color.white, Color.cyan
    };

    private SpriteRenderer spriteRenderer;
    private Texture2D glitchTexture;
    private float timer;
    private Color[] pixels;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 절차적(Procedural) 텍스처 생성
        glitchTexture = new Texture2D(textureWidth, textureHeight);
        glitchTexture.filterMode = FilterMode.Point; // 픽셀이 뚜렷하게 보이도록 설정
        glitchTexture.wrapMode = TextureWrapMode.Clamp;
        
        // 텍스처를 기반으로 새로운 스프라이트 생성 (Pixels Per Unit = 100)
        Sprite newSprite = Sprite.Create(glitchTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 100f);
        spriteRenderer.sprite = newSprite;
        
        // 기본 스프라이트 머티리얼 사용 (부모의 글리치 머티리얼 사용 안 함)
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        pixels = new Color[textureWidth * textureHeight];
        GenerateGlitch();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            GenerateGlitch();
        }
    }

    private void GenerateGlitch()
    {
        for (int y = 0; y < textureHeight; y++)
        {
            // 해당 줄(Row)에 글리치를 발생시킬지 결정
            bool isGlitchRow = Random.value > 0.4f; // 60% 확률로 글리치 줄 생성 (빼곡하게)
            
            Color rowColor = mainDangerColor;
            int streakStart = 0;
            int streakLength = textureWidth;

            if (isGlitchRow)
            {
                // 다채로운 색상 중 하나 선택
                rowColor = glitchColors[Random.Range(0, glitchColors.Length)];
                rowColor.a = Random.Range(0.6f, 1f); // 불투명도 랜덤
                
                // 가로로 찢어지는 느낌을 위해 시작점과 길이를 랜덤하게 설정
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
                    // 기본 붉은색 배경 (약간의 노이즈 추가)
                    Color baseCol = mainDangerColor;
                    baseCol.a = Random.Range(0.3f, 0.7f);
                    pixels[index] = baseCol;
                }
            }
        }
        
        glitchTexture.SetPixels(pixels);
        glitchTexture.Apply();
    }

    // 동적으로 생성한 텍스처, 스프라이트, 머티리얼은 가비지 컬렉터가 자동으로 지워주지 않으므로
    // 오브젝트 파괴 시 명시적으로 메모리에서 해제하여 메모리 누수를 방지합니다.
    private void OnDestroy()
    {
        if (glitchTexture != null) 
            Destroy(glitchTexture);
            
        if (spriteRenderer != null)
        {
            if (spriteRenderer.sprite != null) 
                Destroy(spriteRenderer.sprite);
                
            if (spriteRenderer.material != null) 
                Destroy(spriteRenderer.material);
        }
    }
}
