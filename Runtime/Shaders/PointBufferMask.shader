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

            half3 _Color;
            float _PointSize;
            float4x4 _Transform;

            struct Point {
                float3 pos;
                half4 col;
            };

            // Tree offsets

            // Buffer that holds actual points to be rendered
            StructuredBuffer<Point> _Buffer;
            // StructuredBuffer<float3> _Buffer;

            /* struct Vertex {
                float3 pos;
                fixed4 col;
            };

            struct Attributes {
                float4 pos : POSITION;
                fixed4 col : COLOR;
            }; */

            struct v2f {
                float4 pos : SV_POSITION;
                half psize : PSIZE;
                half4 col : COLOR;
            };

            v2f vert(uint vid : SV_VertexID) {

                v2f o;

                // float4 pos = mul(_Transform, float4(_Buffer[vid], 1));
                // float4 pos = float4(0,0,0,1);
                // o.pos = UnityObjectToClipPos(pos);
                o.pos = UnityObjectToClipPos(mul(_Transform, float4(_Buffer[vid].pos, 1)));
                // o.col = _Color;
                o.col = _Buffer[vid].col;
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