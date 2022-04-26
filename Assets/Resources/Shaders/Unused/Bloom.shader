Shader "Gamba/Bloom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    
    struct data
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct vertInfo
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    // -------------------------------------------------------------------------------------------------------------------------------------------------
    
    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    vertInfo Vertex (data d)
    {
        vertInfo _out;
        
        _out.vertex = UnityObjectToClipPos(d.vertex);
        _out.uv = TRANSFORM_TEX(d.uv, _MainTex);
        
        return _out;
    }
    
    // Variables
    
    
    
    // Functions
    
    
    
    // -------------------------------------------------------------------------------------------------------------------------------------------------

    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            fixed4 Fragment (vertInfo _in) : SV_Target
            {
                float2 uv = _in.uv;
                fixed4 col = tex2D(_MainTex, uv);

                return col;
            }
            ENDCG
        }
    }
}