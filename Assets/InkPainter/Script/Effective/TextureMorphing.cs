using UnityEngine;

namespace Es.InkPainter.Effective
{
	/// <summary>
	/// A class that morphs the texture.
	/// </summary>
	public static class TextureMorphing
	{
		#region PrivateField

		private const string TEXTURE_MORPHING_MATERIAL = "Es.InkPainter.Effective.TextureMorphing";
		private const string LERP_COEFFICIENT = "_LerpCoef";
		private const string SRC_TEX = "_SrcTex";
		private const string DST_TEX = "_DstTex";

		private static Material morphingMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// Create a texture that complements two textures.
		/// </summary>
		/// <param name="src">Texture to use for morphing.</param>
		/// <param name="dst">Texture to use for morphing. It is overwritten after calculation.</param>
		/// <param name="lerpCoef">Interpolation coefficient.</param>
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

		private static void InitMorphingMaterial()
		{
			morphingMaterial = new Material(Resources.Load<Material>(TEXTURE_MORPHING_MATERIAL));
		}

		private static void SetMorphingProperty(Texture src, RenderTexture dst, float lerpCoef)
		{
			morphingMaterial.SetTexture(Shader.PropertyToID(SRC_TEX), src);
			morphingMaterial.SetTexture(Shader.PropertyToID(DST_TEX), dst);
			morphingMaterial.SetFloat(Shader.PropertyToID(LERP_COEFFICIENT), lerpCoef);
		}

		#endregion PrivateMethod
	}
}