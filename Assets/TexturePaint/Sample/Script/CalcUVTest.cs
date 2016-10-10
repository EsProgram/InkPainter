using Es.TexturePaint;
using Es.Utility;
using System.Collections;
using UnityEngine;

/// <summary>
/// サーフェス上の点pからuvを算出するののテスト
/// </summary>
public class CalcUVTest : MonoBehaviour
{
	[SerializeField]
	private bool click = true;

	[SerializeField]
	private PaintBlush blush;

	private MeshFilter meshFilter;
	private Mesh mesh;

	private int index0;
	private int index1;
	private int index2;

	private Vector3 t1;
	private Vector3 t2;
	private Vector3 t3;
	private Vector3 p;

	private Vector3 v1;
	private Vector3 v2;
	private Vector3 vp;

	private Vector3 nv;

	private Vector3 a;
	private Vector3 b;
	private Vector3 c;

	private float d_ab;
	private float d_bc;

	private void Update()
	{
		if((click && Input.GetMouseButtonDown(0)) || (!click && Input.GetMouseButton(0)))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo))
			{
				meshFilter = hitInfo.transform.GetComponent<MeshFilter>();
				mesh = meshFilter.sharedMesh;
				var meshTriangles = mesh.triangles;
				var meshVertices = mesh.vertices;
				var meshUV = mesh.uv;
				p = hitInfo.transform.InverseTransformPoint(hitInfo.point);

				for(var i = 0; i < meshTriangles.Length; i += 3)
				{
					index0 = i + 0;
					index1 = i + 1;
					index2 = i + 2;

					t1 = meshVertices[meshTriangles[index0]];
					t2 = meshVertices[meshTriangles[index1]];
					t3 = meshVertices[meshTriangles[index2]];

					//平面上に存在しない
					if(!Math.ExistPointInPlane(p, t1, t2, t3))
						continue;
					//三角形の辺または内部に存在しない
					if(!Math.ExistPointOnTriangleEdge(p, t1, t2, t3) && !Math.ExistPointInTriangle(p, t1, t2, t3))
						continue;

					var uv1 = meshUV[meshTriangles[index0]];
					var uv2 = meshUV[meshTriangles[index1]];
					var uv3 = meshUV[meshTriangles[index2]];
					Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * transform.localToWorldMatrix;
					var uv = Math.TextureCoordinateCalculation(p, t1, uv1, t2, uv2, t3, uv3, mvp);

					//値のチェック
					var strUV1 = string.Format("({0} , {1})", uv.x, uv.y);
					var strUV2 = string.Format("({0} , {1})", hitInfo.textureCoord.x, hitInfo.textureCoord.y);

					//成功
					if(strUV1.Substring(0, 3) == strUV2.Substring(0, 3))
						Debug.Log(strUV1 + " : " + strUV2);
					//値が結構違う
					else
						Debug.LogWarning(strUV1 + " : " + strUV2);
					hitInfo.transform.GetComponent<DynamicCanvas>().Paint(blush, hitInfo.point);
					return;
				}
				//Raycastではヒットしたのにスルーされた！！
				Debug.LogError("Not Found!!");
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