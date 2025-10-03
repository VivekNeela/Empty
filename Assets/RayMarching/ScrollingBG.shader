Shader "Custom/ScrollingBG"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollSpeed("Scroll Speed", Float) = 0.5
        _Direction("Direction", Vector) = (1, 0, 0, 0) // Default to horizontal scrolling
        _ScaleFactor("Scale Factor", Float) = 0.5
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollSpeed; 
            float4 _Direction; // Direction vector for scrolling    
            float _ScaleFactor; // Scale factor for the texture


            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                o.uv *= _ScaleFactor;

                o.uv += _Direction * _Time.y * _ScrollSpeed; 

                
                // // Local-space X condition
                // if (v.vertex.x < 0.0)
                // {
                    //     o.uv *= 0.5; // Scale down texture on left side of mesh
                // }


                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                return col;

                //for debugging

                float u = frac(i.uv.xy);
                
                return float4(u, u, u, 1); // Return a color based on the u coordinate  
                
            }
            ENDCG
        }
    }
}
