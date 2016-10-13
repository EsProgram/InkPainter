using UnityEngine;

namespace Es.TexturePaint.Sample
{
	public class MousePainter : MonoBehaviour
	{
		/// <summary>
		/// ペイントに使用するメソッドの種類
		/// </summary>
		[System.Serializable]
		private enum UseMethodType
		{
			RaycastHitInfo,
			WorldPoint,
			NearestSurfacePoint,
			DirectUV,
		}

		[SerializeField]
		private PaintBlush blush;

		[SerializeField]
		private UseMethodType useMethodType = UseMethodType.RaycastHitInfo;

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
						switch(useMethodType)
						{
							case UseMethodType.RaycastHitInfo:
								paintObject.Paint(blush, hitInfo);
								break;

							case UseMethodType.WorldPoint:
								paintObject.Paint(blush, hitInfo.point);

								break;

							case UseMethodType.NearestSurfacePoint:
								paintObject.PaintNearestTriangleSurface(blush, hitInfo.point);
								break;

							case UseMethodType.DirectUV:
								paintObject.PaintUVDirect(blush, hitInfo.textureCoord);
								break;

							default:
								break;
						}
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