Shader "Unlit/HeightToColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorMap("ColorMap", 2D) = "white" {}
		_Color("Color", COLOR) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
			float4 _MainTex_ST;
			float4 _Color;

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

				if (length(mainCol.xyz) > 0) {
					return _Color;
				}

				return tex2D(_ColorMap, i.uv);
			}
			ENDCG
		}
	}
}
