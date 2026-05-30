using UnityEngine;
using UnityEngine.UI;

namespace Taehui
{
    /// <summary>
    /// UI Image 컴포넌트에 투명한 배경 위 형형색색의 디지털 글리치 블록, 수평 스캔라인 찢어짐(Tearing)을
    /// 매 프레임 동적으로 렌더링하여 세상이 그래픽적으로 붕괴되는 연출을 구현하는 스크립트
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class GraphicCollapseEffect : MonoBehaviour
    {
        private Image image;
        private Texture2D texture;
        private Color[] pixels;
        private int width = 256;
        private int height = 256;

        private void Awake()
        {
            image = GetComponent<Image>();
            
            // 포인트 필터링(Point FilterMode)을 적용하여 복고풍 8비트 청키한 디지털 글리치 느낌 극대화
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            pixels = new Color[width * height];
            
            // 생성한 동적 텍스처를 UI 스프라이트로 등록
            image.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            image.color = Color.white; // Image 자체 컬러는 본연의 색상 출력을 위해 흰색 고정
        }

        private void Update()
        {
            if (texture == null || pixels == null) return;

            // 1. 매 프레임 전체 픽셀을 투명(Clear)하게 리셋하여 배경 이미지(아침 씬)가 보이도록 함
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 0f);
            }

            // 2. 가로로 길게 찢어진 디지털 글리치 바(Horizontal Glitch Bars) 배치
            int barCount = Random.Range(4, 10);
            for (int b = 0; b < barCount; b++)
            {
                int barY = Random.Range(0, height);
                int barHeight = Random.Range(2, 12);
                int barX = Random.Range(0, width / 3);
                int barWidth = Random.Range(width / 3, width);

                Color barColor = GetRandomGlitchColor();

                for (int y = barY; y < barY + barHeight && y < height; y++)
                {
                    for (int x = barX; x < barX + barWidth && x < width; x++)
                    {
                        pixels[y * width + x] = barColor;
                    }
                }
            }

            // 3. 무작위 직사각형 모양의 흩어지는 데이터 블록(Digital Block Noise) 배치
            int blockCount = Random.Range(15, 30);
            for (int bl = 0; bl < blockCount; bl++)
            {
                int blX = Random.Range(0, width);
                int blY = Random.Range(0, height);
                int blW = Random.Range(4, 25);
                int blH = Random.Range(4, 25);

                Color blockColor = GetRandomGlitchColor();

                for (int y = blY; y < blY + blH && y < height; y++)
                {
                    for (int x = blX; x < blX + blW && x < width; x++)
                    {
                        pixels[y * width + x] = blockColor;
                    }
                }
            }

            // 4. 아주 날카로운 수평 지지직 단색 라인(Thin scanlines) 추가
            if (Random.value > 0.15f)
            {
                int lineCount = Random.Range(6, 15);
                for (int l = 0; l < lineCount; l++)
                {
                    int lineY = Random.Range(0, height);
                    Color lineColor = Random.value > 0.5f ? Color.white : new Color(0f, 1f, 1f, 0.85f);
                    for (int x = 0; x < width; x++)
                    {
                        pixels[lineY * width + x] = lineColor;
                    }
                }
            }

            // 5. 변경 데이터 적용 및 업로드
            texture.SetPixels(pixels);
            texture.Apply();
        }

        /// <summary>
        /// 디지털 붕괴 분위기를 극대화할 형형색색의 네온 빛 원색을 무작위 추출
        /// </summary>
        private Color GetRandomGlitchColor()
        {
            float rand = Random.value;
            if (rand < 0.22f) return new Color(1f, 0.05f, 0.4f, 0.9f);    // 핫 핑크 (Magenta)
            if (rand < 0.44f) return new Color(0f, 1f, 0.95f, 0.9f);     // 네온 시안 (Cyan)
            if (rand < 0.66f) return new Color(0.1f, 0.95f, 0.15f, 0.9f); // 네온 라임 그린 (Lime Green)
            if (rand < 0.85f) return new Color(1f, 0.9f, 0f, 0.9f);       // 일렉트릭 옐로우 (Yellow)
            return new Color(1f, 1f, 1f, 0.95f);                         // 강렬한 화이트 (White)
        }
    }
}
