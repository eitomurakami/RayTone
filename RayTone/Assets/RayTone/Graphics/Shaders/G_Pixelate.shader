Shader "Hidden/G_Pixelate"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _Size("Size", Vector) = (1, 1, 0, 0)
        _Resolution("Resolution", Vector) = (1920, 1080, 0, 0)
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

            sampler2D _MainTex;
            float2 _Size;
            float2 _Resolution;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = floor(i.uv * _Resolution / _Size) / _Resolution * _Size;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
