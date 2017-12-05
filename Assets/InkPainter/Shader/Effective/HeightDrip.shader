Shader "Es/Effective/HeightDrip"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ScaleFactor ("ScaleFactor", FLOAT) = 1
		_Viscosity("Viscosity", FLOAT) = 0.1
		_FlowDirection("Flow Direction", VECTOR) = (0, 0, 0, 0)
		_NormalMap("Normal Map", 2D) = "white" {}
		_HorizontalSpread("HorizontalSpread", Float) = 0.1
		_FixedColor("InkColor", COLOR) = (0, 0, 0, 0)
		[KeywordEnum(ADD, OVERWRITE)]
		COLOR_SYNTHESIS("Color synthesis algorithm", FLOAT) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile COLOR_SYNTHESIS_ADD COLOR_SYNTHESIS_OVERWRITE

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
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _ScaleFactor;
			float _Viscosity;
			float4 _FlowDirection;
			sampler2D _NormalMap;
			float _HorizontalSpread;
			float4 _FixedColor;

			float rand(float3 seed)
			{
				return frac(sin(dot(seed.xyz, float3(12.9898, 78.233, 56.787))) * 43758.5453);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				float3 normal = normalize(UnpackNormal(tex2D(_NormalMap, i.uv)).xyz);
				float VITIATE_Z = pow(normal.b, 2) - normal.y * 0.2;
				float VITIATE_X = _HorizontalSpread * rand(float3(i.uv.xy, i.uv.x + i.uv.y)) * (1 + normal.b * 30);

				float2 shiftZ = float2(_FlowDirection.x * _MainTex_TexelSize.x, _FlowDirection.y * _MainTex_TexelSize.y) * _ScaleFactor * _Viscosity * VITIATE_Z;
				float2 shiftX = float2(_MainTex_TexelSize.x * _FlowDirection.y, _MainTex_TexelSize.y * _FlowDirection.x) * _ScaleFactor * _Viscosity * VITIATE_X;
				float2 shiftx = -shiftX;

				float4 texZ = tex2D(_MainTex, clamp(i.uv.xy + shiftZ, 0, 1));
				float4 texx = tex2D(_MainTex, clamp(i.uv.xy + shiftx + shiftZ, 0, 1));
				float4 texX = tex2D(_MainTex, clamp(i.uv.xy + shiftX + shiftZ, 0, 1));

				float amountUp = (texZ.a + texx.a + texX.a) * 0.3333;

				if (amountUp > (1 - _Viscosity)) {
					float resultAmount = (col.a + amountUp) * 0.5;
#ifdef COLOR_SYNTHESIS_ADD
					float3 maxRGB = max(col.rgb, max(texZ.rgb, max(texx.rgb, texX.rgb)));
					float3 resultRGB = lerp(maxRGB, texZ.rgb, clamp(amountUp - _Viscosity, 0, 1));
					return float4(resultRGB, resultAmount);
#else
					return float4(_FixedColor.rgb, resultAmount);
#endif

				}

				return col;
			}
			ENDCG
		}
	}
}
