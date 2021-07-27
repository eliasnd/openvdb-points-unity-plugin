Shader "Custom/PointBuffer" {
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _PointSize ("Point Size", Float) = 0.05
    }
    Subshader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Point {
                float3 pos;
                half4 col;
            };

            half3 _Color;
            float _PointSize;
            float4x4 _Transform;

            StructuredBuffer<Point> _PointBuffer;

            StructuredBuffer<int> _IndexBuffer;
            int _UseIndexBuffer;

            struct v2f {
                float4 pos : SV_POSITION;
                half psize : PSIZE;
                half4 col : COLOR;
            };

            v2f vert(uint vid : SV_VertexID) {

                v2f o;

                Point p;

                if (_UseIndexBuffer == 1) {
                    p = _PointBuffer[_IndexBuffer[vid]];
                } else {
                    p = _PointBuffer[vid];
                }

                p = _PointBuffer[vid];

                o.pos = UnityObjectToClipPos(mul(_Transform, float4(p.pos, 1)));
                o.col = p.col;
                o.psize = _PointSize;
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                return i.col;
            }

            ENDCG
        }
    }
}