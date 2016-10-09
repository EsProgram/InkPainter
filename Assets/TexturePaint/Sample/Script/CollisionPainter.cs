using Es.TexturePaint;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class CollisionPainter : MonoBehaviour
{
	[SerializeField]
	private PaintBlush blush = null;

	[SerializeField]
	private int wait = 3;

	private int waitCount;

	public void Awake()

	{
		GetComponent<MeshRenderer>().material.color = blush.Color;
	}

	public void FixedUpdate()
	{
		++waitCount;
	}

	public void OnCollisionStay(Collision collision)
	{
		if(waitCount < wait)
			return;
		waitCount = 0;

		foreach(var p in collision.contacts)
		{
			var canvas = p.otherCollider.GetComponent<DynamicCanvas>();
			if(canvas != null)
				Debug.Log(canvas.Paint(blush, p.point));
		}
	}
}