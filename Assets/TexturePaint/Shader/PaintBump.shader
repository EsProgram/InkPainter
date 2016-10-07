Shader "Es/TexturePaint/PaintBump"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Blush("Blush", 2D) = "white"
		[HideInInspector]
		_BlushBump("BlushBump", 2D) = "white"
		[HideInInspector]
		_BlushScale("BlushScale", FLOAT) = 0.1
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_BumpBlend("BumpBlend", FLOAT) = 1
		[HideInInspector]
		[KeywordEnum(USE_BLUSH, MIN, MAX)]
		TEXTURE_PAINT_BUMP_BLEND("Bump Blend Keyword", FLOAT) = 0
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
			sampler2D _BlushBump;
			float4 _PaintUV;
			float _BlushScale;
			float _BumpBlend;
		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile TEXTURE_PAINT_BUMP_BLEND_USE_BLUSH TEXTURE_PAINT_BUMP_BLEND_MIN TEXTURE_PAINT_BUMP_BLEND_MAX
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

					if (blushColor.a > 0) {//透過部分は描画しない
						float2 bumpUV = CalcBlushUV(i.uv, _PaintUV, h);
						float4 bump = tex2D(_BlushBump, bumpUV);
						return TEXTURE_PAINT_BUMP_BLEND(base, bump, _BumpBlend, blushColor.a);
					}
				}


				return base;
			}

			ENDCG
		}
	}
}