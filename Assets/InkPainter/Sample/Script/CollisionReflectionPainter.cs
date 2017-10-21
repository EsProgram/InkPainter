using Es.InkPainter.Effective;
using System.Linq;
using UnityEngine;

namespace Es.InkPainter.Sample
{
	[RequireComponent(typeof(Collider))]
	public class CollisionReflectionPainter : MonoBehaviour
	{
		[SerializeField]
		private Brush brush = null;

		[SerializeField]
		private Camera cam;

		[SerializeField]
		private Vector3 offset;

		[SerializeField]
		private bool debugMode;

		private RenderTexture rt;
		private RenderTexture debug;

		public void OnGUI()
		{
			if(debugMode)
			{
				GUI.Box(new Rect(0, 0, 200, 200), "ReflectionImage");
				GUI.DrawTexture(new Rect(0, 0, 200, 200), debug);
			}
		}

		private void Awake()
		{
			rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
			if(debugMode)
				debug = new RenderTexture(brush.BrushTexture.width, brush.BrushTexture.height, 16);
			cam.targetTexture = rt;
		}

		public void OnDestroy()
		{
			RenderTexture.ReleaseTemporary(rt);
		}

		public void OnCollisionStay(Collision collision)
		{
			if(cam == null)
				return;

			if(!collision.contacts.Any(p => p.otherCollider.GetComponent<InkCanvas>() != null))
				return;

			cam.transform.position = transform.position + offset;

			var contact = collision.contacts.First(p => p.otherCollider.GetComponent<InkCanvas>() != null);
			var canvas = contact.otherCollider.GetComponent<InkCanvas>();

			var buf = RenderTexture.GetTemporary(brush.BrushTexture.width, brush.BrushTexture.height);
			GrabArea.Clip(brush.BrushTexture, brush.Scale, rt, Vector3.one * 0.5f, brush.RotateAngle, GrabArea.GrabTextureWrapMode.Clamp, buf);
			ReverseUV.Vertical(buf, buf);

			if(debugMode)
				Graphics.Blit(buf, debug);

			var brushBuf = brush.BrushTexture;
			brush.BrushTexture = buf;

			canvas.Paint(brush, contact.point);

			RenderTexture.ReleaseTemporary(buf);
			brush.BrushTexture = brushBuf;
		}
	}
}