using UnityEngine;

namespace Es.InkPainter.Effective
{
	/// <summary>
	/// Class for obtaining specified range of texture.
	/// </summary>
	public static class GrabArea
	{
		/// <summary>
		/// Texture wrap mode.
		/// </summary>
		public enum GrabTextureWrapMode
		{
			Clamp,
			Repeat,
			Clip,
		}

		#region PrivateField

		private const string GRAB_AREA_MATERIAL = "Es.InkPainter.Effective.GrabArea";
		private const string CLIP = "_ClipTex";
		private const string TARGET = "_TargetTex";
		private const string CLIP_SCALE = "_ClipScale";
		private const string CLIP_UV = "_ClipUV";
		private const string ROTATE = "_Rotate";

		private const string WM_CLAMP = "WRAP_MODE_CLAMP";
		private const string WM_REPEAT = "WRAP_MODE_REPEAT";
		private const string WM_CLIP = "WRAP_MODE_CLIP";

		private const string ALPHA_REPLACE = "ALPHA_REPLACE";
		private const string ALPHA_NOT_REPLACE = "ALPHA_NOT_REPLACE";

		private static Material grabAreaMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		/// <summary>
		/// Crop the range specified by the clip texture from the target texture.
		/// </summary>
		/// <param name="clipTexture">Texture used for clipping.</param>
		/// <param name="clipScale">The ratio of the size of the clip texture to the target texture.</param>
		/// <param name="grabTargetTexture">Texture of clipping target.</param>
		/// <param name="targetUV">UV coordinates on the target texture.</param>
		/// <param name="rotateAngle">Clip texture rotate angle.(degree)</param>
		/// <param name="wrapMpde">Texture wrap mode.</param>
		/// <param name="dst">Store cropped texture.</param>
		/// <param name="replaceAlpha">Replace to clip textures alpha</param>
		public static void Clip(Texture clipTexture, float clipScale, Texture grabTargetTexture, Vector2 targetUV,float rotateAngle, GrabTextureWrapMode wrapMode, RenderTexture dst, bool replaceAlpha = true)
		{
			if(grabAreaMaterial == null)
				InitGrabAreaMaterial();
			SetGrabAreaProperty(clipTexture, clipScale, grabTargetTexture, targetUV, rotateAngle, wrapMode, replaceAlpha);
			var tmp = RenderTexture.GetTemporary(clipTexture.width, clipTexture.height, 0);
			Graphics.Blit(clipTexture, tmp, grabAreaMaterial);
			Graphics.Blit(tmp, dst);
			RenderTexture.ReleaseTemporary(tmp);
		}

		#endregion PublicMethod

		#region PrivateMethod

		/// <summary>
		/// Initialize the material.
		/// </summary>
		private static void InitGrabAreaMaterial()
		{
			grabAreaMaterial = new Material(Resources.Load<Material>(GRAB_AREA_MATERIAL));
		}

		/// <summary>
		/// Set the value in the material.
		/// </summary>
		/// <param name="clip">Texture used for clipping.</param>
		/// <param name="clipScale">The ratio of the size of the clip texture to the target texture.</param>
		/// <param name="grabTarget">Texture of clipping target.</param>
		/// <param name="targetUV">UV coordinates on the target texture.</param>
		/// <param name="wrapMpde">Texture wrap mode.</param>
		/// <param name="replaceAlpha">Replace to clip textures alpha</param>
		private static void SetGrabAreaProperty(Texture clip, float clipScale, Texture grabTarget, Vector2 targetUV, float rotateAngle, GrabTextureWrapMode wrapMpde, bool replaceAlpha)
		{
			grabAreaMaterial.SetTexture(CLIP, clip);
			grabAreaMaterial.SetTexture(TARGET, grabTarget);
			grabAreaMaterial.SetFloat(CLIP_SCALE, clipScale);
			grabAreaMaterial.SetFloat(ROTATE, rotateAngle);
			grabAreaMaterial.SetVector(CLIP_UV, targetUV);

			foreach(var key in grabAreaMaterial.shaderKeywords)
				grabAreaMaterial.DisableKeyword(key);
			switch(wrapMpde)
			{
				case GrabTextureWrapMode.Clamp:
					grabAreaMaterial.EnableKeyword(WM_CLAMP);
					break;

				case GrabTextureWrapMode.Repeat:
					grabAreaMaterial.EnableKeyword(WM_REPEAT);
					break;

				case GrabTextureWrapMode.Clip:
					grabAreaMaterial.EnableKeyword(WM_CLIP);
					break;

				default:
					break;
			}

			switch(replaceAlpha)
			{
				case true:
					grabAreaMaterial.EnableKeyword(ALPHA_REPLACE);
					break;
				case false:
					grabAreaMaterial.EnableKeyword(ALPHA_NOT_REPLACE);
					break;
			}
		}

		#endregion PrivateMethod
	}
}