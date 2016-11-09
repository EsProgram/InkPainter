using Es.Effective;
using Es.TexturePaint;
using System.Collections;
using UnityEngine;

namespace Es.TexturePaint.Sample
{
	[RequireComponent(typeof(DynamicCanvas))]
	public class TextureRecovery : MonoBehaviour
	{
		[SerializeField, Tooltip("モーフィング係数")]
		private float lerpCoefficient = 0.1f;

		[SerializeField, Tooltip("呼び出し時間")]
		private float callTimer = 0.1f;

		[SerializeField]
		private bool @fixed = false;

		private Material material;
		private DynamicCanvas canvas;

		private Texture defaultMainTexture;
		private RenderTexture paintMainTexture;
		private Texture defaultNormalMap;
		private RenderTexture paintNormalMap;
		private Texture defaultHeightMap;
		private RenderTexture paintHeightMap;

		private void Start()
		{
			material = GetComponent<MeshRenderer>().sharedMaterial;
			canvas = GetComponent<DynamicCanvas>();
			defaultMainTexture = canvas.GetMainTexture(material.name);
			paintMainTexture = canvas.GetPaintMainTexture(material.name);
			defaultNormalMap = canvas.GetNormalTexture(material.name);
			paintNormalMap = canvas.GetPaintNormalTexture(material.name);
			defaultHeightMap = canvas.GetHeightTexture(material.name);
			paintHeightMap = canvas.GetPaintHeightTexture(material.name);
			StartCoroutine(TextureLerp());
		}

		public void FixedUpdate()
		{
			if(!@fixed)
				return;

			if(defaultMainTexture != null && paintMainTexture != null)
				TextureMorphing.Lerp(defaultMainTexture, paintMainTexture, lerpCoefficient);
			if(defaultNormalMap != null && paintNormalMap != null)
				TextureMorphing.Lerp(defaultNormalMap, paintNormalMap, lerpCoefficient);
			if(defaultHeightMap != null && paintHeightMap != null)
				TextureMorphing.Lerp(defaultHeightMap, paintHeightMap, lerpCoefficient);
		}

		private IEnumerator TextureLerp()
		{
			const int CALL_COUNT = 10;
			while(true)
			{
				if(@fixed)
					yield return new WaitForSeconds(1f);
				else
					for(int i = 0; i < CALL_COUNT; ++i)
					{
						yield return new WaitForSeconds(callTimer / 10);
						if(defaultMainTexture != null && paintMainTexture != null)
							TextureMorphing.Lerp(defaultMainTexture, paintMainTexture, lerpCoefficient / CALL_COUNT);
						if(defaultNormalMap != null && paintNormalMap != null)
							TextureMorphing.Lerp(defaultNormalMap, paintNormalMap, lerpCoefficient / CALL_COUNT);
						if(defaultHeightMap != null && paintHeightMap != null)
							TextureMorphing.Lerp(defaultHeightMap, paintHeightMap, lerpCoefficient / CALL_COUNT);
					}
			}
		}
	}
}