Shader "Custom/RayMarching"
{
    Properties
    {

        _Color("Color", Color) = (1,0,0,1)
        _Radius("Sphere Radius", Float) = 0.5
        _BlendSharpness("Blend Sharpness", Float) = 0.5
        _PosOffset("Offset", vector) = (0,0,0,0)
        _MaxSteps("Max Steps", Int) = 64
        _MaxDistance("Max Ray Distance", Float) = 5.0
        _SurfaceEpsilon("Surface Threshold", Float) = 0.001

    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _Radius;
            float _BlendSharpness;
            float4 _Color;
            int _MaxSteps;
            float _MaxDistance;
            float _SurfaceEpsilon;
            float3 _PosOffset;

            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = world.xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // Signed distance to a sphere centered at origin in local space
            float sdfSphere(float3 p, float r)
            {
                return length(p) - r;
            }

            float sdfTorus(float3 p, float2 t)
            {
                // t.x = major radius (from center to ring)
                // t.y = minor radius (radius of the tube)
                float2 q = float2(length(p.xz) - t.x, p.y);
                return length(q) - t.y;
            }

            float smoothMinExp(float a, float b, float k)
            {
                float res = exp(-k*a) + exp(-k*b);
                return -log(res) / k;
            }

            float smoothMinPoly(float a, float b, float k)
            {
                float h = saturate(0.5 + 0.5 * (b - a) / k);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            


            float4 frag(v2f i) : SV_Target
            {
                // World-space ray origin and direction
                float3 ro = _WorldSpaceCameraPos;

                float3 rd = normalize(i.worldPos - ro);

                float t = 0.0;

                float t2 = 0.0;


                for (int j = 0; j < _MaxSteps; j++)
                {
                    float3 worldPoint = ro + rd * t;

                    // float3 worldPoint2 = ro + rd * t2;
                    
                    float3 localPoint = mul(unity_WorldToObject, float4(worldPoint, 1.0)).xyz;

                    float3 localPoint2 = mul(unity_WorldToObject, float4(worldPoint + _PosOffset, 1.0)).xyz;

                    float dist1 = sdfSphere(localPoint, _Radius); 

                    float dist2 = sdfSphere(localPoint2, _Radius * 0.2); // Second sphere with half the radius

                    float dist = smoothMinPoly(dist1, dist2, 0.1);


                    if (dist < _SurfaceEpsilon)
                    {
                        // Basic stuff
                        // if(dist1 < dist2)
                        // return _Color;
                        // else
                        // return float4(0, 1, 0, .7f); // Return green for the second sphere

                        // Color blend
                        float influence1 = exp(-_BlendSharpness * dist1);
                        float influence2 = exp(-_BlendSharpness * dist2);
                        float total = influence1 + influence2;
                        float3 blendedColor = (_Color.rgb * influence1 + float3(0,1.,0) * influence2) / total;

                        return float4(blendedColor, 1.0);


                    }

  
                    t += dist;

                    // t2 += dist2;
                    
                    if (t > _MaxDistance)
                    break;
                }

                return float4(0, 0, 0, 0); // Return transparent if no hit  
            }

            ENDCG

            
        }
    }
}









