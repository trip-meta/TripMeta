Shader "Hidden/FoveatedRendering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GazePointUV ("Gaze Point UV", Vector) = (0.5, 0.5, 0, 0)
        _InnerRadius ("Inner Radius", Float) = 0.15
        _MiddleRadius ("Middle Radius", Float) = 0.3
        _OuterRadius ("Outer Radius", Float) = 0.5
        _InnerScale ("Inner Scale", Float) = 1.0
        _MiddleScale ("Middle Scale", Float) = 0.75
        _OuterScale ("Outer Scale", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _GazePointUV;
            float _InnerRadius;
            float _MiddleRadius;
            float _OuterRadius;
            float _InnerScale;
            float _MiddleScale;
            float _OuterScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float2 GetSampleUV(float2 uv, float2 gazePoint, float scale)
            {
                // 计算相对于注视点的UV偏移
                float2 offset = uv - gazePoint;
                // 应用缩放
                offset /= scale;
                // 转换回UV坐标
                return gazePoint + offset;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算当前像素到注视点的距离
                float2 delta = i.uv - _GazePointUV;
                float dist = length(delta);

                float2 sampleUV = i.uv;
                float scale = 1.0;

                // 根据距离选择不同的采样比例
                if (dist < _InnerRadius)
                {
                    // 内圈 - 全分辨率
                    scale = _InnerScale;
                }
                else if (dist < _MiddleRadius)
                {
                    // 中圈 - 中等分辨率
                    scale = lerp(_InnerScale, _MiddleScale,
                        (dist - _InnerRadius) / (_MiddleRadius - _InnerRadius));
                }
                else if (dist < _OuterRadius)
                {
                    // 外圈 - 低分辨率
                    scale = lerp(_MiddleScale, _OuterScale,
                        (dist - _MiddleRadius) / (_OuterRadius - _MiddleRadius));
                }
                else
                {
                    // 最外围 - 最低分辨率
                    scale = _OuterScale;
                }

                // 计算采样UV
                sampleUV = GetSampleUV(i.uv, _GazePointUV, scale);

                // 边界处理
                sampleUV = saturate(sampleUV);

                // 采样主纹理
                fixed4 col = tex2D(_MainTex, sampleUV);

                // 应用雾效
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
