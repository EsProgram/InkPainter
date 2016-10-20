#ifndef TEXTURE_PAINT_FOUNDATION
#define TEXTURE_PAINT_FOUNDATION

//ペイントブラシが描画範囲内かどうかを調べる
bool IsPaintRange(float2 mainUV, float2 paintUV, float blushScale) {
	return
		paintUV.x - blushScale < mainUV.x &&
		mainUV.x < paintUV.x + blushScale &&
		paintUV.y - blushScale < mainUV.y &&
		mainUV.y < paintUV.y + blushScale;
}

//描画範囲内で利用できるブラシ用UVを計算する
float2 CalcBlushUV(float2 mainUV, float2 paintUV, float blushScale) {
	return (paintUV.xy - mainUV) / blushScale * 0.5 + 0.5;
}

//メインテクスチャとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_COLOR_BLENDに設定
#ifdef TEXTURE_PAINT_COLOR_BLEND_USE_CONTROL
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, blushColor, controlColor) TexturePaintColorBlendUseControl(targetColor, blushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_USE_BLUSH
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, blushColor, controlColor) TexturePaintColorBlendUseBlush(targetColor, blushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_NEUTRAL
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, blushColor, controlColor) TexturePaintColorBlendNeutral(targetColor, blushColor, controlColor)
#else
	#define TEXTURE_PAINT_COLOR_BLEND(targetColor, blushColor, controlColor) TexturePaintColorBlendUseControl(targetColor, blushColor, controlColor)
#endif

float4 ColorBlend(float4 targetColor, float4 blushColor, float blend) {
	return blushColor * (1 - blend * targetColor.a) + targetColor * targetColor.a * blend;
}

#define __COLOR_BLEND(targetColor) ColorBlend((targetColor), mainColor, blushColor.a)

//ブレンド後の色を取得(指定色を使う)
float4 TexturePaintColorBlendUseControl(float4 mainColor, float4 blushColor, float4 controlColor) {
	return __COLOR_BLEND(controlColor);
}

//ブレンド後の色を取得(ブラシテクスチャ色を使う)
float4 TexturePaintColorBlendUseBlush(float4 mainColor, float4 blushColor, float4 controlColor) {
	return __COLOR_BLEND(blushColor);
}

//ブレンド後の色を取得(指定色とブラシテクスチャ色の中間色)
float4 TexturePaintColorBlendNeutral(float4 mainColor, float4 blushColor, float4 controlColor) {
	return __COLOR_BLEND((blushColor + controlColor * controlColor.a) * 0.5);
}

//法線マップとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_NORMAL_BLENDに設定
#ifdef TEXTURE_PAINT_NORMAL_BLEND_USE_BLUSH
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendUseBlush(mainNormal, blushNormal, blend, blushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_ADD
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendAdd(mainNormal, blushNormal, blend, blushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_SUB
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendSub(mainNormal, blushNormal, blend, blushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_MIN
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendMin(mainNormal, blushNormal, blend, blushAlpha)
#elif TEXTURE_PAINT_NORMAL_BLEND_MAX
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendMax(mainNormal, blushNormal, blend, blushAlpha)
#else
	#define TEXTURE_PAINT_NORMAL_BLEND(mainNormal, blushNormal, blend, blushAlpha) TexturePaintNormalBlendLerp(mainNormal, blushNormal, blend, blushAlpha)
#endif

float4 NormalBlend(float4 targetNormal,float4 mainNormal, float blend, float blushAlpha) {
	float4 normal = lerp(mainNormal, targetNormal, blend * blushAlpha);
#if defined(UNITY_NO_DXT5nm)
	return normal;
#else
	normal.w = normal.x;
	normal.xyz = normal.y;
	return normal;
#endif
}

#define __NORMAL_BLEND(targetNormal) NormalBlend((targetNormal), mainNormal, blend, blushAlpha)

//法線マップブレンド後の値を取得(メインテクスチャとブラシを補間)
float4 TexturePaintNormalBlendUseBlush(float4 mainNormal, float4 blushNormal, float blend, float blushAlpha) {
	return __NORMAL_BLEND(blushNormal);
}

//法線マップブレンド後の値を取得(値を加算)
float4 TexturePaintNormalBlendAdd(float4 mainNormal, float4 blushNormal, float blend, float blushAlpha) {
	return __NORMAL_BLEND((mainNormal + blushNormal));
}

//法線マップブレンド後の値を取得(値を減算)
float4 TexturePaintNormalBlendSub(float4 mainNormal, float4 blushNormal, float blend, float blushAlpha) {
	return __NORMAL_BLEND((mainNormal - blushNormal));
}

//法線マップブレンド後の値を取得(値の低い方に補間)
float4 TexturePaintNormalBlendMin(float4 mainNormal, float4 blushNormal, float blend, float blushAlpha) {
	return __NORMAL_BLEND(min(mainNormal, blushNormal));
}

//法線マップブレンド後の値を取得(値の高い方に補間)
float4 TexturePaintNormalBlendMax(float4 mainNormal, float4 blushNormal, float blend, float blushAlpha) {
	return __NORMAL_BLEND(max(mainNormal, blushNormal));
}

//ハイトマップとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_HEIGHT_BLENDに設定
#ifdef TEXTURE_PAINT_HEIGHT_BLEND_USE_BLUSH
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendUseBlush(mainHeight, blushHeight, blend, blushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_ADD
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendAdd(mainHeight, blushHeight, blend, blushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_SUB
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendSub(mainHeight, blushHeight, blend, blushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_MIN
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendMin(mainHeight, blushHeight, blend, blushAlpha)
#elif TEXTURE_PAINT_HEIGHT_BLEND_MAX
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendMax(mainHeight, blushHeight, blend, blushAlpha)
#else
	#define TEXTURE_PAINT_HEIGHT_BLEND(mainHeight, blushHeight, blend, blushAlpha) TexturePaintHeightBlendUseBlush(mainHeight, blushHeight, blend, blushAlpha)
#endif

float4 HeightBlend(float4 targetHeight,float4 mainHeight, float blend, float blushAlpha) {
	return lerp(mainHeight, targetHeight, blend * blushAlpha);
}

#define __HEIGHT_BLEND(targetHeight) HeightBlend((targetHeight), mainHeight, blend, blushAlpha)

//ハイトマップブレンド後の値を取得(メインテクスチャとブラシを補間)
float4 TexturePaintHeightBlendUseBlush(float4 mainHeight, float4 blushHeight, float blend, float blushAlpha) {
	return __HEIGHT_BLEND(blushHeight);
}

//ハイトマップブレンド後の値を取得(値を加算)
float4 TexturePaintHeightBlendAdd(float4 mainHeight, float4 blushHeight, float blend, float blushAlpha) {
	return __HEIGHT_BLEND(mainHeight + blushHeight);
}

//ハイトマップブレンド後の値を取得(値を加算)
float4 TexturePaintHeightBlendSub(float4 mainHeight, float4 blushHeight, float blend, float blushAlpha) {
	return __HEIGHT_BLEND(mainHeight - blushHeight);
}

//ハイトマップブレンド後の値を取得(値の低い方に補間)
float4 TexturePaintHeightBlendMin(float4 mainHeight, float4 blushHeight, float blend, float blushAlpha) {
	return __HEIGHT_BLEND(min(mainHeight, blushHeight));
}

//ハイトマップブレンド後の値を取得(値の高い方に補間)
float4 TexturePaintHeightBlendMax(float4 mainHeight, float4 blushHeight, float blend, float blushAlpha) {
	return __HEIGHT_BLEND(max(mainHeight, blushHeight));
}

#endif //TEXTURE_PAINT_FOUNDATION