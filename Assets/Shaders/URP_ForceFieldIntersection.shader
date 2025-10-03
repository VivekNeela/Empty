Shader "Custom/ForceFieldIntersection"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.2, 0.6, 1.0, 1.0)
        [HDR]_FresnelColor ("Fresnel Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _OutlineColor ("Outline Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2.0
        _OutlineThickness ("Outline Thickness", Float) = 0.01
        // _IntersectionColor ("Intersection Color", Color) = (1.0, 0.2, 0.2, 1.0)
        // _IntersectionThreshold ("Intersection Threshold", Float) = 0.01
        // _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _Alpha ("Alpha Transparency", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForceFieldIntersection"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;
            float4 _Color;
            float4 _FresnelColor;
            float4 _IntersectionColor;
            float _FresnelPower;
            float _IntersectionThreshold;
            float _ScrollSpeed;
            float _Alpha;

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(normalize(IN.positionOS.xyz));
                OUT.viewDir = normalize(_WorldSpaceCameraPos - OUT.worldPos);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            // Fragment Shader
            half4 frag(Varyings IN) : SV_Target
            {
                // Fresnel Effect
                float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);

                // Scrolling Noise
                // float2 scrollUV = IN.uv + float2(0, _ScrollSpeed * _Time.y);
                // float noise = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scrollUV).r;

                // Combine Fresnel Effect
                float3 baseColor = lerp(_Color.rgb, _FresnelColor.rgb, fresnel);

                // float outline = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _OutlinePower);

                // float3 outlineColor = lerp(baseColor, _OutlineColor.rgb, outline);


                // Intersection Detection
                // Fetch depth from the camera depth texture
                // float sceneDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, IN.screenPos.xy / IN.screenPos.w).r;
                // float linearSceneDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                // float fragmentDepth = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);

                // Compute Intersection Factor
                // float intersectionFactor = saturate(1.0 - abs(fragmentDepth - linearSceneDepth) / _IntersectionThreshold);

                // Blend Intersection Color
                // float3 finalColor = lerp(baseColor, _IntersectionColor.rgb, intersectionFactor);

                // Apply Alpha
                // float alpha = _Alpha * (1.0 - intersectionFactor);

                // return half4(finalColor, alpha);

                // return half4(outlineColor, _Alpha);

                //og code
                return half4(baseColor.rgb, _Alpha);

            }
            ENDHLSL
        }


        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" } // So URP renders it

            Cull Front // Flip culling so backfaces become visible
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Uniforms
            float4 _OutlineColor;
            float _OutlineThickness;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Expand vertices along normals
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // posWS += normalWS * _OutlineThickness;

                float res = posWS * _OutlineThickness;

                OUT.positionCS = TransformWorldToHClip(res);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }




    }
    FallBack "Hidden/InternalErrorShader"
}
