    Shader "Gamba/Blur"
    {
        Properties
        {
            _Amount ("Amount", Range (0, 50)) = 0
            [Space(25)]
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
    
            // Variables
    
            int _KernelSize;
    
            float _ScrWidth;
            float _ScrHeight;
    
            bool _Pause;
            sampler2D _CopyTex;
    
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
    
            float magnitude(float2 position) 
            {
                return sqrt(pow(position.x, 2) + pow(position.y, 2));
            }

            float sqrMagnitude(float2 position) 
            {
                return pow(position.x, 2) + pow(position.y, 2);
            }

            // -------------------------------------------------------------------------------------------------------------------------------------------------
    
            fixed4 frag(v2f i) : SV_Target
            {
                float euler = 2.71828;
                float pi = 3.141592653589;

                fixed4 col = tex2D(_MainTex, i.uv);
    
                if (!_Pause)
                {
                    float kernelSum = 1;

                    fixed4 valuesSum = col;

                    //for (float x = 1; x < _KernelSize * 2 + 1; x++)
                    //{
                    //    for (float y = 1; y < _KernelSize * 2 + 1; y++)
                    //    {
                    //        float2 samplePos = float2((x - _KernelSize), (y - _KernelSize)); // local pixel coords (-_KernelSize, KernelSize)
                    //        float localDistance = sqrMagnitude(samplePos / _KernelSize);

                    //        if (localDistance > 1) 
                    //        {
                    //            float2 sampleCoords = i.uv + float2(samplePos.x / _ScrWidth, samplePos.y / _ScrHeight); // world float coords
                    //            fixed4 sampleValue = tex2D(_MainTex, sampleCoords);

                    //            float standardDeviation = 2;

                    //            float kernelValue = pow((1.0 / standardDeviation * sqrt(2 * pi)) * euler, -localDistance / (2 * pow(standardDeviation, 2)));

                    //            kernelSum += kernelValue;
                    //            valuesSum += sampleValue * kernelValue;
                    //        }
                    //    }
                    //}

                    for (float x = 1; x <= _KernelSize; x+= 1.5)
                    {
                        for (float y = 1; y <= _KernelSize; y+= 1.5)
                        {
                            float localDistance = sqrMagnitude(float2(x, y) / _KernelSize);

                            //float standardDeviation = 2;
                            //float kernelValue = pow((1.0 / standardDeviation * sqrt(2 * pi)) * euler, -localDistance / (2 * pow(standardDeviation, 2))); // Gaussian Crap

                            float xCoord = x / _ScrWidth;
                            float yCoord = y / _ScrHeight;

                            if (localDistance < 1)
                            {
                                valuesSum += tex2D(_MainTex, i.uv + float2(xCoord, yCoord));
                                valuesSum += tex2D(_MainTex, i.uv + float2(-xCoord, -yCoord));
                                valuesSum += tex2D(_MainTex, i.uv + float2(-xCoord, yCoord));
                                valuesSum += tex2D(_MainTex, i.uv + float2(xCoord, -yCoord));

                                kernelSum += 4;
                            }
                        }
                    }

                    col = valuesSum / kernelSum;
                }
                else 
                {
                    fixed4 col = tex2D(_CopyTex, i.uv);
                }

                return col;
            }

            ENDCG
        }
    }
}
