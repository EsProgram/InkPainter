Shader "Es/TexturePaint/PaintHeight"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Blush("Blush", 2D) = "white"
		[HideInInspector]
		_BlushHeight("BlushHeight", 2D) = "white"
		[HideInInspector]
		_BlushScale("BlushScale", FLOAT) = 0.1
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_HeightBlend("HeightBlend", FLOAT) = 1
		[HideInInspector]
		[KeywordEnum(USE_BLUSH, ADD, SUB, MIN, MAX)]
		TEXTURE_PAINT_HEIGHT_BLEND("Height Blend Keyword", FLOAT) = 0
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
			sampler2D _BlushHeight;
			float4 _PaintUV;
			float _BlushScale;
			float _HeightBlend;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile TEXTURE_PAINT_HEIGHT_BLEND_USE_BLUSH TEXTURE_PAINT_HEIGHT_BLEND_ADD TEXTURE_PAINT_HEIGHT_BLEND_SUB TEXTURE_PAINT_HEIGHT_BLEND_MIN TEXTURE_PAINT_HEIGHT_BLEND_MAX
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
						float2 heightUV = CalcBlushUV(i.uv, _PaintUV, h);
						float4 height = tex2D(_BlushHeight, heightUV);
						return TEXTURE_PAINT_HEIGHT_BLEND(base, height, _HeightBlend, blushColor.a);
					}
				}

				return base;
			}

			ENDCG
		}
	}
}