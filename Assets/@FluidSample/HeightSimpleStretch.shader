Shader "Unlit/HeightSimpleStretch"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ScaleFactor ("ScaleFactor", FLOAT) = 1
		_Viscosity("Viscosity", FLOAT) = 0.1
		_FlowDirection("Flow Direction", VECTOR) = (0, 0, 0, 0)
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
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _ScaleFactor;
			float _Viscosity;
			float4 _FlowDirection;

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

				//TODO:VITIATEにちっちゃいノイズ入れたい。
				const float VITIATE_X = 0.3;//どのくらい横の液体を考慮するかの係数。
				float2 shiftZ = float2(_FlowDirection.x * _MainTex_TexelSize.x, _FlowDirection.y * _MainTex_TexelSize.y) * _ScaleFactor * _Viscosity;
				float2 shiftX = float2(_MainTex_TexelSize.x * _FlowDirection.y, _MainTex_TexelSize.y * _FlowDirection.x) * _ScaleFactor * _Viscosity * VITIATE_X;
				float2 shiftz = -shiftZ;
				float2 shiftx = -shiftX;

				//TODO:直下の高さを取ってきて、その高さに応じてどの程度流れるかを決めたい
				float4 texZ = tex2Dlod(_MainTex, float4(clamp(i.uv.xy + shiftZ, 0, 1), 0, 0));
				float4 texx = tex2Dlod(_MainTex, float4(clamp(i.uv.xy + shiftx + shiftZ, 0, 1), 0, 0));
				float4 texX = tex2Dlod(_MainTex, float4(clamp(i.uv.xy + shiftX + shiftZ, 0, 1), 0, 0));

				//ピクセルの液体付着量を計算
				float amountUp = texZ.a * 0.5 + texx.a * 0.25 + texX.a * 0.25;//上にある液体の付着量(重みは直上優先)

				//上のピクセルが塗られていた場合、垂れてくると仮定して加算
				if (amountUp > (1 - _Viscosity)) {
					//TODO:色合成のアルゴリズム変更(maxだと明るくなる・・・)
					//垂れてきた液体を加算した合計の液体付着量
					float resultAmount = (col.a + amountUp) * 0.5;
					//垂れた液体の色を計算
					float3 maxRGB = max(col.rgb, max(texZ.rgb, max(texx.rgb, texX.rgb)));//全ての範囲の色の最大値(これで計算すると全体的に明るくなるのでまずい)
					float3 resultRGB = lerp(maxRGB, texZ.rgb, clamp(amountUp - _Viscosity, 0, 1));//垂れてくる液体との線形補間

					return float4(resultRGB, resultAmount);
				}

				//上のピクセルが塗られていない場合、今現在参照しているピクセルの色をそのまま帰す
				return col;
			}
			ENDCG
		}
	}
}
