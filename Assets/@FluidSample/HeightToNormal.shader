Shader "Unlit/HeightToNormal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_bumpMap("Default BumpMap", 2D) = "white" {}
		_NormalScaleFactor("NormalScale", FLOAT) = 1
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
			sampler2D _BumpMap;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _NormalScaleFactor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 PackNormal(float3 normal) {
#if defined(UNITY_NO_DXT5nm)
				return float4(normal);
#else
				return float4(normal.y, normal.y, normal.y, normal.x);
#endif
			}

			float4 frag (v2f i) : SV_Target
			{
				float2 shiftX = { _MainTex_TexelSize.x, 0 };
				float2 shiftZ = { 0, _MainTex_TexelSize.y };

				float3 texX = 2 * tex2Dlod(_MainTex, float4(i.uv.xy + shiftX, 0, 0)) - 1;
				float3 texx = 2 * tex2Dlod(_MainTex, float4(i.uv.xy - shiftX, 0, 0)) - 1;
				float3 texZ = 2 * tex2Dlod(_MainTex, float4(i.uv.xy + shiftZ, 0, 0)) - 1;
				float3 texz = 2 * tex2Dlod(_MainTex, float4(i.uv.xy - shiftZ, 0, 0)) - 1;

				float3 du = { 0, _NormalScaleFactor * (texX.x - texx.x), 1 };
				float3 dv = { 1, _NormalScaleFactor * (texZ.x - texz.x), 0 };

				float3 normal = normalize(cross(du, dv));

				float3 tex = tex2Dlod(_MainTex, float4(i.uv.xy, 0, 0));
				if (length(tex.xyz) <= 0)
					return tex2Dlod(_BumpMap, float4(i.uv.xy, 0, 0));

				return PackNormal(normal * 0.5 + 0.5);
			}
			ENDCG
		}
	}
}
