Shader "Hidden/G_FBM"
{
    Properties
    {
        _Offset("Offset", Vector) = (0, 0, 0, 0)
        _Scale("Scale", Vector) = (1, 1, 0, 0)
        _Octaves("Octaves", Int) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Generate a pseudo-random number
            // https://thebookofshaders.com/10/
            float random(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Generate 2D value noise
            // https://thebookofshaders.com/11/
            float noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                // Smooth Interpolation
                float2 u = smoothstep(0.0, 1.0, f);

                // Mix 4 coorners percentages
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float2 _Offset;
            float2 _Scale;
            int _Octaves;

            // https://thebookofshaders.com/13/
            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _Offset;
                float2 scale = _Scale;
                int octaves = _Octaves;
                float value = 0.0;
                float amplitude = 0.5;
                float2 uv = (i.uv + offset) * scale;

                float2 shift = float2(100.0, 100.0);

                // Rotate to reduce axial bias
                float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
                
                // Loop of octaves
                for (int j = 0; j < octaves; ++j)
                {
                    value += amplitude * noise(uv);
                    uv = mul(rot, uv) * 2.0 + shift;
                    amplitude *= 0.5;
                }

                return float4(value, value, value, 1.0);
            }
            ENDCG
        }
    }
}
