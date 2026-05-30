Shader "Custom/DreamyBackground"
{
    Properties
    {
        _ColorA ("Top-Left Color", Color) = (0.05, 0.05, 0.3, 1)
        _ColorB ("Top-Right Color", Color) = (0.15, 0.02, 0.45, 1)
        _ColorC ("Bottom-Left Color", Color) = (0.0, 0.08, 0.35, 1)
        _ColorD ("Bottom-Right Color", Color) = (0.1, 0.0, 0.28, 1)
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveFreq ("Wave Frequency", Float) = 3.0
        _WaveAmp ("Wave Amplitude", Range(0, 0.4)) = 0.15
        _ShimmerSpeed ("Shimmer Speed", Float) = 1.2
        _ShimmerScale ("Shimmer Scale", Float) = 5.0
        _ShimmerIntensity ("Shimmer Intensity", Range(0, 0.4)) = 0.08
        _ColorCycleSpeed ("Color Cycle Speed", Float) = 0.18
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Background-1"
        }
        LOD 100
        ZWrite On
        Cull Off

        Pass
        {
            Name "DreamyBG"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float4 _ColorC;
                float4 _ColorD;
                float  _WaveSpeed;
                float  _WaveFreq;
                float  _WaveAmp;
                float  _ShimmerSpeed;
                float  _ShimmerScale;
                float  _ShimmerIntensity;
                float  _ColorCycleSpeed;
            CBUFFER_END

            // 간단한 해시 기반 스무스 노이즈
            float hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float smoothNoise(float2 uv)
            {
                float2 id = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(id);
                float b = hash21(id + float2(1, 0));
                float c = hash21(id + float2(0, 1));
                float d = hash21(id + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float  t  = _Time.y;

                // ── UV 파동 왜곡 ──
                float2 wuv = uv;
                wuv.x += sin(uv.y * _WaveFreq       + t * _WaveSpeed      ) * _WaveAmp;
                wuv.y += cos(uv.x * _WaveFreq * 0.7 + t * _WaveSpeed * 0.8) * _WaveAmp * 0.5;

                // ── 120° 위상차로 세 개의 싸이클 채널 ──
                float cy = t * _ColorCycleSpeed;
                float s0 = sin(cy         ) * 0.5 + 0.5;
                float s1 = sin(cy + 2.094f) * 0.5 + 0.5;  // +120°
                float s2 = sin(cy + 4.189f) * 0.5 + 0.5;  // +240°

                // ── 4코너 → bilinear 혼합 ──
                float4 top    = lerp(_ColorA, _ColorB, wuv.x);
                float4 bottom = lerp(_ColorC, _ColorD, wuv.x);
                float4 grad   = lerp(bottom, top, wuv.y);

                // 색상 사이클 혼합 (보라→파랑→시안 오로라 계열)
                float4 aurora0 = float4(0.02, 0.0,  0.25, 1);
                float4 aurora1 = float4(0.0,  0.05, 0.35, 1);
                float4 aurora2 = float4(0.08, 0.0,  0.4,  1);
                float4 cycled  = lerp(lerp(aurora0, aurora1, s0), aurora2, s1 * 0.4);
                grad = lerp(grad, cycled, s2 * 0.35);

                // ── 시머 노이즈 오버레이 ──
                float n1 = smoothNoise(wuv * _ShimmerScale + float2(t * 0.2, t * _ShimmerSpeed));
                float n2 = smoothNoise(wuv * _ShimmerScale * 2.0 - float2(t * _ShimmerSpeed * 0.5, 0));
                float shimmer = (n1 * 0.7 + n2 * 0.3) * 2.0 - 1.0;
                grad.rgb += shimmer * _ShimmerIntensity;

                return half4(saturate(grad.rgb), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
