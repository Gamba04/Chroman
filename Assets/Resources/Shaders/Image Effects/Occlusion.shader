Shader "Gamba/Occlusion"
{
    Properties
    {
        _OcclusionColor ("Color", Color) = (0,0,0,0)

        [Space(25)]

        _MainTex ("Lights texture", 2D) = "defaulttexture" {}
    }

        SubShader
    {
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // -------------------------------------------------------------------------------------------------------------------------------------------------

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables

            fixed4 _OcclusionColor;
            sampler2D _CamTex;

            // -------------------------------------------------------------------------------------------------------------------------------------------------

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // -------------------------------------------------------------------------------------------------------------------------------------------------

            // Functions

            // -------------------------------------------------------------------------------------------------------------------------------------------------

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _OcclusionColor;

                fixed4 lightCol = tex2D(_CamTex, i.uv);

                // Add color
                col = fixed4((col + (lightCol - col) * lightCol.a).rgb, col.a);

                // Limit max alpha reduction?
                float lightLimit = 1;
                if (lightCol.a > lightLimit)
                {
                    lightCol.a = lightLimit;
                }

                // Open alpha holes
                col.a -= lightCol.a;

                return col;
            }

            ENDCG
        }
    }
}
