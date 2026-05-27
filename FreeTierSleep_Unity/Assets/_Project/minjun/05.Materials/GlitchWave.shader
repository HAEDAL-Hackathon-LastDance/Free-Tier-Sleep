Shader "Custom/GlitchWave"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 0.1, 0.1, 0.9)
        _GlitchIntensity ("Glitch Intensity", Range(0, 0.5)) = 0.05
        _ColorDrift ("Color Drift", Range(0, 0.5)) = 0.02
        _Speed ("Wave Speed", Range(0, 50)) = 10.0
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
                
                // 시간에 따른 불규칙한 파도(Wave) 생성
                float time = _Time.y * _Speed;
                float wave = sin(time + uv.y * 20.0) * cos(time * 0.5 + uv.y * 10.0);
                
                // 픽셀화된 느낌을 줄때 사용할 글리치 노이즈 생성
                float noise = random(float2(floor(uv.y * 10.0), floor(time * 5.0)));
                float distortion = wave * noise * _GlitchIntensity;

                // UV 좌표 왜곡 적용
                float2 distortedUV = uv + float2(distortion, 0);

                // RGB 채널 분리 (Chromatic Aberration)
                fixed4 colR = tex2D(_MainTex, distortedUV + float2(_ColorDrift * noise, 0));
                fixed4 colG = tex2D(_MainTex, distortedUV);
                fixed4 colB = tex2D(_MainTex, distortedUV - float2(_ColorDrift * noise, 0));

                // 최종 색상 조합 (글리치 효과 + 붉은색 베이스 컬러)
                return fixed4(colR.r, colG.g, colB.b, colG.a) * _BaseColor;
            }
            ENDCG
        }
    }
}
