Shader "Hidden/G_Rect"
{
    Properties
    {
        _Center("Center", Vector) = (0.5, 0.5, 0, 0)
        _Size("Size", Vector) = (0.5, 0.5, 0, 0)
        _Blur("Blur", Float) = 0
        _BlurSize("BlurSize", Float) = 0
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
            float _BlurSize;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = _Center;
                float2 size = _Size;
                float blur = _Blur;
                float blurSize = _BlurSize;

                // https://www.shadertoy.com/view/7lBXWm
                size *= 0.5;  // half length
                float dist = length(max(abs(i.uv - center), size) - size);  // inside area is negative
                float solid = step(0.0, -dist);
                float blurred = smoothstep(-blurSize, blurSize, -dist);

                // lerp solid and blurred
                float result = lerp(solid, blurred, blur);
                return float4(result, result, result, 1.0);
            }
            ENDCG
        }
    }
}
