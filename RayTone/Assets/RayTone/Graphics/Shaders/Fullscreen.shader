Shader "Hidden/Fullscreen"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _Scale("Scale", Float) = 1
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

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                sampler2D _MainTex;
                float _Scale;

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv * float2(_Scale, 1.0) + float2((1 - _Scale) * 0.5, 0);
                    return tex2D(_MainTex, uv) * (uv.x >= 0 && uv.x < 1.0);
                }
                ENDCG
            }
        }
}
