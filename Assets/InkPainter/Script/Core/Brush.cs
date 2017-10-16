using System;
using UnityEngine;

namespace Es.InkPainter
{
	/// <summary>
	/// Class managing brush information.
	/// </summary>
	[System.Serializable]
	public class Brush : ICloneable
	{
		/// <summary>
		/// Color synthesis method.
		/// </summary>
		public enum ColorBlendType
		{
			/// <summary>
			/// Use the color set for the brush.
			/// </summary>
			UseColor,

			/// <summary>
			/// Use brush texture as it is.
			/// </summary>
			UseBrush,

			/// <summary>
			/// Use the intermediate color between the color of the brush and the color of the texture.
			/// </summary>
			Neutral,

			/// <summary>
			/// Only use the alpha value set for the brush.
			/// </summary>
			AlphaOnly,
		}

		/// <summary>
		/// Brush normal information synthesis method.
		/// </summary>
		public enum NormalBlendType
		{
			/// <summary>
			/// Use the value set for the brush.
			/// </summary>
			UseBrush,

			/// <summary>
			/// Add and synthesize.
			/// </summary>
			Add,

			/// <summary>
			/// Subtract and synthesize.
			/// </summary>
			Sub,

			/// <summary>
			/// Use the minimum value.
			/// </summary>
			Min,

			/// <summary>
			/// Use the maximum value.
			/// </summary>
			Max,
		}

		/// <summary>
		/// Brush height information synthesis method.
		/// </summary>
		public enum HeightBlendType
		{
			/// <summary>
			/// Use the value set for the brush.
			/// </summary>
			UseBrush,

			/// <summary>
			/// Add and synthesize.
			/// </summary>
			Add,

			/// <summary>
			/// Subtract and synthesize.
			/// </summary>
			Sub,

			/// <summary>
			/// Use the minimum value.
			/// </summary>
			Min,

			/// <summary>
			/// Use the maximum value.
			/// </summary>
			Max,

			/// <summary>
			/// Composite color information to RGB and height to A.
			/// </summary>
			ColorRGB_HeightA,
		}

		[SerializeField]
		private Texture brushTexture;

		[SerializeField]
		private Texture brushNormalTexture;

		[SerializeField]
		private Texture brushHeightTexture;

		[SerializeField, Range(0, 1)]
		private float brushScale = 0.1f;

		[SerializeField, Range(0, 360)]
		private float rotateAngle = 0;

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
		/// Brush texture.
		/// </summary>
		public Texture BrushTexture
		{
			get { return brushTexture; }
			set { brushTexture = value; }
		}

		/// <summary>
		/// Brush's normal texture
		/// </summary>
		public Texture BrushNormalTexture
		{
			get { return brushNormalTexture; }
			set { brushNormalTexture = value; }
		}

		/// <summary>
		/// Brush height texture.
		/// </summary>
		public Texture BrushHeightTexture
		{
			get { return brushHeightTexture; }
			set { brushHeightTexture = value; }
		}

		/// <summary>
		/// The size of the brush.
		/// It takes a range from 0 to 1.
		/// </summary>
		public float Scale
		{
			get { return Mathf.Clamp01(brushScale); }
			set { brushScale = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// Rotate angle of the brush.
		/// </summary>
		public float RotateAngle
		{
			get { return rotateAngle; }
			set { rotateAngle = value; }
		}

		/// <summary>
		/// Blending coefficient of normal information.
		/// It takes a range from 0 to 1.
		/// </summary>
		public float NormalBlend
		{
			get { return Mathf.Clamp01(brushNormalBlend); }
			set { brushNormalBlend = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// Blending factor of height information.
		/// It takes a range from 0 to 1.
		/// </summary>
		public float HeightBlend
		{
			get { return Mathf.Clamp01(brushHeightBlend); }
			set { brushHeightBlend = Mathf.Clamp01(value); }
		}

		/// <summary>
		/// The color of the brush.
		/// </summary>
		public Color Color
		{
			get { return brushColor; }
			set { brushColor = value; }
		}

		/// <summary>
		/// Color synthesis method.
		/// </summary>
		public ColorBlendType ColorBlending
		{
			get { return colorBlendType; }
			set { colorBlendType = value; }
		}

		/// <summary>
		/// Brush normal information synthesis method.
		/// </summary>
		public NormalBlendType NormalBlending
		{
			get { return normalBlendType; }
			set { normalBlendType = value; }
		}

		/// <summary>
		/// Brush height information synthesis method.
		/// </summary>
		public HeightBlendType HeightBlending
		{
			get { return heightBlendType; }
			set { heightBlendType = value; }
		}

		public Brush(Texture brushTex, float scale, Color color)
		{
			BrushTexture = brushTex;
			Scale = scale;
			Color = color;
		}

		public Brush(Texture brushTex, float scale, Color color, ColorBlendType colorBlending)
		  : this(brushTex, scale, color)
		{
			ColorBlending = colorBlending;
		}

		public Brush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend)
		  : this(brushTex, scale, color)
		{
			BrushNormalTexture = normalTex;
			NormalBlend = normalBlend;
		}

		public Brush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend, ColorBlendType colorBlending, NormalBlendType normalBlending)
		: this(brushTex, scale, color, normalTex, normalBlend)
		{
			ColorBlending = colorBlending;
			NormalBlending = normalBlending;
		}

		public Brush(Texture brushTex, float scale, Color color, Texture normalTex, float normalBlend, Texture heightTex, float heightBlend, ColorBlendType colorBlending, NormalBlendType normalBlending, HeightBlendType heightBlending)
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