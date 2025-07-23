Shader "Hidden/G_Brightness"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _Brightness("Brightness", Float) = 1
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
            float _Brightness;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                return float4(tex.rgb * _Brightness, tex.a);
            }
            ENDCG
        }
    }
}
