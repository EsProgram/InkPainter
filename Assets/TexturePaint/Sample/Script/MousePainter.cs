using System.Collections;
using TexturePaint;
using UnityEngine;

namespace Es.TexturePaint.Sample
{
	public class MousePainter : MonoBehaviour
	{
		[SerializeField]
		private Texture2D blushTex;

		[SerializeField]
		private Texture2D blushBump;

		[SerializeField]
		private float scale;

		[SerializeField]
		private float bumpBlend;

		[SerializeField]
		private Color color;

		private PaintBlush blush;

		public void Awake()
		{
			blush = new PaintBlush(blushTex, scale, color, blushBump, bumpBlend);
		}

		private void Update()
		{
			if(Input.GetMouseButton(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo))
				{
					var paintObject = hitInfo.transform.GetComponent<DynamicCanvas>();
					if(paintObject != null)
					{
						paintObject.Paint(hitInfo, blush);
					}
				}
			}
		}
	}
}