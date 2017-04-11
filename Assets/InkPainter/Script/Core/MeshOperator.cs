using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Es.InkPainter
{
	/// <summary>
	/// A class that manipulates Mesh.
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
		/// Convert local-space point to texture coordinates.
		/// </summary>
		/// <param name="localPoint">Local-Space Point</param>
		/// <param name="matrixMVP">World-View-Projection Transformation matrix.</param>
		/// <param name="uv">UV coordinates after conversion.</param>
		/// <returns>Whether the conversion was successful.</returns>
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

				if(!Math.ExistPointInPlane(p, t1, t2, t3))
					continue;
				if(!Math.ExistPointOnTriangleEdge(p, t1, t2, t3) && !Math.ExistPointInTriangle(p, t1, t2, t3))
					continue;

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
		/// Returns the point on the surface of Mesh closest to the point on the specified local-space.
		/// </summary>
		/// <param name="localPoint">The point of local-space.</param>
		/// <returns>Mesh The point of local-space on the surface.</returns>
		public Vector3 NearestLocalSurfacePoint(Vector3 localPoint)
		{
			var p = localPoint;
			var tris = Math.GetNearestVerticesTriangle(p, meshVertices, meshTriangles);
			var pds = new List<Vector3>();
			for(int i = 0; i < tris.Length; i += 3)
			{
				var i0 = i;
				var i1 = i + 1;
				var i2 = i + 2;
				pds.Add(Math.TriangleSpaceProjection(p, tris[i0], tris[i1], tris[i2]));
			}
			return pds.OrderBy(t => Vector3.Distance(p, t)).First();
		}

		#endregion PublicMethod
	}
}