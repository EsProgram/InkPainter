Shader "Es/TexturePaint/PaintNormal"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Blush("Blush", 2D) = "white"
		[HideInInspector]
		_BlushNormal("BlushNormal", 2D) = "white"
		[HideInInspector]
		_BlushScale("BlushScale", FLOAT) = 0.1
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_NormalBlend("NormalBlend", FLOAT) = 1
		[HideInInspector]
		[KeywordEnum(USE_BLUSH, ADD, SUB MIN, MAX)]
		TEXTURE_PAINT_NORMAL_BLEND("Normal Blend Keyword", FLOAT) = 0
	}

	SubShader{
		CGINCLUDE

#include "Assets/TexturePaint/Shader/Lib/TexturePaintFoundation.cginc"

			struct app_data {
				float4 vertex:POSITION;
				float4 uv:TEXCOORD0;
			};

			struct v2f {
				float4 screen:SV_POSITION;
				float4 uv:TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _Blush;
			sampler2D _BlushNormal;
			float4 _PaintUV;
			float _BlushScale;
			float _NormalBlend;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile TEXTURE_PAINT_NORMAL_BLEND_USE_BLUSH TEXTURE_PAINT_NORMAL_BLEND_ADD TEXTURE_PAINT_NORMAL_BLEND_SUB TEXTURE_PAINT_NORMAL_BLEND_MIN TEXTURE_PAINT_NORMAL_BLEND_MAX
#pragma vertex vert
#pragma fragment frag

			v2f vert(app_data i) {
				v2f o;
				o.screen = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float h = _BlushScale;
				float4 base = tex2D(_MainTex, i.uv);

				if (IsPaintRange(i.uv, _PaintUV, h)) {
					float2 uv = CalcBlushUV(i.uv, _PaintUV, h);
					float4 blushColor = tex2D(_Blush, uv);

					if (blushColor.a > 0) {
						float2 normalUV = CalcBlushUV(i.uv, _PaintUV, h);
						float4 normal = tex2D(_BlushNormal, normalUV);
						return TEXTURE_PAINT_NORMAL_BLEND(base, normal, _NormalBlend, blushColor.a);
					}
				}

				return base;
			}

			ENDCG
		}
	}
}