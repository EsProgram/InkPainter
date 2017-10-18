using UnityEngine;

namespace Es.InkPainter.Sample
{
	public class ReflectPainter : MonoBehaviour
	{
		[SerializeField]
		private Brush brush;

		[SerializeField]
		private GameObject camPref;

		private RenderTexture rt;
		private Camera cam;
		private Vector2 uv;
		private InkCanvas paintObject;

		public void Awake()
		{
			rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
			brush.ColorBlending = Brush.ColorBlendType.UseBrush;
		}

		public void OnGUI()
		{
			if(GUILayout.Button("Reset"))
			{
				if(paintObject != null)
					paintObject.ResetPaint();
				Destroy(cam);
				cam = null;
			}
		}

		private void Update()
		{
			if(cam == null && Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo))
				{
					paintObject = hitInfo.transform.GetComponent<InkCanvas>();
					if(paintObject != null)
					{
						uv = hitInfo.textureCoord;
						var camObj = Instantiate(camPref, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), hitInfo.transform) as GameObject;
						cam = camObj.GetComponent<Camera>();
						cam.targetTexture = rt;
						camObj.SetActive(true);
					}
				}
			}
			else if(cam != null)
			{
				var buf = RenderTexture.GetTemporary(brush.BrushTexture.width, brush.BrushTexture.height);
				Effective.GrabArea.Clip(brush.BrushTexture, brush.Scale, rt, Vector3.one * 0.5f, brush.RotateAngle, Effective.GrabArea.GrabTextureWrapMode.Clip, buf);
				Effective.ReverseUV.Horizontal(buf, buf);
				var brushBuf = brush.BrushTexture;
				brush.BrushTexture = buf;
				if(paintObject != null)
					paintObject.PaintUVDirect(brush, uv);
				RenderTexture.ReleaseTemporary(buf);
				brush.BrushTexture = brushBuf;
			}
		}
	}
}