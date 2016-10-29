using UnityEngine;

namespace Es.Effective
{
	/// <summary>
	/// テクスチャの指定範囲を取得するクラス
	/// </summary>
	public class GrabArea
	{
		#region PrivateField

		private const string GRAB_AREA_SHADER = "Es/GrabArea";
		private const string CLIP = "_ClipTex";
		private const string TARGET = "_TargetTex";
		private const string CLIP_SCALE = "_ClipScale";
		private const string CLIP_UV = "_ClipUV";

		private static Material grabAreaMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// クリップテクスチャによって指定される範囲のターゲットテクスチャを切り抜く
		/// </summary>
		/// <param name="clipTexture">クリッピングに用いるテクスチャ</param>
		/// <param name="clipScale">クリップテクスチャのターゲットテクスチャに対する大きさの割合</param>
		/// <param name="grabTargetTexture">切り抜く対象のテクスチャ</param>
		/// <param name="targetUV">ターゲットテクスチャ上のUV座標の指定</param>
		/// <param name="dst">切り抜いたテクスチャの保存先テクスチャ</param>
		public static void Clip(Texture clipTexture, float clipScale, Texture grabTargetTexture, Vector2 targetUV, RenderTexture dst)
		{
			if(grabAreaMaterial == null)
				InitGrabAreaMaterial();
			SetGrabAreaProperty(clipTexture, clipScale, grabTargetTexture, targetUV);
			var tmp = RenderTexture.GetTemporary(clipTexture.width, clipTexture.height, 0);
			Graphics.Blit(clipTexture, tmp, grabAreaMaterial);
			Graphics.Blit(tmp, dst);
			RenderTexture.ReleaseTemporary(tmp);
		}

		#endregion PublicMethod

		#region PrivateMethod

		/// <summary>
		/// マテリアルを初期化する
		/// </summary>
		private static void InitGrabAreaMaterial()
		{
			var shader = Shader.Find(GRAB_AREA_SHADER);
			grabAreaMaterial = new Material(shader);
		}

		/// <summary>
		/// マテリアルに値を設定する
		/// </summary>
		/// <param name="clip">クリッピングに用いる</param>
		/// <param name="clipScale">クリッピングスケール</param>
		/// <param name="grabTarget">クリッピング対象テクスチャ</param>
		/// <param name="targetUV">ターゲットテクスチャ上のUV座標の指定</param>
		private static void SetGrabAreaProperty(Texture clip, float clipScale, Texture grabTarget, Vector2 targetUV)
		{
			grabAreaMaterial.SetTexture(CLIP, clip);
			grabAreaMaterial.SetTexture(TARGET, grabTarget);
			grabAreaMaterial.SetFloat(CLIP_SCALE, clipScale);
			grabAreaMaterial.SetVector(CLIP_UV, targetUV);
		}

		#endregion PrivateMethod
	}
}