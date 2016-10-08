using UnityEngine;

namespace Es.TexturePaint
{
	/// <summary>
	/// テクスチャペイントのブラシ情報を管理するクラス
	/// </summary>
	[System.Serializable]
	public class PaintBlush
	{
		/// <summary>
		/// ブラシの色合成方式
		/// </summary>
		public enum ColorBlendType
		{
			/// <summary>
			/// ブラシ設定色を利用する
			/// </summary>
			UseColor,

			/// <summary>
			/// ブラシに設定したテクスチャを利用する
			/// </summary>
			UseBlush,

			/// <summary>
			/// ブラシ設定色とテクスチャを合成する
			/// </summary>
			Neutral,
		}

		/// <summary>
		/// ブラシの凹凸情報合成方式
		/// </summary>
		public enum BumpBlendType
		{
			/// <summary>
			/// ブラシ設定値を利用する
			/// </summary>
			UseBlush,

			/// <summary>
			/// 対象と比較して小さい方の値を利用する
			/// </summary>
			Min,

			/// <summary>
			/// 対象と比較して大きい方の値を利用する
			/// </summary>
			Max,
		}

		[SerializeField]
		private Texture2D blushTexture;

		[SerializeField]
		private Texture2D blushBumpTexture;

		[SerializeField, Range(0, 1)]
		private float blushScale = 0.1f;

		[SerializeField, Range(0, 1)]
		private float blushBumpBlend = 0.1f;

		[SerializeField]
		private Color blushColor;

		[SerializeField]
		private ColorBlendType colorBlendType;

		[SerializeField]
		private BumpBlendType bumpBlendType;

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

		/// <summary>
		/// カラー合成方式
		/// </summary>
		public ColorBlendType ColorBlending
		{
			get { return colorBlendType; }
			set { colorBlendType = value; }
		}

		/// <summary>
		/// 凹凸情報合成方式
		/// </summary>
		public BumpBlendType BumpBlending
		{
			get { return bumpBlendType; }
			set { bumpBlendType = value; }
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

		public PaintBlush(Texture2D blushTex, float scale, Color color, Texture2D bumpTex, float bumpBlend, ColorBlendType colorBlending, BumpBlendType bumpBlending)
		: this(blushTex, scale, color, bumpTex, bumpBlend)
		{
			ColorBlending = colorBlending;
			BumpBlending = bumpBlending;
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