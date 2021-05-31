Shader "Custom/Point" {
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _MainTex ("Texture", 2D) = "white" { }
        _PointSize ("Point Size", Float) = 0.05
    }
    SubShader
    {
        Tags{"RenderType"="Opaque"}
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            half3 _Color;
            float _PointSize;
            float4x4 _Transform;

            struct vertex {
                float3 pos;
                fixed4 col;
            };

        #if _COMPUTE_BUFFER
            StructuredBuffer<float3> _PointBuffer;
        #endif

            struct attributes
            {
                float4 pos : POSITION;
                fixed4 col: COLOR;
            };

            struct varyings {
                float4 pos : SV_POSITION;
                half psize : PSIZE;
                fixed4 col: COLOR;
            };


        #if _COMPUTE_BUFFER   
            varyings vert(uint vid : SV_VertexID)
        #else
            varyings vert(attributes input)
        #endif
            {
            #if _COMPUTE_BUFFER
                float3 pt = _PointBuffer[vid];
                float4 pos = mul(_Transform, float4(pt, 1));
                // float4 pos = float4(pt, 1);
            #else
                float4 pos = input.pos;
            #endif
                fixed4 col = input.col;

                varyings o;
                o.pos = UnityObjectToClipPos(pos);
                o.col = col;
                o.psize = _PointSize;
                return o;
            }
                
            fixed4 frag (varyings i) : SV_Target
            {
                return i.col;
            }

            ENDCG

        }
    }
}