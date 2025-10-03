Shader "Hidden/MaskShader"
{
    Properties
    {
        _Mask ("Mask Texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType"="Opaque" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "DebugMaskPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.uv).r;

                // Red = masked objects, White = everything else
                return mask > 0.5 ? half4(1, 0, 0, 1) : half4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
}
