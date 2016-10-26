Shader "Es/TexturePaint/PaintHeight"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Brush("Brush", 2D) = "white"
		[HideInInspector]
		_BrushHeight("BrushHeight", 2D) = "white"
		[HideInInspector]
		_BrushScale("BrushScale", FLOAT) = 0.1
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_HeightBlend("HeightBlend", FLOAT) = 1
		[HideInInspector]
		[KeywordEnum(USE_BRUSH, ADD, SUB, MIN, MAX)]
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
			sampler2D _Brush;
			sampler2D _BrushHeight;
			float4 _PaintUV;
			float _BrushScale;
			float _HeightBlend;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile TEXTURE_PAINT_HEIGHT_BLEND_USE_BRUSH TEXTURE_PAINT_HEIGHT_BLEND_ADD TEXTURE_PAINT_HEIGHT_BLEND_SUB TEXTURE_PAINT_HEIGHT_BLEND_MIN TEXTURE_PAINT_HEIGHT_BLEND_MAX
#pragma vertex vert
#pragma fragment frag

			v2f vert(app_data i) {
				v2f o;
				o.screen = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float h = _BrushScale;
				float4 base = tex2Dlod(_MainTex, float4(i.uv.xy, 0, 0));

				if (IsPaintRange(i.uv, _PaintUV, h)) {
					float2 uv = CalcBrushUV(i.uv, _PaintUV, h);
					float4 brushColor = tex2Dlod(_Brush, float4(uv.xy, 0, 0));

					if (brushColor.a > 0) {
						float2 heightUV = CalcBrushUV(i.uv, _PaintUV, h);
						float4 height = tex2Dlod(_BrushHeight, float4(heightUV.xy, 0, 0));
						return TEXTURE_PAINT_HEIGHT_BLEND(base, height, _HeightBlend, brushColor.a);
					}
				}

				return base;
			}

			ENDCG
		}
	}
}