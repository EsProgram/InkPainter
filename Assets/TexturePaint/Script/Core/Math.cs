using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Es.TexturePaint
{
	public static class Math
	{
		/// <summary>
		/// 誤差許容範囲
		/// </summary>
		private const float TOLERANCE = 1E-2f;

		/// <summary>
		/// 点pが与えられた3点のなす三角形平面上に存在するかを調査する
		/// </summary>
		/// <param name="p">調査点p</param>
		/// <param name="t1">三角形をなす頂点</param>
		/// <param name="t2">三角形をなす頂点</param>
		/// <param name="t3">三角形をなす頂点</param>
		/// <returns>点pが三角形平面上に存在するかどうか</returns>
		public static bool ExistPointInPlane(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
		{
			var v1 = t2 - t1;
			var v2 = t3 - t1;
			var vp = p - t1;

			var nv = Vector3.Cross(v1, v2);
			var val = Vector3.Dot(nv.normalized, vp.normalized);
			if(-TOLERANCE < val && val < TOLERANCE)
				return true;
			return false;
		}

		/// <summary>
		/// 点pが辺(v1,v2)上に存在するかどうかを調査する
		/// </summary>
		/// <param name="p">調査点</param>
		/// <param name="v1">辺をなす頂点</param>
		/// <param name="v2">辺をなす頂点</param>
		/// <returns>点pが辺上に存在しているかどうか</returns>
		public static bool ExistPointOnEdge(Vector3 p, Vector3 v1, Vector3 v2)
		{
			return 1 - TOLERANCE < Vector3.Dot((v2 - p).normalized, (v2 - v1).normalized);
		}

		/// <summary>
		/// 点pが与えられた3点がなす三角形の辺上に存在するかを調査する
		/// </summary>
		/// <param name="p">調査点p</param>
		/// <param name="t1">三角形をなす頂点</param>
		/// <param name="t2">三角形をなす頂点</param>
		/// <param name="t3">三角形をなす頂点</param>
		/// <returns>点pが三角形の辺城に存在するかどうか</returns>
		public static bool ExistPointOnTriangleEdge(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
		{
			if(ExistPointOnEdge(p, t1, t2) || ExistPointOnEdge(p, t2, t3) || ExistPointOnEdge(p, t3, t1))
				return true;
			return false;
		}

		/// <summary>
		/// 点pが与えられた3点がなす三角形内部に存在するかを調査する
		/// 入力(p, t1, t2, t3)各点は同一平面上に存在する必要がある
		/// </summary>
		/// <param name="p">調査点p</param>
		/// <param name="t1">三角形をなす頂点</param>
		/// <param name="t2">三角形をなす頂点</param>
		/// <param name="t3">三角形をなす頂点</param>
		/// <returns>点pが三角形内部に存在するかどうか</returns>
		public static bool ExistPointInTriangle(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
		{
			var a = Vector3.Cross(t1 - t3, p - t1).normalized;
			var b = Vector3.Cross(t2 - t1, p - t2).normalized;
			var c = Vector3.Cross(t3 - t2, p - t3).normalized;

			var d_ab = Vector3.Dot(a, b);
			var d_bc = Vector3.Dot(b, c);

			if(1 - TOLERANCE < d_ab && 1 - TOLERANCE < d_bc)
				return true;
			return false;
		}

		/// <summary>
		/// 点pの三角形内におけるテクスチャ座標を計算する
		/// 入力pは(t1, t2, t3)のなす三角形内部の点である必要がある
		/// </summary>
		/// <param name="p">調査点</param>
		/// <param name="t1">三角形をなす頂点</param>
		/// <param name="t1UV">t1が持つUV座標情報</param>
		/// <param name="t2">三角形をなす頂点</param>
		/// <param name="t2UV">t2が持つUV座標情報</param>
		/// <param name="t3">三角形をなす頂点</param>
		/// <param name="t3UV">t3が持つUV座標情報</param>
		/// <param name="transformMatrix">入力(p, t1, t2, t3)各点をProjection-Spaceに変換する変換行列</param>
		/// <returns>点pのテクスチャ座標</returns>
		public static Vector2 TextureCoordinateCalculation(Vector3 p, Vector3 t1, Vector2 t1UV, Vector3 t2, Vector2 t2UV, Vector3 t3, Vector2 t3UV, Matrix4x4 transformMatrix)
		{
			//各点をProjectionSpaceへの変換
			Vector4 p1_p = transformMatrix * new Vector4(t1.x, t1.y, t1.z, 1);
			Vector4 p2_p = transformMatrix * new Vector4(t2.x, t2.y, t2.z, 1);
			Vector4 p3_p = transformMatrix * new Vector4(t3.x, t3.y, t3.z, 1);
			Vector4 p_p = transformMatrix * new Vector4(p.x, p.y, p.z, 1);
			//通常座標への変換(ProjectionSpace)
			Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
			Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
			Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
			Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;
			//頂点のなす三角形を点pにより3分割し、必要になる面積を計算
			var s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
			var s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
			var s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));
			//面積比からuvを補間
			var u = s1 / s;
			var v = s2 / s;
			var w = 1 / ((1 - u - v) * 1 / p1_p.w + u * 1 / p2_p.w + v * 1 / p3_p.w);
			var uv = w * ((1 - u - v) * t1UV / p1_p.w + u * t2UV / p2_p.w + v * t3UV / p3_p.w);

			return uv;
		}

		/// <summary>
		/// 与えられた頂点/三角形リストから点pに一番近い頂点を持つ三角形の頂点リストを返す
		/// </summary>
		/// <param name="p">調査点</param>
		/// <param name="vertices">頂点リスト</param>
		/// <param name="triangles">頂点の三角形リスト</param>
		/// <returns></returns>
		public static Vector3[] GetNearestVerticesTriangle(Vector3 p, Vector3[] vertices, int[] triangles)
		{
			List<Vector3> ret = new List<Vector3>();

			int nearestIndex = triangles[0];
			float nearestDistance = Vector3.Distance(vertices[nearestIndex], p);

			for(int i = 0; i < vertices.Length; ++i)
			{
				float distance = Vector3.Distance(vertices[i], p);
				if(distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestIndex = i;
				}
			}

			for(int i = 0; i < triangles.Length; ++i)
			{
				if(triangles[i] == nearestIndex)
				{
					var m = i % 3;
					int i0 = i, i1 = 0, i2 = 0;
					switch(m)
					{
						case 0:
							i1 = i + 1;
							i2 = i + 2;
							break;

						case 1:
							i1 = i - 1;
							i2 = i + 1;
							break;

						case 2:
							i1 = i - 1;
							i2 = i - 2;
							break;

						default:
							break;
					}
					ret.Add(vertices[triangles[i0]]);
					ret.Add(vertices[triangles[i1]]);
					ret.Add(vertices[triangles[i2]]);
				}
			}
			return ret.ToArray();
		}

		/// <summary>
		/// 点pを三角形空間内に投影した点を返す
		/// </summary>
		/// <param name="p">投影する点</param>
		/// <param name="t1">三角形頂点</param>
		/// <param name="t2">三角形頂点</param>
		/// <param name="t3">三角形頂点</param>
		/// <returns>投影後の三角形空間上の点</returns>
		public static Vector3 TriangleSpaceProjection(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
		{
			var g = (t1 + t2 + t3) / 3;
			var pa = t1 - p;
			var pb = t2 - p;
			var pc = t3 - p;
			var ga = t1 - g;
			var gb = t2 - g;
			var gc = t3 - g;

			var _pa_ = pa.magnitude;
			var _pb_ = pb.magnitude;
			var _pc_ = pc.magnitude;

			var lmin = Mathf.Min(Mathf.Min(_pa_, _pb_), _pc_);

			Func<float, float, float> k = (t, u) => (t - lmin + u - lmin) / 2;

			var A = k(_pb_, _pc_);
			var B = k(_pc_, _pa_);
			var C = k(_pa_, _pb_);
			var pd = g + (ga * A + gb * B + gc * C);
			return pd;
		}
	}
}