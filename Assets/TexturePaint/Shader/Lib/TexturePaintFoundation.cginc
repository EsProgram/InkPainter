#ifndef TEXTURE_PAINT_FOUNDATION
#define TEXTURE_PAINT_FOUNDATION

//ペイントブラシが描画範囲内かどうかを調べる
bool IsPaintRange(float2 mainUV, float2 paintUV, float brushScale) {
	return
		paintUV.x - brushScale < mainUV.x &&
		mainUV.x < paintUV.x + brushScale &&
		paintUV.y - brushScale < mainUV.y &&
		mainUV.y < paintUV.y + brushScale;
}

//描画範囲内で利用できるブラシ用UVを計算する
float2 CalcBrushUV(float2 mainUV, float2 paintUV, float brushScale) {
#if UNITY_UV_STARTS_AT_TOP
	return (mainUV - paintUV) / brushScale * 0.5 + 0.5;
#else
	return (paintUV - mainUV) / brushScale * 0.5 + 0.5;
#endif
}

//メインテクスチャとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_COLOR_BLENDに設定
#ifdef TEXTURE_PAINT_COLOR_BLEND_USE_CONTROL
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, brushColor, controlColor) TexturePaintColorBlendUseControl(targetColor, brushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_USE_BRUSH
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, brushColor, controlColor) TexturePaintColorBlendUseBrush(targetColor, brushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_NEUTRAL
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, brushColor, controlColor) TexturePaintColorBlendNeutral(targetColor, brushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_ALPHA_ONLY
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, brushColor, controlColor) TexturePaintColorBlendAlphaOnly(targetColor, brushColor, controlColor)
#else
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, brushColor, controlColor) TexturePaintColorBlendUseControl(targetColor, brushColor, controlColor)
#endif

float4 ColorBlend(float4 targetColor, float4 brushColor, float blend) {
	return brushColor * (1 - blend * targetColor.a) + targetColor * targetColor.a * blend;
}

#define __COLOR_BLEND(targetColor) ColorBlend((targetColor), mainColor, brushColor.a)

//ブレンド後の色を取得(指定色を使う)
float4 TexturePaintColorBlendUseControl(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND(controlColor);
}

//ブレンド後の色を取得(ブラシテクスチャ色を使う)
float4 TexturePaintColorBlendUseBrush(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND(brushColor);
}

//ブレンド後の色を取得(指定色とブラシテクスチャ色の中間色)
float4 TexturePaintColorBlendNeutral(float4 mainColor, float4 brushColor, float4 controlColor) {
	return __COLOR_BLEND((brushColor + controlColor * controlColor.a) * 0.5);
}

//ブレンド後の色を取得(アルファ値のみ書き込み)
float4 TexturePaintColorBlendAlphaOnly(float4 mainColor, float4 brushColor, float4 controlColor) {
	float4 col = mainColor;
	col.a = controlColor.a;
	return __COLOR_BLEND(col);
}

//法線マップとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_NORMAL_BLENDに設定
#ifdef TEXTURE_PAINT_NORMAL_BLEND_USE_BRUSH
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendUseBrush(mainNormal, brushNormal, blend, brushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_ADD
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendAdd(mainNormal, brushNormal, blend, brushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_SUB
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendSub(mainNormal, brushNormal, blend, brushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_MIN
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendMin(mainNormal, brushNormal, blend, brushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_MAX
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendMax(mainNormal, brushNormal, blend, brushAlpha)
#else
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, brushNormal, blend, brushAlpha) TexturePaintNormalBlendLerp(mainNormal, brushNormal, blend, brushAlpha)
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

//法線マップブレンド後の値を取得(メインテクスチャとブラシを補間)
float4 TexturePaintNormalBlendUseBrush(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(brushNormal);
}

//法線マップブレンド後の値を取得(値を加算)
float4 TexturePaintNormalBlendAdd(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND((mainNormal + brushNormal));
}

//法線マップブレンド後の値を取得(値を減算)
float4 TexturePaintNormalBlendSub(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND((mainNormal - brushNormal));
}

//法線マップブレンド後の値を取得(値の低い方に補間)
float4 TexturePaintNormalBlendMin(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(min(mainNormal, brushNormal));
}

//法線マップブレンド後の値を取得(値の高い方に補間)
float4 TexturePaintNormalBlendMax(float4 mainNormal, float4 brushNormal, float blend, float brushAlpha) {
	return __NORMAL_BLEND(max(mainNormal, brushNormal));
}

//ハイトマップとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_HEIGHT_BLENDに設定
#ifdef TEXTURE_PAINT_HEIGHT_BLEND_USE_BRUSH
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendUseBrush(mainHeight, brushHeight, blend, brushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_ADD
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendAdd(mainHeight, brushHeight, blend, brushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_SUB
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendSub(mainHeight, brushHeight, blend, brushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_MIN
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendMin(mainHeight, brushHeight, blend, brushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_MAX
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendMax(mainHeight, brushHeight, blend, brushAlpha)
#else
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, brushHeight, blend, brushAlpha) TexturePaintHeightBlendUseBrush(mainHeight, brushHeight, blend, brushAlpha)
#endif

float4 HeightBlend(float4 targetHeight,float4 mainHeight, float blend, float brushAlpha) {
	return lerp(mainHeight, targetHeight, blend * brushAlpha);
}

#define __HEIGHT_BLEND(targetHeight) HeightBlend((targetHeight), mainHeight, blend, brushAlpha)

//ハイトマップブレンド後の値を取得(メインテクスチャとブラシを補間)
float4 TexturePaintHeightBlendUseBrush(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(brushHeight);
}

//ハイトマップブレンド後の値を取得(値を加算)
float4 TexturePaintHeightBlendAdd(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(mainHeight + brushHeight);
}

//ハイトマップブレンド後の値を取得(値を加算)
float4 TexturePaintHeightBlendSub(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(mainHeight - brushHeight);
}

//ハイトマップブレンド後の値を取得(値の低い方に補間)
float4 TexturePaintHeightBlendMin(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(min(mainHeight, brushHeight));
}

//ハイトマップブレンド後の値を取得(値の高い方に補間)
float4 TexturePaintHeightBlendMax(float4 mainHeight, float4 brushHeight, float blend, float brushAlpha) {
	return __HEIGHT_BLEND(max(mainHeight, brushHeight));
}

#endif //TEXTURE_PAINT_FOUNDATION