Shader "Custom/URP_Dithering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _FadeDistance ("Fade Distance", Float) = 5.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.5
            // #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _BaseColor;
            float _FadeDistance;
            float _Smoothness;
            float _Metallic;
            
            static const float ditherMatrix[16] = {
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0, 14.0, 6.0,
                3.0, 11.0, 1.0,  9.0,
                15.0, 7.0, 13.0, 5.0
            };
            
            float Dither(float2 screenUV)
            {
                int2 pixelPos = int2(fmod(screenUV * _ScreenParams.xy, 4));
                int index = pixelPos.y * 4 + pixelPos.x;
                return ditherMatrix[index] / 16.0;
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.vertex.xyz);
                OUT.worldPos = worldPos;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.vertex.xyz);
                OUT.vertex = posInputs.positionCS;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                OUTPUT_LIGHTMAP_UV(IN.vertex.xy, unity_LightmapST, OUT.lightmapUV);
                OUTPUT_SH(OUT.normalWS, OUT.vertexSH);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float3 camToFrag = IN.worldPos - _WorldSpaceCameraPos.xyz;
                float dist = length(camToFrag);
                float fade = saturate(dist / _FadeDistance);
                float ditherThreshold = Dither(IN.screenPos.xy / IN.screenPos.w);
                clip(fade - ditherThreshold);
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = IN.shadowCoord;
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, normalWS);
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = texColor.rgb * _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = 0;
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.alpha = 1.0;
                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // :white_tick: SHADOW CASTER PASS
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };
            
            float _FadeDistance;
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.vertex.xyz);
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.vertex.xyz);
                output.positionCS = posInputs.positionCS;
                output.uv = input.uv;
                output.worldPos = worldPos;
                return output;
            }
            
            float DitherShadow(float2 screenUV)
            {
                int2 pixelPos = int2(fmod(screenUV * _ScreenParams.xy, 4));
                int index = pixelPos.y * 4 + pixelPos.x;
                static const float ditherMatrix[16] = {
                    0.0,  8.0,  2.0, 10.0,
                    12.0, 4.0, 14.0, 6.0,
                    3.0, 11.0, 1.0,  9.0,
                    15.0, 7.0, 13.0, 5.0
                };
                return ditherMatrix[index] / 16.0;
            }
            
            float4 ShadowPassFragment(Varyings IN) : SV_Target
            {
                float dist = length(IN.worldPos - _WorldSpaceCameraPos.xyz);
                float fade = saturate(dist / _FadeDistance);
                // float ditherThreshold = DitherShadow(IN.uv);
                // clip(fade - ditherThreshold);
                return 0;
            }
            ENDHLSL
        }
    }
    // FallBack "Hidden/InternalErrorShader"
}









