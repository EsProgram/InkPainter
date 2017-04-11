Shader "Es/Effective/ReverseUV"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ReverseX("Reverse X", Range(0,1)) = 1
		_ReverseY("Reverse Y", Range(0,1)) = 1
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float _ReverseX;
			float _ReverseY;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				uv.x = fmod(abs(_ReverseX - uv.x), 1);
				uv.y = fmod(abs(_ReverseY - uv.y), 1);
				fixed4 col = tex2D(_MainTex, uv);

				return col;
			}
			ENDCG
		}
	}
}
