using UnityEngine;

namespace Es.Utility
{
	/// <summary>
	/// Make it possible to operate cameras like Scene view in Game view.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class SceneViewCamera : MonoBehaviour
	{
		[SerializeField, Range(0.1f, 100f)]
		private float wheelSpeed = 1f;

		[SerializeField, Range(0.1f, 100f)]
		private float moveSpeed = 0.3f;

		[SerializeField, Range(0.1f, 1f)]
		private float rotateSpeed = 0.3f;

		private Vector3 preMousePos;

		private void Update()
		{
			MouseUpdate();
			return;
		}

		private void MouseUpdate()
		{
			float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
			if(scrollWheel != 0.0f)
				MouseWheel(scrollWheel);

			if(Input.GetMouseButtonDown(0) ||
			   Input.GetMouseButtonDown(1) ||
			   Input.GetMouseButtonDown(2))
				preMousePos = Input.mousePosition;

			MouseDrag(Input.mousePosition);
		}

		private void MouseWheel(float delta)
		{
			transform.position += transform.forward * delta * wheelSpeed;
			return;
		}

		private void MouseDrag(Vector3 mousePos)
		{
			Vector3 diff = mousePos - preMousePos;

			if(diff.magnitude < Vector3.kEpsilon)
				return;

			if(Input.GetMouseButton(2))
				transform.Translate(-diff * Time.deltaTime * moveSpeed);
			else if(Input.GetMouseButton(1))
				CameraRotate(new Vector2(-diff.y, diff.x) * rotateSpeed);

			preMousePos = mousePos;
		}

		public void CameraRotate(Vector2 angle)
		{
			transform.RotateAround(transform.position, transform.right, angle.x);
			transform.RotateAround(transform.position, Vector3.up, angle.y);
		}
	}
}