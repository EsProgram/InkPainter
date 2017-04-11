Shader "Es/InkPainter/PaintHeight"{
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
		_Color("Color", VECTOR) = (0,0,0,0)
		[HideInInspector]
		[KeywordEnum(USE_BRUSH, ADD, SUB, MIN, MAX, COLOR_RGB_HEIGHT_A)]
		INK_PAINTER_HEIGHT_BLEND("Height Blend Keyword", FLOAT) = 0
	}

	SubShader{
		CGINCLUDE

#include "Assets/InkPainter/Shader/Lib/InkPainterFoundation.cginc"

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
			float4 _Color;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile INK_PAINTER_HEIGHT_BLEND_USE_BRUSH INK_PAINTER_HEIGHT_BLEND_ADD INK_PAINTER_HEIGHT_BLEND_SUB INK_PAINTER_HEIGHT_BLEND_MIN INK_PAINTER_HEIGHT_BLEND_MAX INK_PAINTER_HEIGHT_BLEND_COLOR_RGB_HEIGHT_A
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
				float4 base = SampleTexture(_MainTex, i.uv.xy);

				if (IsPaintRange(i.uv, _PaintUV, h)) {
					float2 uv = CalcBrushUV(i.uv, _PaintUV, h);
					float4 brushColor = SampleTexture(_Brush, uv.xy);

					if (brushColor.a > 0) {
						float2 heightUV = CalcBrushUV(i.uv, _PaintUV, h);
						float4 height = SampleTexture(_BrushHeight, heightUV.xy);
#if INK_PAINTER_HEIGHT_BLEND_COLOR_RGB_HEIGHT_A
						height.a = 0.299 * height.r + 0.587 * height.g + 0.114 * height.b;
						height.rgb = _Color.rgb;
						brushColor.a = _Color.a;
#endif
						return INK_PAINTER_HEIGHT_BLEND(base, height, _HeightBlend, brushColor);
					}
				}

				return base;
			}

			ENDCG
		}
	}
}