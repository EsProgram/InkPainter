using UnityEngine;

namespace TexturePaint
{
	[System.Serializable]
	public class PaintBlush
	{
		[SerializeField]
		private Texture2D blushTexture;

		[SerializeField]
		private Texture2D blushBumpTexture;

		[SerializeField]
		private float blushScale;

		[SerializeField]
		private float blushBumpBlend;

		[SerializeField]
		private Color blushColor;

		/// <summary>
		/// ブラシのテクスチャ
		/// </summary>
		public Texture2D BlushTexture
		{
			get { return blushTexture; }
			set { blushTexture = value; }
		}

		/// <summary>
		/// ブラシ法線マップテクスチャ
		/// </summary>
		public Texture2D BlushBumpTexture
		{
			get { return blushBumpTexture; }
			set { blushBumpTexture = value; }
		}

		/// <summary>
		/// ブラシの大きさ
		/// [0,1]の範囲をとるテクスチャサイズの比
		/// </summary>
		public float Scale
		{
			get { return Mathf.Clamp01(blushScale); }
			set { blushScale = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// 法線マップブレンド係数
		/// [0,1]の範囲を取る
		/// </summary>
		public float BumpBlend
		{
			get { return Mathf.Clamp01(blushBumpBlend); }
			set { blushBumpBlend = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// ブラシの色
		/// </summary>
		public Color Color
		{
			get { return blushColor; }
			set { blushColor = value; }
		}

		public PaintBlush(Texture2D blushTex, float scale, Color color)
		{
			BlushTexture = blushTex;
			Scale = scale;
			Color = color;
		}

		public PaintBlush(Texture2D blushTex, float scale, Color color, Texture2D bumpTex, float bumpBlend)
		  : this(blushTex, scale, color)
		{
			BlushBumpTexture = bumpTex;
			BumpBlend = bumpBlend;
		}

		public PaintBlush ShallowCopy()
		{
			return new PaintBlush(
				BlushTexture,
				Scale,
				Color,
				BlushBumpTexture,
				BumpBlend);
		}
	}
}