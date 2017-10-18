using Es.InkPainter.Effective;
using UnityEngine;

namespace Es.InkPainter.Sample
{
	public class ClipPainter : MonoBehaviour
	{
		[SerializeField]
		private bool grab = true;

		[SerializeField]
		private Brush brush = null;

		[SerializeField]
		private GrabArea.GrabTextureWrapMode wrapMode = GrabArea.GrabTextureWrapMode.Repeat;

		private RenderTexture t = null;
		private RaycastHit hitInfo;

		private void OnGUI()
		{
			GUI.Box(new Rect(0, 0, 300, 320), "");
			GUI.Box(new Rect(0, 0, 300, 300), "Grab Texture");
			if(t != null)
				GUI.DrawTexture(new Rect(0, 0, 300, 300), t);
			grab = GUI.Toggle(new Rect(0, 300, 300, 20), grab, "Grab");
		}

		public void Awake()
		{
			t = new RenderTexture(brush.BrushTexture.width, brush.BrushTexture.height, 0);
		}

		private void Update()
		{
			if(Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(ray, out hitInfo))
				{
					var d = hitInfo.transform.GetComponent<InkCanvas>();
					if(d != null && !grab)
					{
						d.Paint(brush, hitInfo);
					}
					if(grab)
					{
						GrabArea.Clip(brush.BrushTexture, brush.Scale, hitInfo.transform.GetComponent<MeshRenderer>().sharedMaterial.mainTexture, hitInfo.textureCoord, brush.RotateAngle, wrapMode, t);
						brush.BrushTexture = t;
						brush.ColorBlending = Brush.ColorBlendType.UseBrush;
						grab = false;
					}
				}
			}
		}
	}
}