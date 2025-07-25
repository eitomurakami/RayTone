Shader "Hidden/G_Displace"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _SecondTex("SecondTex", 2D) = "black" {}
        _Weight("Weight", Vector) = (0, 0, 0, 0)
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
            sampler2D _SecondTex;
            float2 _Weight;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv + (tex2D(_SecondTex, i.uv).rg * 2.0 - 1.0) * _Weight;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
