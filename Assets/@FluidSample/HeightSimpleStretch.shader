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

				float2 shiftZ = { _FlowDirection.x * _MainTex_TexelSize.x, _FlowDirection.y * _MainTex_TexelSize.y };
				float2 shiftx = { -_MainTex_TexelSize.x * _FlowDirection.y, -_MainTex_TexelSize.y * _FlowDirection.x, };
				float2 shiftX = { _MainTex_TexelSize.x * _FlowDirection.y, _MainTex_TexelSize.y * _FlowDirection.x, };

				shiftZ *= _ScaleFactor * _Viscosity;
				//shiftx *= _ScaleFactor * _Viscosity;
				//shiftX *= _ScaleFactor * _Viscosity;
				const float VITIATE = 0.5;
				shiftx *= _ScaleFactor * _Viscosity * VITIATE;
				shiftX *= _ScaleFactor * _Viscosity * VITIATE;

				float4 texZ = tex2Dlod(_MainTex, float4(i.uv.xy + shiftZ, 0, 0));
				float4 texx = tex2Dlod(_MainTex, float4(i.uv.xy + shiftx + shiftZ, 0, 0));
				float4 texX = tex2Dlod(_MainTex, float4(i.uv.xy + shiftX + shiftZ, 0, 0));

				if (abs(length(texZ.xyz)) < _Viscosity)
					return col;

				return (col + texZ + texx + texX) * 0.25;
			}
			ENDCG
		}
	}
}
