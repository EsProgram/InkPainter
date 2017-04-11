Shader "Es/Sample/VTFHeightTransform" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ParallaxMap("ParallaxMap", 2D) = "white" {}
		_ParallaxScale("ParallaxScale", RANGE(0,10)) = 1
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma surface surf BlinnPhong

		sampler2D _MainTex;
		sampler2D _ParallaxMap;
		float _ParallaxScale;

	struct Input {
		float2 uv_MainTex;
	};

	void vert(inout appdata_full v) {
		float4 tex = tex2Dlod(_ParallaxMap, float4(v.texcoord.xy,0,0));
		v.vertex.y += tex.r * _ParallaxScale;
	}

	void surf(Input IN, inout SurfaceOutput o) {
		half4 tex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = tex.rgb;
		o.Alpha = tex.a;
	}

	ENDCG
	}
		FallBack "Diffuse"
}
