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
	#define TEXTURE_PAINT_COLOR_BLEND(mainColor, blushColor, controlColor) TexturePaintColorBlendUseControl(mainColor, blushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_USE_BLUSH
	#define TEXTURE_PAINT_COLOR_BLEND(mainColor, blushColor, controlColor) TexturePaintColorBlendUseBlush(mainColor, blushColor, controlColor)
#elif TEXTURE_PAINT_COLOR_BLEND_NEUTRAL
	#define TEXTURE_PAINT_COLOR_BLEND(mainColor, blushColor, controlColor) TexturePaintColorBlendNeutral(mainColor, blushColor, controlColor)
#else
	#define TEXTURE_PAINT_COLOR_BLEND(mainColor, blushColor, controlColor) TexturePaintColorBlendUseControl(mainColor, blushColor, controlColor)
#endif

float4 ColorBlend(float4 targetColor, float4 mainColor, float blend) {
	return mainColor * (1 - blend) + targetColor * blend;
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
	return __COLOR_BLEND((blushColor + controlColor) * 0.5);
}

//バンプマップとブラシのブレンディングアルゴリズムをTEXTURE_PAINT_BUMP_BLENDに設定
#ifdef TEXTURE_PAINT_BUMP_BLEND_USE_BLUSH
	#define TEXTURE_PAINT_BUMP_BLEND(mainBump, blushBump, blend, blushAlpha) TexturePaintBumpBlendUseBlush(mainBump, blushBump, blend, blushAlpha)
#elif TEXTURE_PAINT_BUMP_BLEND_MIN
	#define TEXTURE_PAINT_BUMP_BLEND(mainBump, blushBump, blend, blushAlpha) TexturePaintBumpBlendMin(mainBump, blushBump, blend, blushAlpha)
#elif TEXTURE_PAINT_BUMP_BLEND_MAX
	#define TEXTURE_PAINT_BUMP_BLEND(mainBump, blushBump, blend, blushAlpha) TexturePaintBumpBlendMax(mainBump, blushBump, blend, blushAlpha)
#else
	#define TEXTURE_PAINT_BUMP_BLEND(mainBump, blushBump, blend, blushAlpha) TexturePaintBumpBlendLerp(mainBump, blushBump, blend, blushAlpha)
#endif

float4 BumpBlend(float4 targetBump,float4 mainBump, float blend, float blushAlpha) {
	return normalize(lerp(mainBump, targetBump * blushAlpha, blend));
}

#define __BUMP_BLEND(targetBump) BumpBlend((targetBump), mainBump, blend, blushAlpha)

//バンプマップブレンド後の値を取得(メインテクスチャとブラシを補間)
float4 TexturePaintBumpBlendUseBlush(float4 mainBump, float4 blushBump, float blend, float blushAlpha) {
	return __BUMP_BLEND(blushBump);
}

//バンプマップブレンド後の値を取得(値の低い方に補間)
float4 TexturePaintBumpBlendMin(float4 mainBump, float4 blushBump, float blend, float blushAlpha) {
	return __BUMP_BLEND(min(mainBump, blushBump));
}

//バンプマップブレンド後の値を取得(値の高い方に補間)
float4 TexturePaintBumpBlendMax(float4 mainBump, float4 blushBump, float blend, float blushAlpha) {
	return __BUMP_BLEND(max(mainBump, blushBump));
}

#endif //TEXTURE_PAINT_FOUNDATION