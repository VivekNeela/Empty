Shader "CustomEffects/Outlines"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        _OutlineThickness("Thickness", Float) = 0.5
        _DepthSensitivity("Depth Sens", Float) = 0.5
        _NormalSensitivity("Normal Sens", Float) = 0.5
        _OutlineColor("Outline Color", Color) = (1, 0, 0, 1) // Default to horizontal scrolling
        // _ScaleFactor("Scale Factor", Float) = 0.5
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    // The Blit.hlsl file provides the vertex shader (Vert),
    // the input structure (Attributes), and the output structure (Varyings)
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _VerticalBlur;
    float _HorizontalBlur;
    // sampler2D _Mask;


    float4 BlurVertical (Varyings input) : SV_Target
    {
        const float BLUR_SAMPLES = 64;
        const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;

        float3 color = 0;
        float blurPixels = _VerticalBlur * _ScreenParams.y;

        for(float i = - BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i ++)
        {
            float2 sampleOffset = float2 (0, (blurPixels / _BlitTexture_TexelSize.w) * (i / BLUR_SAMPLES_RANGE));
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + sampleOffset).rgb;
        }

        return float4(color.rgb / (BLUR_SAMPLES + 1), 1);
    }

    float4 BlurHorizontal (Varyings input) : SV_Target
    {
        const float BLUR_SAMPLES = 64;
        const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;

        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float3 color = 0;
        float blurPixels = _HorizontalBlur * _ScreenParams.x;
        for(float i = - BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i ++)
        {
            float2 sampleOffset =
            float2 ((blurPixels / _BlitTexture_TexelSize.z) * (i / BLUR_SAMPLES_RANGE), 0);
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + sampleOffset).rgb;
        }
        return float4(color / (BLUR_SAMPLES + 1), 1);
    }

    // Input textures
    TEXTURE2D(_CameraDepthTexture);
    SAMPLER(sampler_CameraDepthTexture);

    TEXTURE2D(_CameraNormalsTexture);
    SAMPLER(sampler_CameraNormalsTexture);

    TEXTURE2D(_Mask);
    SAMPLER(sampler_Mask);

    TEXTURE2D(_MaskDepth);
    SAMPLER(sampler_MaskDepth);

    TEXTURE2D(_NormalTex);
    SAMPLER(sampler_NormalTex);



    float _OutlineThickness; // how far to sample in screen space
    float _DepthSensitivity; // how sensitive to depth edges
    float _NormalSensitivity; // how sensitive to normal edges
    float4 _OutlineColor;

    // Depth sampling
    float SampleDepth(float2 uv)
    {
        return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
        // return SAMPLE_TEXTURE2D(_MaskDepth, sampler_MaskDepth, uv).r;
    }

    // Normal sampling
    float3 SampleNormal(float2 uv)
    {
        float4 encoded = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv);
        return normalize(encoded.xyz * 2.0 - 1.0); // decode normal

        // float4 encoded = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uv);
        // return normalize(encoded.xyz * 2.0 - 1.0); // decode normal
    }

    //debug normals (looks cool)
    float3 ShowDebugNormals(float2 input)
    {
        // Decode normal (remap from [0, 1] → [ - 1, 1])
        float3 encodedNormal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, input).rgb;
        float3 normal = normalize(encodedNormal * 2.0 - 1.0);

        // Remap back to [0, 1] so it can be shown on screen
        float3 debugColor = normal * 0.5 + 0.5;
        return debugColor;
    }


    float4 Frag(Varyings input) : SV_Target
    {
        //Original version
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


        // Sample scene Color
        float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

        // Sample mask texture
        float mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.texcoord).r;

        // Sample mask depth texture
        float maskDepth = SAMPLE_TEXTURE2D(_MaskDepth, sampler_MaskDepth, input.texcoord).r;

        // Sample normals texture
        float normals = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, input.texcoord).r;

        //shows the mask depth
        // return float4(maskDepth.xxxx);
        //shows the normals tex

        // if(SampleDepth(input.texcoord) > maskDepth + .0001)
        // return float4(1, 0, 0, 1);
        // else
        // return float4(0, 1, 0, 1);

        // Early out : if not part of target layer, return scene color
        if (mask <= 0.0) //draw outlines...
        return sceneColor;

        // if((SampleDepth(input.texcoord) - maskDepth) > 0.0001)
        if(SampleDepth(input.texcoord) > maskDepth + 0.0001)
        return sceneColor;


        // -- -- -- Edge Detection (Edge lord)
        float2 texelSize = _ScreenParams.zw * _OutlineThickness;

        float depthCenter = SampleDepth(input.texcoord);
        float3 normalCenter = SampleNormal(input.texcoord);

        float edge = 0.0;

        // sample four neighbors (can expand to 8 for stronger outlines)
        float2 offsets[4] = {
            float2(texelSize.x, 0),
            float2(- texelSize.x, 0),
            float2(0, texelSize.y),
            float2(0, - texelSize.y)
        };

        //this loops over all neighbouring pixels of the current pixel
        [unroll]
        for (int i = 0; i < 4; i ++)
        {

            float2 uv = input.texcoord + offsets[i];

            float depth = SampleDepth(uv);
            float3 normal = SampleNormal(uv);

            float neighborMask = maskDepth;

            // ✅ if neighbor is also masked, skip edge detection
            // if (neighborMask > 0.0)
            // continue;

            // depth difference
            float depthDiff = abs(depthCenter - depth) * _DepthSensitivity;

            // normal difference
            float normalDiff = 1.0 - dot(normalCenter, normal);
            normalDiff *= _NormalSensitivity;

            edge = max(edge, max(depthDiff, normalDiff));
        }

        edge = saturate(edge);


        //for showing normals
        // return float4(ShowDebugNormals(input.texcoord), 1.0);

        return lerp(sceneColor, _OutlineColor, edge);


    }



    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        ZTest Always
        Cull Off

        // 🔹 Important : Only run on pixels where stencil == 1
        Stencil
        {
            Ref 1
            Comp Equal
            Pass Keep
        }


        //Blur passes
        // Pass
        // {
            // Name "BlurPassVertical"

            // HLSLPROGRAM

            // #pragma vertex Vert
            // #pragma fragment BlurVertical

            // ENDHLSL
        // }

        // Pass
        // {
            // Name "BlurPassHorizontal"

            // HLSLPROGRAM

            // #pragma vertex Vert
            // #pragma fragment BlurHorizontal

            // ENDHLSL
        // }

        Pass
        {
            Name "OutlinesPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }


    }
}