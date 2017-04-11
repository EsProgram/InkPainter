using UnityEngine;

namespace Es.InkPainter.Sample
{
	public class MousePainter : MonoBehaviour
	{
		/// <summary>
		/// Types of methods used to paint.
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
		private Brush brush;

		[SerializeField]
		private UseMethodType useMethodType = UseMethodType.RaycastHitInfo;

		private void Update()
		{
			if(Input.GetMouseButton(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				bool success = true;
				RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo))
				{
					var paintObject = hitInfo.transform.GetComponent<InkCanvas>();
					if(paintObject != null)
						switch(useMethodType)
						{
							case UseMethodType.RaycastHitInfo:
								success = paintObject.Paint(brush, hitInfo);
								break;

							case UseMethodType.WorldPoint:
								success = paintObject.Paint(brush, hitInfo.point);
								break;

							case UseMethodType.NearestSurfacePoint:
								success = paintObject.PaintNearestTriangleSurface(brush, hitInfo.point);
								break;

							case UseMethodType.DirectUV:
								if(!(hitInfo.collider is MeshCollider))
									Debug.LogWarning("Raycast may be unexpected if you do not use MeshCollider.");
								success = paintObject.PaintUVDirect(brush, hitInfo.textureCoord);
								break;
						}
					if(!success)
						Debug.LogError("Failed to paint.");
				}
			}
		}

		public void OnGUI()
		{
			if(GUILayout.Button("Reset"))
			{
				foreach(var canvas in FindObjectsOfType<InkCanvas>())
					canvas.ResetPaint();
			}
		}
	}
}