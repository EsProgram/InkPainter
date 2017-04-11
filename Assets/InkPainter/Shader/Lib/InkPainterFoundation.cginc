#ifndef INK_PAINTER_FOUNDATION
#define INK_PAINTER_FOUNDATION

float4 SampleTexture(sampler2D tex, float2 uv) {
#if SHADER_TARGET < 30
	return tex2D(tex, uv);
#else
	return tex2Dlod(tex, float4(uv, 0, 0));
#endif
}

bool IsPaintRange(float2 mainUV, float2 paintUV, float brushScale) {
	return
		paintUV.x - brushScale < mainUV.x &&
		mainUV.x < paintUV.x + brushScale &&
		paintUV.y - brushScale < mainUV.y &&
		mainUV.y < paintUV.y + brushScale;
}

float2 CalcBrushUV(float2 mainUV, float2 paintUV, float brushScale) {
#if UNITY_UV_STARTS_AT_TOP
	return (mainUV - paintUV) / brushScale * 0.5 + 0.5;
#else
	return (paintUV - mainUV) / brushScale * 0.5 + 0.5;
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
#if defined(UNITY_NO_DXT5nm)
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