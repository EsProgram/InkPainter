Shader "Es/TexturePaint/Paint"{
	Properties{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Blush("Blush", 2D) = "white"
		[HideInInspector]
		_BlushScale("BlushScale", FLOAT) = 0.1
		[HideInInspector]
		_BlushColor("BlushColor", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
	}

	SubShader{

		CGINCLUDE

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
			float4 _PaintUV;
			float _BlushScale;
			float4 _BlushColor;
		ENDCG

		Pass{
			CGPROGRAM
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
				if (_PaintUV.x - h < i.uv.x && i.uv.x < _PaintUV.x + h &&
						_PaintUV.y - h < i.uv.y && i.uv.y < _PaintUV.y + h) {
					float2 uv = (_PaintUV.xy - i.uv) / h * 0.5 + 0.5;

					float dx = _ScreenParams.z * _BlushScale * 0.1;
					float dy = _ScreenParams.w * _BlushScale * 0.1;

					float4 blushCol = tex2D(_Blush, uv) +
						tex2D(_Blush, uv + float2(dx, dy)) +
						tex2D(_Blush, uv + float2(-dx, dy)) +
						tex2D(_Blush, uv + float2(dx, -dy)) +
						tex2D(_Blush, uv + float2(-dx, -dy)) * 0.2;

					if (blushCol.a - 1 >= 0)//透過部分は描画しない
						return _BlushColor;
				}

				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}