using Es.Effective;
using Es.TexturePaint;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DynamicCanvas))]
public class TexMorphTest : MonoBehaviour
{
	[SerializeField, Range(0, 1)]
	private float lerpCoef = 0.1f;

	private Material mat;
	private DynamicCanvas canvas;

	private Texture tex;
	private RenderTexture rtex;

	public void Start()
	{
		mat = GetComponent<MeshRenderer>().sharedMaterial;
		canvas = GetComponent<DynamicCanvas>();
		tex = canvas.GetMainTexture(mat.name);
		rtex = canvas.GetPaintMainTexture(mat.name);
	}

	public void Update()
	{
		TextureMorphing.Lerp(tex, rtex, lerpCoef);
	}
}