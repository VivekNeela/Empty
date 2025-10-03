Shader "Unlit/CracksShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CrackColor("Crack Color", Color) = (0, 0, 0, 1)
        _CrackScale("Crack Scale", Float) = 10.0
        _CrackWidth("Crack Width", Range(0.001, 0.2)) = 0.05
        _CrackIntensity("Crack Intensity", Range(0, 1)) = 1.0
        _Direction("Direction", vector) = (1, 0, 0, 0) // Default to horizontal scrolling
        _ScrollSpeed("Scroll Speed", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _CrackColor;
            float _CrackScale;
            float _CrackWidth;
            float _CrackIntensity;
            float _ScrollSpeed;
            float4 _Direction;


            // -- -- -- -- Worley Noise Function -- -- -- --
            float worley(float2 uv)
            {
                uv *= _CrackScale;

                float2 i = floor(uv);
                float2 f = frac(uv);

                float minDist = 1.0;

                // check neighboring cells
                [unroll(9)]
                for (int y = - 1; y <= 1; y ++)
                {
                    for (int x = - 1; x <= 1; x ++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cell = i + neighbor;

                        // random point in cell
                        float2 rand = frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
                        float2 featurePoint = neighbor + rand;

                        float d = distance(f, featurePoint);
                        minDist = min(minDist, d);
                    }
                }
                return minDist;
            }


            // -- -- -- - hash -- -- -- --
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // -- -- -- - noise -- -- -- --
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // -- -- -- - fractal brownian motion -- -- -- --
            float fbm(float2 p)
            {
                float f = 0;
                float a = 0.5;
                float2x2 rot = float2x2(1.6, - 1.2, 1.2, 1.6);
                for (int i = 0; i < 5; i ++) {
                    f += a * noise(p); // "noise" is a Perlin / simplex function
                    p = mul(rot, p) * 2.0;
                    a *= 0.5;
                }
                return f;
            }



            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = TransformObjectToHClip(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //scrolling the uv
                // o.uv += _Direction * _Time.y * _ScrollSpeed;

                return o;
            }


            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float n = worley(i.uv + _Time.y * _Direction.xy);

                // cracks are thin lines where distance is small
                float cracks = smoothstep(_CrackWidth, 1.0, n);

                float2 warp = float2(fbm(i.uv * 4.0), fbm(i.uv * 4.0 + 10.0));
                float warpedFbm = fbm(i.uv * 8.0 + warp * 2.0);

                float dist = abs(i.uv.x - 0.5); // distance from vertical line at center

                float core = exp(- dist * 80.0); // sharp bright line
                float glow = exp(- dist * 20.0); // softer outer glow

                float lightning = core + glow * warpedFbm;


                float3 finalCol = lerp(col, _CrackColor.rgb, cracks * _CrackIntensity);

                // return float4(finalCol, col.a);
                return float4(finalCol, col.a); // add lightning effect

            }
            ENDHLSL
        }
    }
}
