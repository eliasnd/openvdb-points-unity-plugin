Shader "Custom/Point" {
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _MainTex ("Texture", 2D) = "white" { }
        _PointSize ("Point Size", Float) = 0.05
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PointSize;

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv: TEXCOORD0;
                fixed4 col: COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half psize : PSIZE;
                fixed4 col: COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = TRANSFORM_TEX (v.uv, _MainTex);
                o.psize = _PointSize;
                o.col = v.col;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.col;
            }

            ENDCG

        }
    }
}