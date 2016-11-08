using System.Collections;
using UnityEngine;

namespace Es.Effective
{
	/// <summary>
	/// テクスチャのモーフィングを実行するクラス
	/// </summary>
	public class TextureMorphing
	{
		#region PrivateField

		private const string TEXTURE_MORPHING_SHADER = "Es/Effective/TextureMorphing";
		private const string LERP_COEFFICIENT = "_LerpCoef";
		private const string SRC_TEX = "_SrcTex";
		private const string DST_TEX = "_DstTex";

		private static Material morphingMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// 2つのテクスチャを補間したテクスチャを作成する
		/// </summary>
		/// <param name="texture">RenderTexture</param>
		/// <param name="src">モーフィングに利用するテクスチャ</param>
		/// <param name="dst">モーフィング後の値を上書きするテクスチャ</param>
		/// <param name="lerpCoef">補間係数</param>
		public static void Lerp(Texture src, RenderTexture dst, float lerpCoef)
		{
			if(morphingMaterial == null)
				InitMorphingMaterial();
			SetMorphingProperty(src, dst, lerpCoef);
			var tmp = RenderTexture.GetTemporary(src.width, src.height);
			Graphics.Blit(src, tmp, morphingMaterial);
			Graphics.Blit(tmp, dst);
			RenderTexture.ReleaseTemporary(tmp);
		}

		#endregion PublicMethod

		#region PrivateMethod

		/// <summary>
		/// マテリアルの初期化をする
		/// </summary>
		private static void InitMorphingMaterial()
		{
			var shader = Shader.Find(TEXTURE_MORPHING_SHADER);
			morphingMaterial = new Material(shader);
		}

		/// <summary>
		/// マテリアルに値を設定する
		/// </summary>
		/// <param name="src">モーフィングに利用するテクスチャ</param>
		/// <param name="dst">モーフィング後の値を上書きするテクスチャ</param>
		/// <param name="lerpCoef">補間係数</param>
		private static void SetMorphingProperty(Texture src, RenderTexture dst, float lerpCoef)
		{
			morphingMaterial.SetTexture(Shader.PropertyToID(SRC_TEX), src);
			morphingMaterial.SetTexture(Shader.PropertyToID(DST_TEX), dst);
			morphingMaterial.SetFloat(Shader.PropertyToID(LERP_COEFFICIENT), lerpCoef);
		}

		#endregion PrivateMethod
	}
}