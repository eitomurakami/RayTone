Shader "Hidden/G_Hue"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _Hue("Hue", Float) = 0
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

            void Unity_Hue_Normalized_float(float3 In, float Offset, out float3 Out)
            {
                // RGB to HSV
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
                float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
                float D = Q.x - min(Q.w, Q.y);
                float E = 1e-10;
                float V = (D == 0) ? Q.x : (Q.x + E);
                float3 hsv = float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), V);

                float hue = hsv.x + Offset;
                hsv.x = (hue < 0)
                    ? hue + 1
                    : (hue > 1)
                    ? hue - 1
                    : hue;

                // HSV to RGB
                float4 K2 = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
                Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
            }

            sampler2D _MainTex;
            float _Hue;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float3 result;
                Unity_Hue_Normalized_float(tex.rgb, _Hue, result);
                return float4(result, tex.a);
            }
            ENDCG
        }
    }
}
