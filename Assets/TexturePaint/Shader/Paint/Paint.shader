Shader "Es/TexturePaint/Paint"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Brush("Brush", 2D) = "white"
		[HideInInspector]
		_BrushScale("BrushScale", FLOAT) = 0.1
		[HideInInspector]
		_ControlColor("ControlColor", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		[KeywordEnum(USE_CONTROL, USE_BRUSH, NEUTRAL, ALPHA_ONLY)]
		TEXTURE_PAINT_COLOR_BLEND("Color Blend Keyword", FLOAT) = 0
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
			float4 _PaintUV;
			float _BrushScale;
			float4 _ControlColor;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile TEXTURE_PAINT_COLOR_BLEND_USE_CONTROL TEXTURE_PAINT_COLOR_BLEND_USE_BRUSH TEXTURE_PAINT_COLOR_BLEND_NEUTRAL TEXTURE_PAINT_COLOR_BLEND_ALPHA_ONLY
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
				float4 brushColor = float4(1, 1, 1, 1);

				if (IsPaintRange(i.uv, _PaintUV, h)) {
					float2 uv = CalcBrushUV(i.uv, _PaintUV, h);
					brushColor = tex2Dlod(_Brush, float4(uv.xy, 0, 0));

					return TEXTURE_PAINT_COLOR_BLEND(base, brushColor, _ControlColor);
				}
				return base;
			}

			ENDCG
		}
	}
}