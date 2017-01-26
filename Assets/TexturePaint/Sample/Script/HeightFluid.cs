using UnityEngine;

namespace Es.TexturePaint.Sample
{
	[RequireComponent(typeof(DynamicCanvas), typeof(Renderer))]
	public class HeightFluid : MonoBehaviour
	{
		private const int CREATE_TEXTURE_SIZE = 1024;

		private Material heightFluid;
		private Material height2Normal;
		private Material height2Color;
		private Material singleColorFill;
		private Material material;
		private string materialName;
		private DynamicCanvas canvas;

		[SerializeField]
		private Vector2 flowDirection;

		[SerializeField]
		private float flowingForce = 1;

		[SerializeField, Range(0.01f, 1f)]
		private float viscosity = 0.1f;

		[SerializeField]
		private float normalScaleFactor = 1;

		private void Init(DynamicCanvas canvas)
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
			heightFluid = Resources.Load<Material>("Es.TexturePaint.Fluid.HeightSimpleStretch");
			height2Normal = Resources.Load<Material>("Es.TexturePaint.Fluid.HeightToNormal");
			height2Color = Resources.Load<Material>("Es.TexturePaint.Fluid.HeightToColor");
			singleColorFill = Resources.Load<Material>("Es.TexturePaint.Fluid.SingleColorFill");
		}

		private void Start()
		{
			material = GetComponent<Renderer>().sharedMaterial;
			materialName = material.name;

			canvas = GetComponent<DynamicCanvas>();
			canvas.OnInitializedAfter = Init;
		}

		private void OnWillRenderObject()
		{
			//HeightMapを垂らす
			var heightPaint = canvas.GetPaintHeightTexture(materialName);
			if(heightPaint == null)
			{
				var newHeightPaint = new RenderTexture(CREATE_TEXTURE_SIZE, CREATE_TEXTURE_SIZE, 0);
				InitHeightMap(newHeightPaint);
				canvas.SetPaintHeightTexture(materialName, newHeightPaint);
				heightPaint = newHeightPaint;
			}
			var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height);
			heightFluid.SetFloat("_ScaleFactor", flowingForce);
			heightFluid.SetFloat("_Viscosity", viscosity);
			heightFluid.SetVector("_FlowDirection", flowDirection.normalized);
			Graphics.Blit(heightPaint, heightTmp, heightFluid);
			Graphics.Blit(heightTmp, heightPaint);
			RenderTexture.ReleaseTemporary(heightTmp);

			//HeightMapからMainTexture生成
			var mainPaint = canvas.GetPaintMainTexture(materialName);
			if(mainPaint == null)
			{
				var newMainPaint = new RenderTexture(CREATE_TEXTURE_SIZE, CREATE_TEXTURE_SIZE, 0);
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
			Graphics.Blit(heightPaint, mainTmp, height2Color);
			Graphics.Blit(mainTmp, mainPaint);
			RenderTexture.ReleaseTemporary(mainTmp);

			//HeightMapからNormalMap生成
			var normalPaint = canvas.GetPaintNormalTexture(materialName);
			if(normalPaint == null)
			{
				var newNormalPaint = new RenderTexture(CREATE_TEXTURE_SIZE, CREATE_TEXTURE_SIZE, 0);
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
			Graphics.Blit(heightPaint, normalTmp, height2Normal);
			Graphics.Blit(normalTmp, normalPaint);
			RenderTexture.ReleaseTemporary(normalTmp);
		}
	}
}