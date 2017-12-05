using UnityEngine;
using System.Linq;
using System.Collections;

namespace Es.InkPainter.Effective
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(InkCanvas))]
	public class HeightFluid : MonoBehaviour
	{
		/// <summary>
		/// Color synthesis algorithm
		/// </summary>
		enum ColorSynthesis
		{
			Add,
			Overwrite,
		}

		#region SerializedField

		[SerializeField]
		private bool useMainTextureFluid = true;

		[SerializeField]
		private bool useNormalMapFluid = true;

		[SerializeField]
		private int createTextureSize = 1024;

		[SerializeField]
		private ColorSynthesis colorSynthesis = ColorSynthesis.Overwrite;

		[SerializeField, Range(0, 1)]
		private float alpha = 1f;

		[SerializeField]
		private Vector2 flowDirection = Vector2.up;

		[SerializeField, Range(0, 1)]
		private float flowingForce = 1;

		[SerializeField, Range(0.1f, 10f)]
		private float easeOfDripping = 1.0f;

		[SerializeField, Range(1f, 0f)]
		private float influenceOfNormal = 1;

		[SerializeField, Range(0.01f, 1)]
		private float horizontalSpread = 0.01f;

		[SerializeField]
		private float normalScaleFactor = 1;

		[SerializeField, Range(0.001f, 0.999f)]
		private float AdhesionBorder = 0.01f;

		[SerializeField]
		private bool performanceOptimize = true;

		[SerializeField, Range(0.01f, 10f)]
		private float fluidProcessStopTime = 5f;

		#endregion SerializedField

		#region PrivateField

		private bool enabledFluid;
		private float lastPaintedTime;
		private Material heightFluid;
		private Material height2Normal;
		private Material height2Color;
		private Material singleColorFill;
		private Material invertAlpha;
		private InkCanvas canvas;
		private Color lastPaintedColor;

		#endregion PrivateField

		#region ShaderKeywords
		private const string COLOR_SYNTHESIS_ADD = "COLOR_SYNTHESIS_ADD";
		private const string COLOR_SYNTHESIS_OVERWRITE = "COLOR_SYNTHESIS_OVERWRITE";
		#endregion ShaderKeywords

		#region PrivateMethod

		/// <summary>
		/// Fluid process initialization.
		/// </summary>
		/// <param name="canvas">Target canvas.</param>
		private void Init(InkCanvas canvas)
		{
			foreach(var set in canvas.PaintDatas)
			{
				var heightPaint = canvas.GetPaintHeightTexture(set.material.name);
				if(heightPaint != null)
					SingleColorFill(heightPaint, Vector4.zero);
				canvas.OnPaintStart += (own, brush) =>
				{
					if(lastPaintedColor != brush.Color)
					{
						lastPaintedColor = brush.Color;
						StopFluid();
					}
				};
			}
		}

		/// <summary>
		/// Fill single color.
		/// </summary>
		/// <param name="texture">Target render texture.</param>
		/// <param name="color">Color.</param>
		private void SingleColorFill(RenderTexture texture, Color color)
		{
			var tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			singleColorFill.SetVector("_Color", color);
			Graphics.Blit(texture, tmp, singleColorFill);
			Graphics.Blit(tmp, texture);
			RenderTexture.ReleaseTemporary(tmp);
		}

		/// <summary>
		/// Invert texture alpha.
		/// </summary>
		/// <param name="texture">Target render texture.</param>
		private void InvertAlpha(RenderTexture texture)
		{
			var tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Graphics.Blit(texture, tmp, invertAlpha);
			Graphics.Blit(tmp, texture);
			RenderTexture.ReleaseTemporary(tmp);
		}

		/// <summary>
		/// Enabled fluid procass.
		/// </summary>
		/// <param name="canvas">Target canvas.</param>
		/// <param name="brush">Brush used painting.</param>
		private void EnabledFluid(InkCanvas canvas, Brush brush)
		{
			enabledFluid = true;
			lastPaintedTime = Time.time;
			brush.ColorBlending = Brush.ColorBlendType.AlphaOnly;
			brush.NormalBlending = Brush.NormalBlendType.UseBrush;
			brush.HeightBlending = Brush.HeightBlendType.ColorRGB_HeightA;
		}

		/// <summary>
		/// Stop fluid process.
		/// </summary>
		private void StopFluid()
		{
			foreach(var set in canvas.PaintDatas)
			{
				var materialName = set.material.name;
				var heightPaint = canvas.GetPaintHeightTexture(materialName);
				if(heightPaint != null)
					InvertAlpha(heightPaint);
			}
		}


		#endregion PrivateMethod

		#region UnityEventMethod

		private void Awake()
		{
			heightFluid = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightDrip"));
			height2Normal = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightToNormal"));
			height2Color = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightToColor"));
			singleColorFill = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.SingleColorFill"));
			invertAlpha = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.InvertAlpha"));

			canvas = GetComponent<InkCanvas>();
			canvas.OnInitializedAfter += Init;
			canvas.OnPaintStart += EnabledFluid;
		}

		private void OnWillRenderObject()
		{
			if(performanceOptimize && enabledFluid && Time.time - lastPaintedTime > fluidProcessStopTime)
			{
				//In order to prevent continuation of dripping, reversing the sign of the adhesion amount.
				StopFluid();
				enabledFluid = false;
			}

			if(!enabledFluid)
				return;

			foreach(var set in canvas.PaintDatas)
			{
				var materialName = set.material.name;
				var heightPaint = canvas.GetPaintHeightTexture(materialName);
				if(heightPaint == null)
				{
					var newHeightPaint = new RenderTexture(createTextureSize, createTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					SingleColorFill(newHeightPaint, Vector4.zero);
					canvas.SetPaintHeightTexture(materialName, newHeightPaint);
					heightPaint = newHeightPaint;
					set.material.SetFloat("_Parallax", 0);
				}
				var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				heightFluid.SetFloat("_ScaleFactor", flowingForce);
				heightFluid.SetFloat("_Viscosity", easeOfDripping);
				heightFluid.SetFloat("_HorizontalSpread", horizontalSpread);
				heightFluid.SetFloat("_InfluenceOfNormal", influenceOfNormal);
				heightFluid.SetVector("_FlowDirection", flowDirection.normalized);
				heightFluid.SetVector("_FixedColor", lastPaintedColor);
				foreach(var key in heightFluid.shaderKeywords)
					heightFluid.DisableKeyword(key);
				switch(colorSynthesis)
				{
					case ColorSynthesis.Add:
						heightFluid.EnableKeyword(COLOR_SYNTHESIS_ADD);
						break;
					case ColorSynthesis.Overwrite:
					default:
						heightFluid.EnableKeyword(COLOR_SYNTHESIS_OVERWRITE);
						break;
				}
				if(canvas.GetNormalTexture(materialName) != null)
					heightFluid.SetTexture("_NormalMap", canvas.GetNormalTexture(materialName));
				Graphics.Blit(heightPaint, heightTmp, heightFluid);
				Graphics.Blit(heightTmp, heightPaint);
				RenderTexture.ReleaseTemporary(heightTmp);

				if(useMainTextureFluid)
				{
					var mainPaint = canvas.GetPaintMainTexture(materialName);
					if(mainPaint == null)
					{
						var newMainPaint = new RenderTexture(createTextureSize, createTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
						if(canvas.GetMainTexture(materialName) != null)
							Graphics.Blit(canvas.GetMainTexture(materialName), newMainPaint);
						canvas.SetPaintMainTexture(materialName, newMainPaint);
						mainPaint = newMainPaint;
					}
					var mainTmp = RenderTexture.GetTemporary(mainPaint.width, mainPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					height2Color.SetTexture("_ColorMap", mainPaint);
					height2Color.SetTexture("_BaseColor", canvas.GetMainTexture(materialName));
					height2Color.SetFloat("_Alpha", alpha);
					height2Color.SetFloat("_Border", AdhesionBorder);
					Graphics.Blit(heightPaint, mainTmp, height2Color);
					Graphics.Blit(mainTmp, mainPaint);
					RenderTexture.ReleaseTemporary(mainTmp);
				}

				if(useNormalMapFluid)
				{
					var normalPaint = canvas.GetPaintNormalTexture(materialName);
					if(normalPaint == null)
					{
						var newNormalPaint = new RenderTexture(createTextureSize, createTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
						//DXT5nm color initialization.
						SingleColorFill(newNormalPaint, Vector4.one * 0.5f);
						set.material.EnableKeyword("_NORMALMAP");
						if(canvas.GetNormalTexture(materialName) != null)
							Graphics.Blit(canvas.GetNormalTexture(materialName), newNormalPaint);
						canvas.SetPaintNormalTexture(materialName, newNormalPaint);
						normalPaint = newNormalPaint;
					}
					var normalTmp = RenderTexture.GetTemporary(normalPaint.width, normalPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					height2Normal.SetTexture("_BumpMap", normalPaint);
					height2Normal.SetFloat("_NormalScaleFactor", normalScaleFactor);
					height2Normal.SetFloat("_Border", AdhesionBorder);
					Graphics.Blit(heightPaint, normalTmp, height2Normal);
					Graphics.Blit(normalTmp, normalPaint);
					RenderTexture.ReleaseTemporary(normalTmp);
				}
			}
		}

		#endregion UnityEventMethod
	}
}