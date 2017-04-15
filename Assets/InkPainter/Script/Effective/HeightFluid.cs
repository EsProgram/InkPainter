using UnityEngine;
using System;
using System.Collections;

namespace Es.InkPainter.Effective
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(InkCanvas), typeof(Renderer))]
	public class HeightFluid : MonoBehaviour
	{
		private Material heightFluid;
		private Material height2Normal;
		private Material height2Color;
		private Material singleColorFill;
		private InkCanvas canvas;

		[SerializeField]
		private bool useMainTextureFluid = true;

		[SerializeField]
		private bool useNormalMapFluid = true;

		[SerializeField]
		private int createTextureSize = 1024;

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
		private bool automaticPerformanceOptimization = true;

		[SerializeField, Range(0.01f, 10f)]
		private float automaticFluidProcessStopTime = 5f;

		private bool enabledFluid;

		private void Init(InkCanvas canvas)
		{
			foreach(var set in canvas.PaintDatas)
			{
				var heightPaint = canvas.GetPaintHeightTexture(set.material.name);
				if(heightPaint != null)
					SingleColorFill(heightPaint, Vector4.zero);
			}
		}

		private void SingleColorFill(RenderTexture texture, Color color)
		{
			var tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			singleColorFill.SetVector("_Color", color);
			Graphics.Blit(texture, tmp, singleColorFill);
			Graphics.Blit(tmp, texture);
			RenderTexture.ReleaseTemporary(tmp);
		}

		private void DisableFluid(InkCanvas canvas)
		{
			if(enabledFluid && automaticPerformanceOptimization)
				StartCoroutine(DisableFluid());
		}

		private IEnumerator DisableFluid()
		{
			yield return new WaitForSeconds(automaticFluidProcessStopTime);
			enabledFluid = false;
		}

		private void EnabledFluid(InkCanvas canvas, Brush brush)
		{
			enabledFluid = true;
			brush.ColorBlending = Brush.ColorBlendType.AlphaOnly;
			brush.NormalBlending = Brush.NormalBlendType.UseBrush;
			brush.HeightBlending = Brush.HeightBlendType.ColorRGB_HeightA;
		}

		private void Awake()
		{
			heightFluid = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightDrip"));
			height2Normal = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightToNormal"));
			height2Color = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.HeightToColor"));
			singleColorFill = new Material(Resources.Load<Material>("Es.InkPainter.Fluid.SingleColorFill"));

			canvas = GetComponent<InkCanvas>();
			canvas.OnInitializedAfter += Init;
			canvas.OnPaintStart += EnabledFluid;
			canvas.OnPaintEnd += DisableFluid;
		}

		private void OnWillRenderObject()
		{
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
				}
				var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				heightFluid.SetFloat("_ScaleFactor", flowingForce);
				heightFluid.SetFloat("_Viscosity", easeOfDripping);
				heightFluid.SetFloat("_HorizontalSpread", horizontalSpread);
				heightFluid.SetFloat("_InfluenceOfNormal", influenceOfNormal);
				heightFluid.SetVector("_FlowDirection", flowDirection.normalized);
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
						{
							var tmp = RenderTexture.GetTemporary(newMainPaint.width, newMainPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
							Graphics.Blit(canvas.GetMainTexture(materialName), tmp);
							Graphics.Blit(tmp, newMainPaint);
							RenderTexture.ReleaseTemporary(tmp);
						}
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
						if(canvas.GetNormalTexture(materialName) != null)
						{
							var tmp = RenderTexture.GetTemporary(newNormalPaint.width, newNormalPaint.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
							Graphics.Blit(canvas.GetNormalTexture(materialName), tmp);
							Graphics.Blit(tmp, newNormalPaint);
							RenderTexture.ReleaseTemporary(tmp);
						}
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
	}
}