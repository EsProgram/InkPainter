Shader "Es/Effective/GrabArea"{
	Properties{
		[HideInInspector]
		_ClipTex("Clipping Texture", 2D) = "white"
		[HideInInspector]
		_TargetTex("Target Texture", 2D) = "white"
		[HideInInspector]
		_ClipScale("Clipping Scale", FLOAT) = 0.1
		[HideInInspector]
		_ClipUV("Target UV Position", VECTOR) = (0,0,0,0)
		[KeywordEnum(CLAMP, REPEAT, CLIP)]
		WRAP_MODE("Color Blend Keyword", FLOAT) = 0
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

			sampler2D _TargetTex;
			sampler2D _ClipTex;
			float4 _ClipUV;
			float _ClipScale;

		ENDCG

		Pass{
			CGPROGRAM
#pragma multi_compile WRAP_MODE_CLAMP WRAP_MODE_REPEAT WRAP_MODE_CLIP
#pragma vertex vert
#pragma fragment frag

			v2f vert(app_data i) {
				v2f o;
				o.screen = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float alpha = tex2D(_ClipTex, i.uv.xy).a;
				float uv_x = (i.uv.x - 0.5) * _ClipScale * 2 + _ClipUV.x;
				float uv_y = (i.uv.y - 0.5) * _ClipScale * 2 + _ClipUV.y;

#if WRAP_MODE_CLAMP
				//Clamp UV
				uv_x = clamp(uv_x, 0, 1);
				uv_y = clamp(uv_y, 0, 1);
#elif WRAP_MODE_REPEAT
				//Repeat UV
				uv_x = fmod(abs(uv_x), 1);
				uv_y = fmod(abs(uv_y), 1);
#elif WRAP_MODE_CLIP
				//Clip UV
				clip(uv_x);
				clip(uv_y);
				clip(trunc(uv_x) * -1);
				clip(trunc(uv_y) * -1);
#endif

				float4 base = tex2D(_TargetTex, float2(uv_x, uv_y));
				base.a = alpha;
				return base;
			}

			ENDCG
		}
	}
}
