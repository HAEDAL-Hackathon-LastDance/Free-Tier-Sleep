Shader "Custom/GlitchWave"
{
    Properties
    {
        _BaseColor ("Primary Color (Red)", Color) = (1.0, 0.1, 0.1, 1.0)
        _SecondaryColor ("Secondary Color (Green)", Color) = (0.1, 1.0, 0.1, 1.0)
        _ColorMixRatio ("Color Mix Ratio", Range(0, 1)) = 0.5
        _EffectSpeed ("Effect Scroll Speed", Range(0, 50)) = 10.0
        _GridSize ("Grid Density", Float) = 40.0
        _Brightness ("Brightness", Range(0, 5)) = 1.5
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

            float4 _BaseColor;
            float4 _SecondaryColor;
            float _ColorMixRatio;
            float _EffectSpeed;
            float _GridSize;
            float _Brightness;

            // 2D 랜덤 함수
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // 가짜 문자(숫자/알파벳 느낌)를 생성하는 함수
            float fakeCharacter(float2 cellId, float2 innerUV)
            {
                // 셀 내부를 3x5 픽셀 그리드로 쪼개어 문자 형태를 흉내냄
                float2 subGrid = floor(innerUV * float2(3.0, 5.0));
                
                // 각 서브 픽셀마다 랜덤 값을 주어 켜거나 끔 (0.5 기준)
                float pixelRand = random(cellId + subGrid * 0.137);
                
                // 테두리 부분은 약간 비워두어 글자 간격을 만듦
                float margin = step(0.1, innerUV.x) * step(innerUV.x, 0.9) * step(0.1, innerUV.y) * step(innerUV.y, 0.9);
                
                return step(0.5, pixelRand) * margin;
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
                
                // 1. 화면을 세로 기둥(Column)으로 나눔
                float columns = uv.x * _GridSize;
                float colId = floor(columns);
                
                // 2. 기둥마다 상승 속도와 시작 위치(Offset)를 다르게 설정
                // 여기서 _EffectSpeed를 사용하여 시각적 스크롤 속도를 결정합니다.
                float colSpeed = _EffectSpeed * (0.5 + 0.5 * random(float2(colId, 0.0)));
                float colOffset = random(float2(colId, 1.0)) * 100.0;
                
                // 3. 기둥별 색상 결정 (Primary vs Secondary)
                float colorRand = random(float2(colId, 2.0));
                float4 streamColor = lerp(_BaseColor, _SecondaryColor, step(_ColorMixRatio, colorRand));
                
                // 4. 위로 솟구치는 행(Row) 계산
                // uv.y에 시간을 빼주면 위로 올라가는 효과가 생김
                float rows = uv.y * _GridSize - _Time.y * colSpeed + colOffset;
                float rowId = floor(rows);
                
                float2 cellId = float2(colId, rowId);
                float2 innerUV = frac(float2(columns, rows));
                
                // 5. 가짜 문자 형태 생성
                float charShape = fakeCharacter(cellId, innerUV);
                
                // 6. 문자의 밝기 랜덤화 (깜빡이는 느낌)
                float charBrightness = random(cellId + float2(0.0, _Time.y * 0.1));
                
                // 7. 꼬리(Trail) 효과: 기둥의 특정 길이만큼 그라데이션으로 사라짐
                float trail = frac(rows * 0.05);
                trail = smoothstep(0.1, 0.9, trail); // 부드러운 페이드 아웃
                
                // 8. 최종 강도 계산 (문자 형태 * 밝기 * 꼬리)
                float finalIntensity = charShape * charBrightness * trail * _Brightness;
                
                // 9. 결정된 색상 적용 및 알파 블렌딩
                fixed4 finalColor = streamColor * finalIntensity;
                
                // 알파값이 1을 초과하여 렌더링이 깨지거나 하얗게 타는 것을 방지
                finalColor.a = saturate(finalIntensity); 
                
                return finalColor;
            }
            ENDCG
        }
    }
}
