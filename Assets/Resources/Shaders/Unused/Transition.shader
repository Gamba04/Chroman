    Shader "Gamba/Transition"
    {
        Properties
        {
            _MainTex("Main Texture", 2D) = "defaulttexture" {}
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

            sampler2D _BlurTex;
            float _Value;
    
            // Variables

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
                fixed4 col = tex2D(_MainTex, float2(i.uv.x, i.uv.y));
                fixed4 blur = tex2D(_BlurTex, float2(i.uv.x, i.uv.y));
    
                col = col + (blur - col) * _Value;

                return col;
            }

            ENDCG
        }
    }
}
