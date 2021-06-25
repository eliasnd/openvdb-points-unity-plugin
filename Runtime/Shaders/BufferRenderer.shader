Shader "Custom/Buffer" {
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _PointSize ("Point Size", Float) = 0.05
    }
    Subshader
    {
        Tags { "RenderType"="Opaque" };
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            half3 _Color;
            float _PointSize;
            float4x4 _Transform;
            StructuredBuffer<float3> _Buffer;

            /* struct Vertex {
                float3 pos;
                fixed4 col;
            };

            struct Attributes {
                float4 pos : POSITION;
                fixed4 col : COLOR;
            }; */

            struct Varyings {
                float4 pos : SV_POSITION;
                half psize : PSIZE;
                half3 col : COLOR;
            };

            Varyings vert(uint vid : SV_VertexID) {
                float3 pt = _Buffer[vid];
                float4 pos = mul(_Transform, float4(pt, 1));
                half3 col = _Color;

                Varyings o;
                o.pos = UnityObjectToClipPos(pos);
                o.col = col;
                o.psize = _PointSize;
                return o;
            }

            half4 frag(Varyings i) : SV_Target {
                return half4(i.col, 1);
            }

            ENDCG
        }
    }
}