Shader "Custom/2D_SSR"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _ReflectionOpacity ("Reflection Opacity", Range(0,1)) = 0.4
        _FadeStart ("Fade Start Y", Float) = 0.0
        _FadeEnd ("Fade End Y", Float) = 1.0
        _DistortionStrength ("Distortion Strength", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass
        {
           

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _CameraOpaqueTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _ReflectionOpacity;
            float _FadeStart;
            float _FadeEnd;
            float _DistortionStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenUV = ComputeScreenPos(o.pos).xy / o.pos.w;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Flip screen UV vertically
                float2 reflUV = i.screenUV;
                reflUV.y = 1.0 - reflUV.y;

                // Add simple sine wave distortion
                reflUV.x += sin(i.worldPos.y * 30.0 + _Time.y * 5.0) * _DistortionStrength;

                // Sample the screen as the reflection
                half4 reflColor = tex2D(_CameraOpaqueTexture, reflUV);

                // Fade based on world Y
                float fade = saturate((i.worldPos.y - _FadeStart) / (_FadeEnd - _FadeStart));
                float alpha = _ReflectionOpacity * (1.0 - fade);

                return half4(reflColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
