Shader "Gamba/KineticCloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags
    {
        "RenderType" = "Transparent"
        "Queue" = "Transparent+1"
    }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        ZTest LEqual

        GrabPass
        {
            
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
            sampler2D _GrabTexture;
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

                col *= i.color;
                col.rgb *= col.a;

                float4 grabPos = i.grabPos;

                float2 distortion = (i.uv - float2(0.5,0.5)) * -(0.5 - col.a * col.a) * 0.4 /_CameraZoom;

                distortion = float2(distortion.x/ _XRatio, distortion.y);
                grabPos.xy += distortion * grabPos.z;

                fixed4 under = tex2Dproj(_GrabTexture, grabPos);

                col += (under - col) * col.a;

                fixed3 tone = col.rgb * i.color.rgb;

                col.rgb += (tone - col.rgb) * 0.3;

                return col;
            }

            ENDCG
        }
    }
}