using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Es.TexturePaint
{
	/// <summary>
	/// Mesh操作を行うクラス
	/// </summary>
	public class MeshOperator
	{
		#region MeshData

		private Mesh mesh;
		private int[] meshTriangles;
		private Vector3[] meshVertices;
		private Vector2[] meshUV;

		#endregion MeshData

		#region PublicMethod

		public MeshOperator(Mesh mesh)
		{
			if(mesh == null)
				throw new System.ArgumentNullException("mesh");
			this.mesh = mesh;
			meshTriangles = this.mesh.triangles;
			meshVertices = this.mesh.vertices;
			meshUV = this.mesh.uv;
		}

		/// <summary>
		/// ローカル座標をUV座標に変換する
		/// </summary>
		/// <param name="localPoint">Local-Space Point</param>
		/// <param name="matrixMVP">World-View-Projection変換行列</param>
		/// <param name="uv">変換後のUV座標</param>
		/// <returns>変換の成否</returns>
		public bool LocalPointToUV(Vector3 localPoint, Matrix4x4 matrixMVP, out Vector2 uv)
		{
			int index0;
			int index1;
			int index2;
			Vector3 t1;
			Vector3 t2;
			Vector3 t3;
			Vector3 p = localPoint;

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

				//UV座標算出
				var uv1 = meshUV[meshTriangles[index0]];
				var uv2 = meshUV[meshTriangles[index1]];
				var uv3 = meshUV[meshTriangles[index2]];
				uv = Math.TextureCoordinateCalculation(p, t1, uv1, t2, uv2, t3, uv3, matrixMVP);

				return true;
			}
			uv = default(Vector3);
			return false;
		}

		/// <summary>
		/// 指定したLocal-Space上の点に一番近いオブジェクト表面上の点を返す
		/// </summary>
		/// <param name="localPoint">Local-Space Point</param>
		/// <returns>Local-Space Point</returns>
		public Vector3 NearestLocalSurfacePoint(Vector3 localPoint)
		{
			var p = localPoint;

			//頂点の中で一番近いものを含む三角形を取得
			var tris = Math.GetNearestVerticesTriangle(p, meshVertices, meshTriangles);

			//それぞれの三角形空間でそれっぽいp'を計算
			var pds = new List<Vector3>();
			for(int i = 0; i < tris.Length; i += 3)
			{
				var i0 = i;
				var i1 = i + 1;
				var i2 = i + 2;
				pds.Add(Math.TriangleSpaceProjection(p, tris[i0], tris[i1], tris[i2]));
			}

			//pに一番近いp'が求めたかったオブジェクト表面
			var pd = pds.OrderBy(t => Vector3.Distance(p, t)).First();

			return pd;
		}

		#endregion PublicMethod
	}
}