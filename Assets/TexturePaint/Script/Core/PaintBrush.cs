using System;
using UnityEngine;

namespace Es.TexturePaint
{
	/// <summary>
	/// テクスチャペイントのブラシ情報を管理するクラス
	/// </summary>
	[System.Serializable]
	public class PaintBrush : ICloneable
	{
		/// <summary>
		/// ブラシの色合成方式
		/// </summary>
		public enum ColorBlendType
		{
			/// <summary>
			/// ブラシ設定値を利用する
			/// </summary>
			UseColor,

			/// <summary>
			/// ブラシに設定したテクスチャを利用する
			/// </summary>
			UseBrush,

			/// <summary>
			/// ブラシ設定値とテクスチャを合成する
			/// </summary>
			Neutral,

			/// <summary>
			/// ブラシ設定値のアルファ値のみを合成する
			/// </summary>
			AlphaOnly,
		}

		/// <summary>
		/// ブラシの凹凸情報合成方式
		/// </summary>
		public enum NormalBlendType
		{
			/// <summary>
			/// ブラシ設定値を利用する
			/// </summary>
			UseBrush,

			/// <summary>
			/// 対象を加算する
			/// </summary>
			Add,

			/// <summary>
			/// 対象を減算する
			/// </summary>
			Sub,

			/// <summary>
			/// 対象と比較して小さい方の値を利用する
			/// </summary>
			Min,

			/// <summary>
			/// 対象と比較して大きい方の値を利用する
			/// </summary>
			Max,
		}

		public enum HeightBlendType
		{
			/// <summary>
			/// ブラシ設定値を利用する
			/// </summary>
			UseBrush,

			/// <summary>
			/// 対象を加算する
			/// </summary>
			Add,

			/// <summary>
			/// 対象を減算する
			/// </summary>
			Sub,

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
		private Texture brushTexture;

		[SerializeField]
		private Texture brushNormalTexture;

		[SerializeField]
		private Texture brushHeightTexture;

		[SerializeField, Range(0, 1)]
		private float brushScale = 0.1f;

		[SerializeField, Range(0, 1)]
		private float brushNormalBlend = 0.1f;

		[SerializeField, Range(0, 1)]
		private float brushHeightBlend = 0.1f;

		[SerializeField]
		private Color brushColor;

		[SerializeField]
		private ColorBlendType colorBlendType;

		[SerializeField]
		private NormalBlendType normalBlendType;

		[SerializeField]
		private HeightBlendType heightBlendType;

		/// <summary>
		/// ブラシのテクスチャ
		/// </summary>
		public Texture BrushTexture
		{
			get { return brushTexture; }
			set { brushTexture = value; }
		}

		/// <summary>
		/// ブラシ法線マップテクスチャ
		/// </summary>
		public Texture BrushNormalTexture
		{
			get { return brushNormalTexture; }
			set { brushNormalTexture = value; }
		}

		/// <summary>
		/// ブラシハイトマップテクスチャ
		/// </summary>
		public Texture BrushHeightTexture
		{
			get { return brushHeightTexture; }
			set { brushHeightTexture = value; }
		}

		/// <summary>
		/// ブラシの大きさ
		/// [0,1]の範囲をとるテクスチャサイズの比
		/// </summary>
		public float Scale
		{
			get { return Mathf.Clamp01(brushScale); }
			set { brushScale = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// 法線マップブレンド係数
		/// [0,1]の範囲を取る
		/// </summary>
		public float NormalBlend
		{
			get { return Mathf.Clamp01(brushNormalBlend); }
			set { brushNormalBlend = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// ハイトマップブレンド係数
		/// [0,1]の範囲を取る
		/// </summary>
		public float HeightBlend
		{
			get { return Mathf.Clamp01(brushHeightBlend); }
			set { brushHeightBlend = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// ブラシの色
		/// </summary>
		public Color Color
		{
			get { return brushColor; }
			set { brushColor = value; }
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
		public NormalBlendType NormalBlending
		{
			get { return normalBlendType; }
			set { normalBlendType = value; }
		}

		/// <summary>
		/// 高さ情報合成方式
		/// </summary>
		public HeightBlendType HeightBlending
		{
			get { return heightBlendType; }
			set { heightBlendType = value; }
		}

		public PaintBrush(Texture brushTex, float scale, Color color)
		{
			BrushTexture = brushTex;
			Scale = scale;
			Color = color;
		}

		public PaintBrush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend)
		  : this(brushTex, scale, color)
		{
			BrushNormalTexture = normalTex;
			NormalBlend = normalBlend;
		}

		public PaintBrush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend, ColorBlendType colorBlending, NormalBlendType normalBlending)
		: this(brushTex, scale, color, normalTex, normalBlend)
		{
			ColorBlending = colorBlending;
			NormalBlending = normalBlending;
		}

		public PaintBrush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend, Texture heightTex, float heightBlend, ColorBlendType colorBlending, NormalBlendType normalBlending, HeightBlendType heightBlending)
		: this(brushTex, scale, color, normalTex, normalBlend, colorBlending, normalBlending)
		{
			BrushHeightTexture = heightTex;
			HeightBlend = heightBlend;
			HeightBlending = heightBlending;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}