using TexturePaint;
using UnityEngine;

namespace Es.TexturePaint.Sample
{
	public class MousePainter : MonoBehaviour
	{
		[SerializeField, Tooltip("ブラシ形状のテクスチャ")]
		private Texture2D blushTex;

		[SerializeField, Tooltip("ブラシのインクの凹凸を表すテクスチャ")]
		private Texture2D blushBump;

		[SerializeField, Range(0.001f, 1f), Tooltip("ブラシの大きさ")]
		private float blushSize = 0.3f;

		[SerializeField, Range(0.001f, 1f), Tooltip("ブラシの凹凸ブレンド係数")]
		private float bumpBlend = 0.3f;

		[SerializeField, Tooltip("インクの色")]
		private Color color = Color.red;

		private void Update()
		{
			if(Input.GetMouseButton(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo))
				{
					var blush = new PaintBlush(blushTex, blushSize, color, blushBump, bumpBlend);
					var paintObject = hitInfo.transform.GetComponent<DynamicCanvas>();
					if(paintObject != null)
						paintObject.Paint(hitInfo, blush);
				}
			}
		}

		public void OnGUI()
		{
			if(GUILayout.Button("Reset"))
			{
				foreach(var canvas in FindObjectsOfType<DynamicCanvas>())
					canvas.ResetPaint();
			}
		}
	}
}