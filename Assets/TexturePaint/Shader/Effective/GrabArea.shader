Shader "Es/GrabArea"{
	Properties{
		[HideInInspector]
		_ClipTex("Clipping Texture", 2D) = "white"
		[HideInInspector]
		_TargetTex("Target Texture", 2D) = "white"
		[HideInInspector]
		_ClipScale("Clipping Scale", FLOAT) = 0.1
		[HideInInspector]
		_ClipUV("Target UV Position", VECTOR) = (0,0,0,0)
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
#pragma vertex vert
#pragma fragment frag

			v2f vert(app_data i) {
				v2f o;
				o.screen = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float alpha = tex2Dlod(_ClipTex, float4(i.uv.xy, 0, 0)).a;
				float uv_x = (i.uv.x - 0.5) * _ClipScale * 2 + _ClipUV.x;
				float uv_y = (i.uv.y - 0.5) * _ClipScale * 2 + _ClipUV.y;

				//Repeat UV
				uv_x = fmod(uv_x, 1);
				uv_y = fmod(uv_y, 1);

				float4 base = tex2Dlod(_TargetTex, float4(uv_x, uv_y, 0, 0));
				base.a = alpha;
				return base;
			}

			ENDCG
		}
	}
}
