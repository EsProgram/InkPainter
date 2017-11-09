#ifndef INK_PAINTER_FOUNDATION
#define INK_PAINTER_FOUNDATION

float4 SampleTexture(sampler2D tex, float2 uv) {
#if SHADER_TARGET < 30
	return tex2D(tex, uv);
#else
	return tex2Dlod(tex, float4(uv, 0, 0));
#endif
}

bool ExistPointInTriangle(float3 p, float3 t1, float3 t2, float3 t3)
{
	const float TOLERANCE = 1 - 0.1;

	float3 a = normalize(cross(t1 - t3, p - t1));
	float3 b = normalize(cross(t2 - t1, p - t2));
	float3 c = normalize(cross(t3 - t2, p - t3));

	float d_ab =dot(a, b);
	float d_bc =dot(b, c);

	if (TOLERANCE < d_ab && TOLERANCE < d_bc) {
		return true;
	}
	return false;
}

float2 Rotate(float2 p, float degree) {
	float rad = radians(degree);
	float x = p.x * cos(rad) - p.y * sin(rad);
	float y = p.x * sin(rad) + p.y * cos(rad);
	return float2(x, y);
}

bool IsPaintRange(float2 mainUV, float2 paintUV, float brushScale, float deg) {
	float3 p = float3(mainUV, 0);
	float3 v1 = float3(Rotate(float2(-brushScale, brushScale), deg) + paintUV, 0);
	float3 v2 = float3(Rotate(float2(-brushScale, -brushScale), deg) + paintUV, 0);
	float3 v3 = float3(Rotate(float2(brushScale, -brushScale), deg) + paintUV, 0);
	float3 v4 = float3(Rotate(float2(brushScale, brushScale), deg) + paintUV, 0);
	return ExistPointInTriangle(p, v1, v2, v3) || ExistPointInTriangle(p, v1, v3, v4);
}

float2 CalcBrushUV(float2 mainUV, float2 paintUV, float brushScale, float deg) {
#if UNITY_UV_STARTS_AT_TOP
	return Rotate((mainUV - paintUV) / brushScale, -deg) * 0.5 + 0.5;
#else
	return Rotate((paintUV - mainUV) / brushScale, deg) * 0.5 + 0.5;
#endif
}

#ifdef INK_PAINTER_COLOR_BLEND_USE_CONTROL
	#define INK_PAINTER_COLOR_BLEND(targetColor, brushColor, controlColor) InkPainterColorBlendUseControl(targetColor, brushColor, controlColor)
#elif INK_PAINTER_COLOR_BLEND_USE_BRUSH
	#define INK_PAINTER_COLOR_BLEND(targetColor, brushColor, controlColor) InkPainterColorBlendUseBrush(targetColor, brushColor, controlColor)
#elif INK_PAINTER_COLOR_BLEND_NEUTRAL
	#define INK_PAINTER_COLOR_BLEND(targetColor, brushColor, controlColor) InkPainterColorBlendNeutral(targetColor, brushColor, controlColor)
#elif INK_PAINTER_COLOR_BLEND_ALPHA_ONLY
	#define INK_PAINTER_COLOR_BLEND(targetColor, brushColor, controlColor) InkPainterColorBlendAlphaOnly(targetColor, brushColor, controlColor)
#else
	#define INK_PAINTER_COLOR_BLEND(targetColor, brushColor, controlColor) InkPainterColorBlendUseControl(targetColor, brushColor, controlColor)
#endif

float4 ColorBlend(float4 targetColor, float4 brushColor, float blend) {
	return brushColor * (1 - blend * targetColor.a) + targetColor * targetColor.a * blend;
}

#define __COLOR_BLEND(targetColor) ColorBlend((targetColor), mainColor, brushColor.a)

float4 InkPainterColorBlendUseControl(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND(controlColor);
}

float4 InkPainterColorBlendUseBrush(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND(brushColor);
}

float4 InkPainterColorBlendNeutral(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND((brushColor + controlColor * controlColor.a) * 0.5);
}

float4 InkPainterColorBlendAlphaOnly(float4 mainColor, float4 brushColor, float4 controlColor) {
	float4 col = mainColor;
	col.a = controlColor.a;
	return __COLOR_BLEND(col);
}

#ifdef INK_PAINTER_NORMAL_BLEND_USE_BRUSH
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendUseBrush(mainNormal, brushNormal, blend, brushAlpha)
#elif INK_PAINTER_NORMAL_BLEND_ADD
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendAdd(mainNormal, brushNormal, blend, brushAlpha)
#elif INK_PAINTER_NORMAL_BLEND_SUB
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendSub(mainNormal, brushNormal, blend, brushAlpha)
#elif INK_PAINTER_NORMAL_BLEND_MIN
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendMin(mainNormal, brushNormal, blend, brushAlpha)
#elif INK_PAINTER_NORMAL_BLEND_MAX
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendMax(mainNormal, brushNormal, blend, brushAlpha)
#else
	#define INK_PAINTER_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) InkPainterNormalBlendLerp(mainNormal, brushNormal, blend, brushAlpha)
#endif

float4 NormalBlend(float4 targetNormal,float4 mainNormal, float blend, float brushAlpha) {
	float4 normal = lerp(mainNormal, targetNormal, blend * brushAlpha);
#if defined(UNITY_NO_DXT5nm) || defined(DXT5NM_COMPRESS_UNUSE)
	return normal;
#else
	normal.w = normal.x;
	normal.xyz = normal.y;
	return normal;
#endif
}

#define __NORMAL_BLEND(targetNormal) NormalBlend((targetNormal), mainNormal, blend, brushAlpha)

float4 InkPainterNormalBlendUseBrush(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(brushNormal);
}

float4 InkPainterNormalBlendAdd(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND((mainNormal + brushNormal));
}

float4 InkPainterNormalBlendSub(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND((mainNormal - brushNormal));
}

float4 InkPainterNormalBlendMin(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(min(mainNormal, brushNormal));
}

float4 InkPainterNormalBlendMax(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(max(mainNormal, brushNormal));
}

#ifdef INK_PAINTER_HEIGHT_BLEND_USE_BRUSH
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendUseBrush(mainHeight, brushHeight, blend, brushAlpha)
#elif INK_PAINTER_HEIGHT_BLEND_ADD
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendAdd(mainHeight, brushHeight, blend, brushAlpha)
#elif INK_PAINTER_HEIGHT_BLEND_SUB
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendSub(mainHeight, brushHeight, blend, brushAlpha)
#elif INK_PAINTER_HEIGHT_BLEND_MIN
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendMin(mainHeight, brushHeight, blend, brushAlpha)
#elif INK_PAINTER_HEIGHT_BLEND_MAX
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendMax(mainHeight, brushHeight, blend, brushAlpha)
#elif INK_PAINTER_HEIGHT_BLEND_COLOR_RGB_HEIGHT_A
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushColor) InkPainterHeightBlendColorRGBHeightA(mainHeight, brushHeight, blend, brushColor)
#else
	#define INK_PAINTER_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) InkPainterHeightBlendUseBrush(mainHeight, brushHeight, blend, brushAlpha)
#endif

float4 HeightBlend(float4 targetHeight, float4 mainHeight, float blend, float4 brushColor) {
	return lerp(mainHeight, targetHeight, blend * brushColor.a);
}

#define __HEIGHT_BLEND(targetHeight) HeightBlend((targetHeight), mainHeight, blend, brushAlpha)

float4 InkPainterHeightBlendUseBrush(float4 mainHeight, float4 brushHeight, float blend, float4 brushAlpha) {
	return __HEIGHT_BLEND(brushHeight);
}

float4 InkPainterHeightBlendAdd(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(mainHeight + brushHeight);
}

float4 InkPainterHeightBlendSub(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(mainHeight - brushHeight);
}

float4 InkPainterHeightBlendMin(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(min(mainHeight, brushHeight));
}

float4 InkPainterHeightBlendMax(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(max(mainHeight, brushHeight));
}

float4 InkPainterHeightBlendColorRGBHeightA(float4 mainHeight, float4 brushHeight, float blend, float4 brushColor) {
	return float4(lerp(brushColor.rgb, brushHeight.rgb, brushColor.a), lerp(mainHeight.a, brushHeight.a, blend));
}

#endif //INK_PAINTER_FOUNDATION