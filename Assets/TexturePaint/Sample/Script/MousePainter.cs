using UnityEngine;

namespace Es.TexturePaint.Sample
{
	public class MousePainter : MonoBehaviour
	{
		[SerializeField]
		private PaintBlush blush;

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
						paintObject.Paint(blush, hitInfo.point);
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