Shader "Custom/Point" {
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _PointSize ("Point Size", Float) = 0.05
        _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 position : SV_Position;
                half3 color : COLOR;
                half psize : PSIZE;
            };

            float4x4 _Transform;
            half _PointSize;

            Varyings Vertex(Attributes input)
            {
                float4 pos = input.position;
                float4 col = input.color;

                Varyings o;
                o.position = UnityObjectToClipPos(pos);
                o.color = col;
                o.psize = _PointSize;
                return o;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                return input.color;
            }

            ENDCG
        }
    }
}