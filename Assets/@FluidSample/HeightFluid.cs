using Es.TexturePaint;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DynamicCanvas))]
public class HeightFluid : MonoBehaviour
{
	[SerializeField]
	private Material heightFluid;

	[SerializeField]
	private Material height2Normal;

	[SerializeField]
	private Material height2Color;

	[SerializeField]
	private Material singleColorFill;

	[SerializeField]
	private Vector2 flowDirection;

	[SerializeField]
	private float flowingForce = 1;

	[SerializeField, Range(0.01f, 1f)]
	private float viscosity = 0.1f;

	[SerializeField]
	private float normalScaleFactor = 1;

	private void OnGUI()
	{
		if(GUI.Button(new Rect(0, 30, 150, 50), "真のリセット"))
		{
			var canvas = GetComponent<DynamicCanvas>();
			canvas.ResetPaint();
			Start();
		}
	}

	private void Start()
	{
		var canvas = GetComponent<DynamicCanvas>();
		var materialName = canvas.GetComponent<Renderer>().sharedMaterial.name;
		var heightPaint = canvas.GetPaintHeightTexture(materialName);
		var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height);
		singleColorFill.SetVector("_Color", Vector4.zero);
		Graphics.Blit(heightPaint, heightTmp, singleColorFill);
		Graphics.Blit(heightTmp, heightPaint);
		RenderTexture.ReleaseTemporary(heightTmp);
	}

	private void OnWillRenderObject()
	{
		var canvas = GetComponent<DynamicCanvas>();
		var materialName = canvas.GetComponent<Renderer>().sharedMaterial.name;

		//HeightMapを垂らす
		var heightPaint = canvas.GetPaintHeightTexture(materialName);
		var heightTmp = RenderTexture.GetTemporary(heightPaint.width, heightPaint.height);
		heightFluid.SetFloat("_ScaleFactor", flowingForce);
		heightFluid.SetFloat("_Viscosity", viscosity);
		heightFluid.SetVector("_FlowDirection", flowDirection.normalized);
		Graphics.Blit(heightPaint, heightTmp, heightFluid);
		Graphics.Blit(heightTmp, heightPaint);
		RenderTexture.ReleaseTemporary(heightTmp);

		//HeightMapからMainTexture生成
		var mainPaint = canvas.GetPaintMainTexture(materialName);
		var mainTmp = RenderTexture.GetTemporary(mainPaint.width, mainPaint.height);
		height2Color.SetTexture("_ColorMap", mainPaint);
		Graphics.Blit(heightPaint, mainTmp, height2Color);
		Graphics.Blit(mainTmp, mainPaint);
		RenderTexture.ReleaseTemporary(mainTmp);

		//HeightMapからNormalMap生成
		var normalPaint = canvas.GetPaintNormalTexture(materialName);
		var normalTmp = RenderTexture.GetTemporary(normalPaint.width, normalPaint.height);
		height2Normal.SetTexture("_BumpMap", normalPaint);
		height2Normal.SetFloat("_NormalScaleFactor", normalScaleFactor);
		Graphics.Blit(heightPaint, normalTmp, height2Normal);
		Graphics.Blit(normalTmp, normalPaint);
		RenderTexture.ReleaseTemporary(normalTmp);
	}
}