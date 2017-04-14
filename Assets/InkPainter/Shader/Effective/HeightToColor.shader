Shader "Es/Effective/HeightToColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorMap("ColorMap", 2D) = "white" {}
		_BaseColor("BaseColor", 2D) = "white" {}
		_Alpha("Alpha", Float) = 1
		_Border("Border", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

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

			sampler2D _MainTex;
			sampler2D _ColorMap;
			sampler2D _BaseColor;
			float4 _MainTex_ST;
			float _Alpha;
			float _Border;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 mainCol = tex2D(_MainTex, i.uv);

				if (mainCol.a > _Border) {
					return lerp(tex2D(_BaseColor, i.uv), float4(mainCol.rgb, 1), _Alpha);
				}

				return tex2D(_ColorMap, i.uv);
			}
			ENDCG
		}
	}
}
