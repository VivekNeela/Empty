Shader "Custom/URPSoftIntersectionLit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            float4 _BaseColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

                o.positionHCS = TransformWorldToHClip(positionWS);
                o.screenPos = ComputeScreenPos(o.positionHCS);
                o.normalWS = normalWS;
                o.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Normalize vectors
                float3 normal = normalize(i.normalWS);
                float3 viewDir = normalize(i.viewDirWS);

                // Get main directional light
                Light light = GetMainLight();
                float3 lightDir = normalize(light.direction);
                float NdotL = saturate(dot(normal, lightDir));

                // Diffuse Lambert lighting
                float3 litColor = _BaseColor.rgb * light.color * NdotL;

                // Soft intersection
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float sceneRawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
                float linearSceneDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                float linearObjDepth = i.screenPos.z;

                float depthDiff = linearSceneDepth - linearObjDepth;
                float fade = saturate(smoothstep(0.0, 0.1, depthDiff));

                float alpha = _BaseColor.a * fade;

                return float4(litColor * fade, alpha);
            }
            ENDHLSL
        }
    }
}
