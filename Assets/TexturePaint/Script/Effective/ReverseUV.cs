using UnityEngine;

namespace Es.Effective
{
	/// <summary>
	/// UVの反転を行うクラス
	/// </summary>
	public class ReverseUV
	{
		#region PrivateField

		private const string REVERSE_UV_SHADER = "Es/Effective/ReverseUV";
		private const string REVERSE_X = "_ReverseX";
		private const string REVERSE_Y = "_ReverseY";

		private const float DEFAULT = 1f;
		private const float REVERSE = 0f;

		private static Material reverseUVMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// 横方向の反転を行う
		/// </summary>
		/// <param name="src">反転対象のテクスチャ</param>
		/// <param name="dst">反転後のテクスチャ格納先</param>
		public static void Horizontal(Texture src, RenderTexture dst)
		{
			if(reverseUVMaterial == null)
				InitReverseUVMaterial();
			SetReverseUVProperty(REVERSE, DEFAULT);
			Blit(src, dst);
		}

		/// <summary>
		/// 縦方向の反転を行う
		/// </summary>
		/// <param name="src">反転対象のテクスチャ</param>
		/// <param name="dst">反転後のテクスチャ格納先</param>
		public static void Vertical(Texture src, RenderTexture dst)
		{
			if(reverseUVMaterial == null)
				InitReverseUVMaterial();
			SetReverseUVProperty(DEFAULT, REVERSE);
			Blit(src, dst);
		}

		/// <summary>
		/// 縦横方向の反転を行う
		/// </summary>
		/// <param name="src">反転対象のテクスチャ</param>
		/// <param name="dst">反転後のテクスチャ格納先</param>
		public static void HorizontalAndVertical(Texture src, RenderTexture dst)
		{
			if(reverseUVMaterial == null)
				InitReverseUVMaterial();
			SetReverseUVProperty(REVERSE, REVERSE);
			Blit(src, dst);
		}

		#endregion PublicMethod

		#region PrivateField

		/// <summary>
		/// マテリアルを初期化する
		/// </summary>
		private static void InitReverseUVMaterial()
		{
			var shader = Shader.Find(REVERSE_UV_SHADER);
			reverseUVMaterial = new Material(shader);
		}

		/// <summary>
		/// マテリアルに値を設定する
		/// </summary>
		/// <param name="x">x方向の反転値</param>
		/// <param name="y">y方向の反転値</param>
		private static void SetReverseUVProperty(float x, float y)
		{
			reverseUVMaterial.SetFloat(REVERSE_X, x);
			reverseUVMaterial.SetFloat(REVERSE_Y, y);
		}

		private static void Blit(Texture src, RenderTexture dst)
		{
			var tmp = RenderTexture.GetTemporary(src.width, src.height, 0);
			Graphics.Blit(src, tmp, reverseUVMaterial);
			Graphics.Blit(tmp, dst);
			RenderTexture.ReleaseTemporary(tmp);
		}

		#endregion PrivateField
	}
}