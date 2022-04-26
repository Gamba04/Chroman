Shader "Gamba/Sprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags
    {
        "RenderType" = "Opaque"
        "Queue" = "Transparent"
    }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        ZTest LEqual

        //GrabPass
        //{
        //    "_UnderlyingColor"
        //}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //float4 grabPos : TEXCOORD1;

                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            //sampler2D _UnderlyingColor;

            float4 _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 coords = i.uv + _Offset.xy;

                if (coords.x > 1) coords.x -= 1;
                if (coords.y > 1) coords.x -= 1;
                if (coords.x < 0) coords.x += 1;
                if (coords.y < 0) coords.x += 1;

                fixed4 col = tex2D(_MainTex, coords);

                col *= i.color;
                col.rgb *= col.a;

                return col;
            }

            ENDCG
        }
    }
}