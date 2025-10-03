Shader "Custom/BrushStampNoTex"
{
    Properties
    {
        _MainTex ("Canvas", 2D) = "white" {}
        _BrushColor ("Brush Color", Color) = (0,0,0,1)
        _BrushCenter("Brush Center (UV)", Vector) = (0.5,0.5,0,0)
        _BrushSize ("Brush Radius (UV)", Float) = 0.05
        _Hardness ("Edge Hardness", Range(0,1)) = 0.8
        _Opacity ("Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BrushColor;
            float4 _BrushCenter;   // xy = UV coords of brush center
            float _BrushSize;      // brush radius in UV (0..1)
            float _Hardness;       // 0 = very soft, 1 = hard edge
            float _Opacity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Existing canvas pixel
                fixed4 oldCol = tex2D(_MainTex, i.uv);

                // Distance from this pixel to brush center (in UV space)
                float dist = distance(i.uv, _BrushCenter.xy);

                // Normalize distance: 0 at center, 1 at edge
                float t = saturate(dist / _BrushSize);

                // Brush alpha falloff: hardness controls sharpness
                float brushAlpha = 1.0 - pow(t, lerp(8.0, 64.0, _Hardness));
                brushAlpha = saturate(brushAlpha);

                // Final alpha (with opacity)
                float blendAlpha = brushAlpha * _Opacity;

                // Blend brush color onto canvas
                fixed4 outCol = lerp(oldCol, _BrushColor, 1);

                return outCol;
            }
            ENDCG
        }
    }
}
