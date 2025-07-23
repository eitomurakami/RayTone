Shader "Hidden/G_Transform"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "black" {}
        _Scale("Scale", Vector) = (1, 1, 0, 0)
        _Rotate("Rotate", Float) = 0
        _Translate("Translate", Vector) = (0, 0, 0, 0)
        _Pivot("Pivot", Vector) = (0, 0, 0, 0)
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
            float2 _Scale;
            float _Rotate;
            float2 _Translate;
            float2 _Pivot;
            float2 _Resolution;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Scale
                uv -= _Pivot;
                uv *= 1.0 / _Scale;
                uv += _Pivot;

                // Rotate
                float2 ratio = float2(_Resolution.x / _Resolution.y, 1.0);
                float angle = radians(_Rotate);  // degrees to radians
                uv -= _Pivot;
                uv *= (ratio * _Scale);
                uv = mul(float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)), uv);
                uv /= (ratio * _Scale);
                uv += _Pivot;

                // Translate
                uv -= _Translate * (1.0 / _Scale);

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
