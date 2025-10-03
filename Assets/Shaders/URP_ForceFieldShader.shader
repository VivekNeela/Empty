Shader "Custom/ForceFieldShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {} // Noise texture
        _Color ("Base Color", Color) = (0.2, 0.6, 1.0, 1.0)
        _FresnelColor ("Fresnel Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2.0
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _Alpha ("Alpha Transparency", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForceField"
            Tags { "LightMode"="UniversalForward" }

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
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;
            float4 _Color;
            float4 _FresnelColor;
            float _FresnelPower;
            float _DistortionStrength;
            float _ScrollSpeed;
            float _Alpha;

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(normalize(IN.positionOS.xyz));
                OUT.viewDir = normalize(_WorldSpaceCameraPos - OUT.worldPos);
                return OUT;
            }

            // Fragment Shader
            half4 frag(Varyings IN) : SV_Target
            {
                // Fresnel Effect
                float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);

                // Scrolling Noise
                float2 scrollUV = IN.uv + float2(0, _ScrollSpeed * _Time.y);
                float noise = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scrollUV).r;

                // Distortion Effect
                float2 distortedUV = IN.uv + (noise - 0.5) * _DistortionStrength;
                float4 distortedTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);

                // Combine Fresnel and Distortion
                float3 finalColor = lerp(_Color.rgb, _FresnelColor.rgb, fresnel) * distortedTex.rgb;

                // Apply Alpha
                float alpha = _Alpha * fresnel;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
