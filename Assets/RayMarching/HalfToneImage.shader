Shader "Custom/HalfToneImage"
{
    Properties
    {
        _MainTex ("Circle Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 5
        // _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.0, 0, 0)
        _ScaleCenter ("Scale Center", Vector) = (0.1, 0.0, 0, 0)
        _MinScale ("Min Scale", Float) = 0.3
        _MaxScale ("Max Scale", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent" 
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Tiling;
            // float4 _ScrollSpeed;
            float4 _ScaleCenter;
            float _MinScale;
            float _MaxScale;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // float2 scroll = _Time.y * _ScrollSpeed.xy * 1.5; // Adjust speed multiplier as needed
                // o.uv += _ScrollSpeed.xy * _Time.y ; // Apply scrolling effect    
                
                return o;
            }

            float computeTileScale(float2 tileID, float2 distanceFromCenter)
            {
                // Normalize tileID to [0,1] based on expected tiling count
                float2 normTile = tileID / _Tiling;

                // Example: scale based on distance from center of UV space
                float dist = distance(normTile, distanceFromCenter);
                return lerp(_MaxScale, _MinScale, dist * 2.0); // scale falls off outward
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float2 scroll = _Time.y * _ScrollSpeed.xy * 1.5; // Adjust speed multiplier as needed
                float2 tiledUV = i.uv * _Tiling ;

                // Identify the tile we're in
                float2 tileID = floor(tiledUV);
                float2 tileLocalUV = frac(tiledUV); // [0-1] UV inside the tile

                // Scale UVs within the tile based on tile ID
                float scale = computeTileScale(tileID, _ScaleCenter); // Center of the tile

                // Scale the local UV around tile center
                float2 scaledLocalUV = (tileLocalUV - 0.5) / saturate(scale) + 0.5;

                // Sample texture using local scaled UV
                fixed4 col = tex2D(_MainTex, scaledLocalUV);

                // Optional: fade out when scaled too small or outside bounds
                if (scaledLocalUV.x < 0.0 || scaledLocalUV.x > 1.0 || scaledLocalUV.y < 0.0 || scaledLocalUV.y > 1.0)
                    col *= 0.0;

                return col;
            }
            ENDCG
        }
    }
}
