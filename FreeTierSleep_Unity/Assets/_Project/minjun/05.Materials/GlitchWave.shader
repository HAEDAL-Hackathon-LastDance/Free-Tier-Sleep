Shader "Custom/GlitchWave"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 0.1, 0.1, 0.9)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1.0)) = 0.1
        _ColorDrift ("Color Drift", Range(0, 0.5)) = 0.05
        _Speed ("Wave Speed", Range(0, 50)) = 5.0
        _GridSize ("Grid Size (Pixelation)", Float) = 64.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _GlitchIntensity;
            float _ColorDrift;
            float _Speed;
            float _GridSize;

            // 랜덤 노이즈 생성 함수
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 1. UV 픽셀화 (Blocky Noise)
                float2 blockyUV = floor(uv * _GridSize) / _GridSize;
                
                // 2. 수직 스크롤 시간 적용
                float time = _Time.y * _Speed;
                
                // 3. 픽셀화된 UV와 시간을 기반으로 노이즈 생성
                float noise = random(blockyUV + float2(0, floor(time)));
                
                // 4. 글리치 임계값 적용 (노이즈가 특정 수치 이상일 때만 가로로 찢어짐)
                float glitchOffset = 0.0;
                if (noise > 0.8) 
                {
                    glitchOffset = (noise - 0.8) * _GlitchIntensity;
                }

                // UV 좌표 왜곡 적용
                float2 distortedUV = uv + float2(glitchOffset, 0);

                // 5. 색수차(Chromatic Aberration) 강화
                fixed4 colR = tex2D(_MainTex, distortedUV + float2(_ColorDrift * noise, 0));
                fixed4 colG = tex2D(_MainTex, distortedUV);
                fixed4 colB = tex2D(_MainTex, distortedUV - float2(_ColorDrift * noise, 0));

                // 6. 스캔라인 효과 (가로줄)
                float scanline = sin(uv.y * 300.0 - _Time.y * 20.0) * 0.1 + 0.9;

                // 최종 색상 조합 (글리치 효과 + 붉은색 베이스 컬러 + 스캔라인)
                return fixed4(colR.r, colG.g, colB.b, colG.a) * _BaseColor * scanline;
            }
            ENDCG
        }
    }
}
