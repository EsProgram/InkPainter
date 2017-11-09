Shader "Es/Effective/HeightToNormal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_bumpMap("Default BumpMap", 2D) = "white" {}
		_NormalScaleFactor("NormalScale", FLOAT) = 1
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
			sampler2D _BumpMap;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _NormalScaleFactor;
			float _Border;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 PackNormal(float3 normal) {
#if defined(UNITY_NO_DXT5nm)
				return float4(normal, 0);
#else
				return float4(normal.y, normal.y, normal.y, normal.x);
#endif
			}

			float4 frag (v2f i) : SV_Target
			{
				float2 shiftX = { _MainTex_TexelSize.x, 0 };
				float2 shiftZ = { 0, _MainTex_TexelSize.y };

				float4 texX = 2 * tex2D(_MainTex, i.uv.xy + shiftX) - 1;
				float4 texx = 2 * tex2D(_MainTex, i.uv.xy - shiftX) - 1;
				float4 texZ = 2 * tex2D(_MainTex, i.uv.xy + shiftZ) - 1;
				float4 texz = 2 * tex2D(_MainTex, i.uv.xy - shiftZ) - 1;

				float3 du = { 1, 0, _NormalScaleFactor * (texX.a - texx.a) };
				float3 dv = { 0, 1, _NormalScaleFactor * (texZ.a - texz.a)};

				float3 normal = normalize(cross(du, dv));

				float4 tex = tex2D(_MainTex, i.uv.xy);
				if (tex.a <= _Border)
					return tex2D(_BumpMap, i.uv.xy);

				return PackNormal(normal * 0.5 + 0.5);
			}
			ENDCG
		}
	}
}
