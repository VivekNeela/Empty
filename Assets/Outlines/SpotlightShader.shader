Shader "Unlit/SpotlightShader"
{
    Properties
    {
        _ConeTip("Cone Tip", Vector) = (0, 0, 0, 1)
        _IntersectionSize("Intersection Size", Range(0, 1)) = 0.1
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100
        Cull Off

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
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1; // screen - space position
            };

            float4 _ConeTip;
            float4 _Color;
            float _IntersectionSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex); // required for depth sampling
                return o;
            }

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);


            float4 frag (v2f i) : SV_Target
            {
                // screen UV
                float2 uv = i.screenPos.xy / i.screenPos.w;

                // sample scene depth at this fragment
                float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;

                // convert both depths to linear eye depth
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float fragDepth = LinearEyeDepth(i.screenPos.z / i.screenPos.w, _ZBufferParams);

                // check occlusion (is something closer than this fragment?)
                bool occluded = (sceneDepth < fragDepth + _IntersectionSize);

                if (occluded)
                {
                    return float4(_Color.xyz, 1);
                }

                // visible → draw cone color
                return _Color;
                // return float4(rawDepth.xxx, 1); // visualize raw depth
            }
            ENDHLSL
        }
    }
}
