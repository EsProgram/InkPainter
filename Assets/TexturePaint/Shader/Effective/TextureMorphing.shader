Shader "Es/Effective/TextureMorphing"
{
	Properties
	{
		_SrcTex("Source Texture", 2D) = "white" {}
		_DstTex("Destination Texture", 2D) = "white" {}
		_LerpCoef("Lerp Coefficient", RANGE(0,1)) = 0.5
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

			sampler2D _SrcTex;
			sampler2D _DstTex;
			float _LerpCoef;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 src = tex2D(_SrcTex, i.uv);
				float4 dst = tex2D(_DstTex, i.uv);
				float4 col = src * _LerpCoef + dst * (1 - _LerpCoef);
				return col;
			}
			ENDCG
		}
	}
}
