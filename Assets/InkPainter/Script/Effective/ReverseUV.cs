using UnityEngine;

namespace Es.InkPainter.Effective
{
	/// <summary>
	/// Class to reverse UV.
	/// </summary>
	public static class ReverseUV
	{
		#region PrivateField

		private const string REVERSE_UV_MATERIAL = "Es.InkPainter.Effective.ReverseUV";
		private const string REVERSE_X = "_ReverseX";
		private const string REVERSE_Y = "_ReverseY";

		private const float DEFAULT = 1f;
		private const float REVERSE = 0f;

		private static Material reverseUVMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// Performs horizontal reversal.
		/// </summary>
		/// <param name="src">Texture to invert.</param>
		/// <param name="dst">The storage location of inverted texture.</param>
		public static void Horizontal(Texture src, RenderTexture dst)
		{
			if(reverseUVMaterial == null)
				InitReverseUVMaterial();
			SetReverseUVProperty(REVERSE, DEFAULT);
			Blit(src, dst);
		}

		/// <summary>
		/// Performs vertical reversal.
		/// </summary>
		/// <param name="src">Texture to invert.</param>
		/// <param name="dst">The storage location of inverted texture.</param>
		public static void Vertical(Texture src, RenderTexture dst)
		{
			if(reverseUVMaterial == null)
				InitReverseUVMaterial();
			SetReverseUVProperty(DEFAULT, REVERSE);
			Blit(src, dst);
		}

		/// <summary>
		/// Performs horizontal and vertical reversal.
		/// </summary>
		/// <param name="src">Texture to invert.</param>
		/// <param name="dst">The storage location of inverted texture.</param>
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
		/// Initialize the material.
		/// </summary>
		private static void InitReverseUVMaterial()
		{
			reverseUVMaterial = new Material(Resources.Load<Material>(REVERSE_UV_MATERIAL));
		}

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