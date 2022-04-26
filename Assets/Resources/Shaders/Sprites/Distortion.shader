Shader "Gamba/Distortion"
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

        GrabPass
        {
            "_UnderlyingColor"
        }

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
                float4 grabPos : TEXCOORD1;

                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _UnderlyingColor;

            float _AlphaSplitEnabled;
            float _XRatio;
            float _CameraZoom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                col.a *= i.color.a;
                col.rgb *= col.a;

                float4 grabPos = i.grabPos;

                float2 distortion = (i.uv - float2(0.5,0.5)) * pow(col.a, 1.0/5) / _CameraZoom;

                distortion = float2(distortion.x / _XRatio, distortion.y) * 10 * col.a;
                grabPos.xy += distortion * grabPos.z;

                fixed4 under = tex2Dproj(_UnderlyingColor, grabPos);

                col = col * 0.5 * col.a + (under - col) * col.a;

                return col;
            }

            ENDCG
        }
    }
}