Shader "Gamba/LightsPostProcess"
{
    Properties
    {
        _MainTex("Lights texture", 2D) = "defaulttexture" {}
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
        sampler2D _CameraDepthNormalsTexture;
        float4 _MainTex_ST;

        // Variables

        sampler2D _OcclusionTex;
        float _LightIntensity;

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
            fixed4 col = tex2D(_MainTex, i.uv);

            float depth = 0;
            float3 normal = 0;

            DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depth, normal);

            fixed4 occlusion = tex2D(_OcclusionTex, i.uv);

            // Compensate shadow alpha
            float min = 0.6;
            if (occlusion.a < min) 
            {
                occlusion.a += (min - occlusion.a) * (1 - min);
            }
            
            // Add shadow
            col += (occlusion - col) * occlusion.a;

            // Add Exposure
            //col *= 1 + (1 - occlusion.a) * _LightIntensity;

            col.rgb += occlusion.rgb * (1 - occlusion.a) * _LightIntensity;

            //if (i.uv.x < 0.3) 
            {
                //col.xyz = depth;
            }

            return col;
        }

        ENDCG
    }
    }
}
