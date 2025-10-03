Shader "Custom/Dither"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _FadeDistance ("Fade Distance", Range(0,2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 200
        ZWrite On

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _FadeDistance;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        // 4x4 Bayer dithering pattern
        static const float ditherMatrix[16] =
        {
              0.0,  8.0,  2.0, 10.0,
              12.0, 4.0,  14.0, 6.0,
              3.0,  11.0, 1.0,  9.0,
              15.0, 7.0,  13.0, 5.0
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        //dither Functions...
        
        // Compute dithering threshold based on screen position
        float DitherPattern(float2 screenUV)
        {
            int x = fmod(screenUV.x * _ScreenParams.x, 4);
            int y = fmod(screenUV.y * _ScreenParams.y, 4);
            int index = y * 4 + x;
            return ditherMatrix[index] / 16.0;
        }




        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            float3 camToPixel = IN.worldPos - _WorldSpaceCameraPos.xyz;
            float distance = length(camToPixel);
            float fadeFactor = saturate(distance / _FadeDistance);

            float ditherTreshold = DitherPattern(IN.screenPos.xy / IN.screenPos.w);

            if(fadeFactor < ditherTreshold)
                discard;

            // Albedo comes from a texture tinted by color
            float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            //applying the fade factor
            o.Alpha = fadeFactor;
        
        
        }
        ENDCG
    }
    FallBack "Diffuse"
}
