Shader "Hidden/G_Ellipse"
{
    Properties
    {
        _Center("Center", Vector) = (0.5, 0.5, 0, 0)
        _Size("Size", Vector) = (0.5, 0.5, 0, 0)
        _Blur("Blur", Float) = 0
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

            float2 _Center;
            float2 _Size;
            float _Blur;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = _Center;
                float2 size = _Size;
                float blur = _Blur;

                size *= 0.5;  // diameter to radius
                float dist = 1.0 - clamp(distance(i.uv / size, center / size), 0.0, 1.0);

                // lerp solid and blurred
                float result = lerp(ceil(dist), dist, blur);
                return float4(result, result, result, 1.0);
            }
            ENDCG
        }
    }
}
