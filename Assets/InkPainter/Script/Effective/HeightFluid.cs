using UnityEngine;

namespace Es.InkPainter.Effective
{
	[RequireComponent(typeof(InkCanvas), typeof(Renderer))]
	public class HeightFluid : MonoBehaviour
	{
		public bool enabledFluid = true;

		private Material heightFluid;
		private Material height2Normal;
		private Material height2Color;
		private Material singleColorFill;
		private string materialName;
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
		private Vector2 flowDirection;

		[SerializeField]
		private float flowingForce = 1;

		[SerializeField, Range(0.1f, 10f)]
		private float easeOfDripping = 0.8f;

		[SerializeField, Range(1f, 0f)]
		private float influenceOfNormal = 1;

		[SerializeField, Range(0.01f, 1)]
		private float horizontalSpread = 0.1f;

		[SerializeField]
		private float normalScaleFactor = 1;

		[SerializeField, Range(0f, 1f)]
		private float AdhesionBorder = 0.1f;

		private void Init(InkCanvas canvas)
		{
			var heightPaint = canvas.GetPaintHeightTexture(materialName);
			if(heightPaint != null)
				InitHeightMap(heightPaint);
		}

		private void InitHeightMap(RenderTexture heightPaint)
		{
			var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height);
			singleColorFill.SetVector("_Color", Vector4.zero);
			Graphics.Blit(heightPaint, heightTmp, singleColorFill);
			Graphics.Blit(heightTmp, heightPaint);
			RenderTexture.ReleaseTemporary(heightTmp);
		}

		private void Awake()
		{
			heightFluid = Resources.Load<Material>("Es.InkPainter.Fluid.HeightDrip");
			height2Normal = Resources.Load<Material>("Es.InkPainter.Fluid.HeightToNormal");
			height2Color = Resources.Load<Material>("Es.InkPainter.Fluid.HeightToColor");
			singleColorFill = Resources.Load<Material>("Es.InkPainter.Fluid.SingleColorFill");
			materialName = GetComponent<Renderer>().sharedMaterial.name;

			canvas = GetComponent<InkCanvas>();
			canvas.OnInitializedAfter += Init;
		}

		private void OnWillRenderObject()
		{
			var heightPaint = canvas.GetPaintHeightTexture(materialName);
			if(heightPaint == null)
			{
				var newHeightPaint = new RenderTexture(createTextureSize, createTextureSize, 0);
				InitHeightMap(newHeightPaint);
				canvas.SetPaintHeightTexture(materialName, newHeightPaint);
				heightPaint = newHeightPaint;
			}
			var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height);
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
					var newMainPaint = new RenderTexture(createTextureSize, createTextureSize, 0);
					if(canvas.GetMainTexture(materialName) != null)
					{
						var tmp = RenderTexture.GetTemporary(newMainPaint.width, newMainPaint.height, 0);
						Graphics.Blit(canvas.GetMainTexture(materialName), tmp);
						Graphics.Blit(tmp, newMainPaint);
						RenderTexture.ReleaseTemporary(tmp);
					}
					canvas.SetPaintMainTexture(materialName, newMainPaint);
					mainPaint = newMainPaint;
				}
				var mainTmp = RenderTexture.GetTemporary(mainPaint.width, mainPaint.height);
				height2Color.SetTexture("_ColorMap", mainPaint);
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
					var newNormalPaint = new RenderTexture(createTextureSize, createTextureSize, 0);
					if(canvas.GetNormalTexture(materialName) != null)
					{
						var tmp = RenderTexture.GetTemporary(newNormalPaint.width, newNormalPaint.height, 0);
						Graphics.Blit(canvas.GetNormalTexture(materialName), tmp);
						Graphics.Blit(tmp, newNormalPaint);
						RenderTexture.ReleaseTemporary(tmp);
					}
					canvas.SetPaintNormalTexture(materialName, newNormalPaint);
					normalPaint = newNormalPaint;
				}
				var normalTmp = RenderTexture.GetTemporary(normalPaint.width, normalPaint.height);
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