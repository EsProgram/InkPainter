using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Es.TexturePaint
{
	/// <summary>
	/// テクスチャペイントを行うキャンバス
	/// マテリアル単位で設定する
	/// </summary>
	[RequireComponent(typeof(Renderer))]
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(MeshFilter))]
	[DisallowMultipleComponent]
	public class DynamicCanvas : MonoBehaviour
	{
		[Serializable]
		private class PaintSet
		{
			/// <summary>
			/// ペイントを適用するマテリアル
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Material material;

			[SerializeField, Tooltip("メインテクスチャのプロパティ名")]
			public string mainTextureName = "_MainTex";

			[SerializeField, Tooltip("バンプマップテクスチャのプロパティ名")]
			public string bumpTextureName = "_BumpMap";

			[SerializeField, Tooltip("バンプマップペイントをするか(マテリアルにバンプマップが設定されている必要があります)")]
			public bool useBumpPaint = false;

			/// <summary>
			/// 最初にマテリアルにセットされているメインテクスチャ
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Texture mainTexture;

			/// <summary>
			/// 最初にマテリアルにセットされているバンプマップ
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintMainTexture;

			/// <summary>
			/// メインテクスチャをコピーしたペイント用RenderTexture
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Texture bumpTexture;

			/// <summary>
			/// バンプマップをコピーしたペイント用RenderTexture
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintBumpTexture;

			#region ShaderPropertyID

			[HideInInspector]
			[NonSerialized]
			public int mainTexturePropertyID;

			[HideInInspector]
			[NonSerialized]
			public int bumpTexturePropertyID;

			#endregion ShaderPropertyID
		}

		private const int DEFAULT_TEXTURE_SIZE = 256;

		#region SerializedProperties

		[SerializeField]
		private List<PaintSet> paintSet = null;

		[SerializeField, HideInInspector, Tooltip("テクスチャペイント用マテリアル")]
		private Material paintMaterial = null;

		[SerializeField, HideInInspector, Tooltip("ブラシバンプマップ用マテリアル")]
		private Material paintBumpMaterial = null;

		#endregion SerializedProperties

		#region ShaderPropertyID

		private int paintUVPropertyID;
		private int blushTexturePropertyID;
		private int blushScalePropertyID;
		private int blushColorPropertyID;
		private int blushBumpTexturePropertyID;
		private int blushBumpBlendPropertyID;

		#endregion ShaderPropertyID

		#region ShaderKeywords

		private const string COLOR_BLEND_USE_CONTROL = "TEXTURE_PAINT_COLOR_BLEND_USE_CONTROL";
		private const string COLOR_BLEND_USE_BLUSH = "TEXTURE_PAINT_COLOR_BLEND_USE_BLUSH";
		private const string COLOR_BLEND_NEUTRAL = "TEXTURE_PAINT_COLOR_BLEND_NEUTRAL";

		private const string BUMP_BLEND_USE_BLUSH = "TEXTURE_PAINT_BUMP_BLEND_USE_BLUSH";
		private const string BUMP_BLEND_MIN = "TEXTURE_PAINT_BUMP_BLEND_MIN";
		private const string BUMP_BLEND_MAX = "TEXTURE_PAINT_BUMP_BLEND_MAX";

		#endregion ShaderKeywords

		private MeshFilter meshFilter;
		private Mesh mesh;
		private int[] meshTriangles;
		private Vector3[] meshVertices;
		private Vector2[] meshUV;

		#region UnityEventMethod

		public void Awake()
		{
			InitPropertyID();
			SetMaterial();
			SetTexture();
			SetRenderTexture();
			MeshDataCache();
		}

		public void OnDestroy()
		{
			Debug.Log("DynamicCanvasを破棄しました");
			ReleaseRenderTexture();
		}

		#endregion UnityEventMethod

		/// <summary>
		/// メッシュから取得できるデータをキャッシュする。
		/// </summary>
		private void MeshDataCache()
		{
			meshFilter = GetComponent<MeshFilter>();
			mesh = meshFilter.sharedMesh;
			meshTriangles = mesh.triangles;
			meshVertices = mesh.vertices;
			meshUV = mesh.uv;
		}

		/// <summary>
		/// シェーダーのプロパティIDを初期化する
		/// </summary>
		private void InitPropertyID()
		{
			foreach(var p in paintSet)
			{
				p.mainTexturePropertyID = Shader.PropertyToID(p.mainTextureName);
				p.bumpTexturePropertyID = Shader.PropertyToID(p.bumpTextureName);
			}
			paintUVPropertyID = Shader.PropertyToID("_PaintUV");
			blushTexturePropertyID = Shader.PropertyToID("_Blush");
			blushScalePropertyID = Shader.PropertyToID("_BlushScale");
			blushColorPropertyID = Shader.PropertyToID("_ControlColor");
			blushBumpTexturePropertyID = Shader.PropertyToID("_BlushBump");
			blushBumpBlendPropertyID = Shader.PropertyToID("_BumpBlend");
		}

		/// <summary>
		/// マテリアルを取得しセットする
		/// </summary>
		private void SetMaterial()
		{
			var m = GetComponent<Renderer>().materials;
			for(int i = 0; i < m.Length; ++i)
			{
				paintSet[i].material = m[i];
			}
		}

		/// <summary>
		/// テクスチャを取得しセットする
		/// </summary>
		private void SetTexture()
		{
			foreach(var p in paintSet)
			{
				p.mainTexture = p.material.GetTexture(p.mainTexturePropertyID);
				if(p.useBumpPaint)
					p.bumpTexture = p.material.GetTexture(p.bumpTexturePropertyID);
			}
		}

		/// <summary>
		/// RenderTextureを生成しマテリアルにセットする
		/// </summary>
		private void SetRenderTexture()
		{
			foreach(var p in paintSet)
			{
				//MainTextureが設定されていない場合は白テクスチャ
				if(p.mainTexture == null)
					p.mainTexture = new Texture2D(DEFAULT_TEXTURE_SIZE, DEFAULT_TEXTURE_SIZE, TextureFormat.RGBA32, false);
				//DynamicPaint用RenderTextureの生成
				p.paintMainTexture = new RenderTexture(p.mainTexture.width, p.mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				//メインテクスチャのコピー
				Graphics.Blit(p.mainTexture, p.paintMainTexture);
				//マテリアルのテクスチャをRenderTextureに変更
				p.material.SetTexture(p.mainTexturePropertyID, p.paintMainTexture);
				if(p.useBumpPaint)
				{
					//BumpTextureが設定されている場合
					if(p.bumpTexture != null)
					{
						//法線マップテクスチャの生成
						p.paintBumpTexture = new RenderTexture(p.mainTexture.width, p.mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
						//法線マップのコピー
						Graphics.Blit(p.bumpTexture, p.paintBumpTexture);
						//マテリアルの法線マップテクスチャをRenderTextureに変更
						p.material.SetTexture(p.bumpTexturePropertyID, p.paintBumpTexture);
					}
					else
						Debug.LogWarning("バンプマップペイントを利用するにはマテリアルにバンプマップテクスチャが設定されている必要があります");
				}
			}
		}

		/// <summary>
		/// RenderTextureリリース処理
		/// </summary>
		private void ReleaseRenderTexture()
		{
			foreach(var p in paintSet)
			{
				if(RenderTexture.active != p.paintMainTexture && p.paintMainTexture != null && p.paintMainTexture.IsCreated())
					p.paintMainTexture.Release();
				if(RenderTexture.active != p.paintBumpTexture && p.paintBumpTexture != null && p.paintBumpTexture.IsCreated())
					p.paintBumpTexture.Release();
			}
		}

		/// <summary>
		/// ペイントに必要なデータをシェーダーにセットする
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		private void SetPaintData(PaintBlush blush, Vector2 uv)
		{
			paintMaterial.SetVector(paintUVPropertyID, uv);
			paintMaterial.SetTexture(blushTexturePropertyID, blush.BlushTexture);
			paintMaterial.SetFloat(blushScalePropertyID, blush.Scale);
			paintMaterial.SetVector(blushColorPropertyID, blush.Color);

			foreach(var key in paintMaterial.shaderKeywords)
				paintMaterial.DisableKeyword(key);
			switch(blush.ColorBlending)
			{
				case PaintBlush.ColorBlendType.UseColor:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_CONTROL);
					break;

				case PaintBlush.ColorBlendType.UseBlush:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_BLUSH);
					break;

				case PaintBlush.ColorBlendType.Neutral:
					paintMaterial.EnableKeyword(COLOR_BLEND_NEUTRAL);
					break;

				default:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_CONTROL);
					break;
			}
		}

		/// <summary>
		/// バンプマップペイントに必要なデータをシェーダーにセットする
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		private void SetPaintBumpData(PaintBlush blush, Vector2 uv)
		{
			paintBumpMaterial.SetVector(paintUVPropertyID, uv);
			paintBumpMaterial.SetTexture(blushTexturePropertyID, blush.BlushTexture);
			paintBumpMaterial.SetTexture(blushBumpTexturePropertyID, blush.BlushBumpTexture);
			paintBumpMaterial.SetFloat(blushScalePropertyID, blush.Scale);
			paintBumpMaterial.SetFloat(blushBumpBlendPropertyID, blush.BumpBlend);

			foreach(var key in paintBumpMaterial.shaderKeywords)
				paintBumpMaterial.DisableKeyword(key);
			switch(blush.BumpBlending)
			{
				case PaintBlush.BumpBlendType.UseBlush:
					paintBumpMaterial.EnableKeyword(BUMP_BLEND_USE_BLUSH);
					break;

				case PaintBlush.BumpBlendType.Min:
					paintBumpMaterial.EnableKeyword(BUMP_BLEND_MIN);
					break;

				case PaintBlush.BumpBlendType.Max:
					paintBumpMaterial.EnableKeyword(BUMP_BLEND_MAX);
					break;

				default:
					paintBumpMaterial.EnableKeyword(BUMP_BLEND_USE_BLUSH);
					break;
			}
		}

		/// <summary>
		/// 直接UV座標を指定したペイント処理を行う
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		/// <returns>ペイントの成否</returns>
		public bool PaintUVDirect(PaintBlush blush, Vector2 uv)
		{
			foreach(var p in paintSet)
			{
				RenderTexture buf = RenderTexture.GetTemporary(p.paintMainTexture.width, p.paintMainTexture.height);
				if(buf == null)
				{
					Debug.LogError("RenderTextureの生成に失敗しました");
					return false;
				}
				//メインテクスチャへのペイント
				if(blush.BlushTexture != null && p.paintMainTexture != null && p.paintMainTexture.IsCreated())
				{
					SetPaintData(blush, uv);
					Graphics.Blit(p.paintMainTexture, buf, paintMaterial);
					Graphics.Blit(buf, p.paintMainTexture);
				}

				//バンプマップへのペイント
				if(p.useBumpPaint && blush.BlushBumpTexture != null && p.paintBumpTexture != null && p.paintBumpTexture.IsCreated())
				{
					SetPaintBumpData(blush, uv);

					Graphics.Blit(p.paintBumpTexture, buf, paintBumpMaterial);
					Graphics.Blit(buf, p.paintBumpTexture);
				}
				RenderTexture.ReleaseTemporary(buf);
			}
			return true;
		}

		/// <summary>
		/// 与えられたworldPosに近いMeshSurface上の点に対してペイント処理を行う
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="worldPos">近似点</param>
		/// <param name="renderCamera">レンダリングに利用するカメラ</param>
		/// <returns></returns>
		public bool PaintNearestTriangleSurface(PaintBlush blush, Vector3 worldPos, Camera renderCamera = null)
		{
			var p = transform.worldToLocalMatrix.MultiplyPoint(worldPos);

			//頂点の中で一番近いものを含む三角形を取得
			var tris = Utility.Math.GetNearestVerticesTriangle(p, meshVertices, meshTriangles);

			//それぞれの三角形空間でそれっぽいp'を計算
			var pds = new List<Vector3>();
			for(int i = 0; i < tris.Length; i += 3)
			{
				var i0 = i;
				var i1 = i + 1;
				var i2 = i + 2;
				pds.Add(Utility.Math.TriangleSpaceProjection(p, tris[i0], tris[i1], tris[i2]));
			}

			//HACK:p'が三角形内部にない場合は一番近い頂点位置をp'に設定した方がいい？

			//pに一番近いp'が求めたかったオブジェクト表面
			var pd = pds.OrderBy(t => Vector3.Distance(p, t)).First();

			return Paint(blush, transform.localToWorldMatrix.MultiplyPoint(pd), renderCamera);
		}

		/// <summary>
		/// ペイント処理を行う
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="worldPos">
		/// キャンバス上の塗る点(World-Space)
		/// メッシュが構成する形状のサーフェスの上の点
		/// </param>
		/// <param name="renderCamera">レンダリングに利用するカメラ</param>
		/// <returns>ペイント成否</returns>
		public bool Paint(PaintBlush blush, Vector3 worldPos, Camera renderCamera = null)
		{
			if(renderCamera == null)
				renderCamera = Camera.main;

			int index0;
			int index1;
			int index2;
			Vector3 t1;
			Vector3 t2;
			Vector3 t3;
			Vector3 p = transform.InverseTransformPoint(worldPos);

			for(var i = 0; i < meshTriangles.Length; i += 3)
			{
				index0 = i + 0;
				index1 = i + 1;
				index2 = i + 2;

				t1 = meshVertices[meshTriangles[index0]];
				t2 = meshVertices[meshTriangles[index1]];
				t3 = meshVertices[meshTriangles[index2]];

				//平面上に存在しない
				if(!Utility.Math.ExistPointInPlane(p, t1, t2, t3))
					continue;
				//三角形の辺または内部に存在しない
				if(!Utility.Math.ExistPointOnTriangleEdge(p, t1, t2, t3) && !Utility.Math.ExistPointInTriangle(p, t1, t2, t3))
					continue;

				//UV座標算出
				var uv1 = meshUV[meshTriangles[index0]];
				var uv2 = meshUV[meshTriangles[index1]];
				var uv3 = meshUV[meshTriangles[index2]];
				Matrix4x4 mvp = renderCamera.projectionMatrix * renderCamera.worldToCameraMatrix * transform.localToWorldMatrix;
				var uv = Utility.Math.TextureCoordinateCalculation(p, t1, uv1, t2, uv2, t3, uv3, mvp);

				return PaintUVDirect(blush, uv);
			}
			return false;
		}

		/// <summary>
		/// ペイント処理を行う
		/// CanvasにはMeshColliderが設定されている必要があります
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="hitInfo">RaycastのHit情報</param>
		/// <returns>ペイントの成否</returns>
		public bool Paint(PaintBlush blush, RaycastHit hitInfo)
		{
			if(hitInfo.collider != null && hitInfo.collider.gameObject == gameObject)
			{
				#region ErrorCheck

				if(blush == null)
				{
					Debug.LogError("ブラシが設定されていません");
					return false;
				}

				#endregion ErrorCheck

				//MeshColliderが設定されていない場合はヒット位置でペイントを行う
				if(!(GetComponent<Collider>() is MeshCollider))
				{
					Debug.LogWarning("MeshColliderが設定されていないキャンバスにRayCastを利用したPaintを行うと予期せぬ動作をする場合があります");
					//頂点のTriangleから一番近いサーフェス上の点を算出してPaintに渡すように
					return PaintNearestTriangleSurface(blush, hitInfo.point);
				}
				return PaintUVDirect(blush, hitInfo.textureCoord);
			}
			return false;
		}

		/// <summary>
		/// ペイントをリセットする
		/// </summary>
		public void ResetPaint()
		{
			ReleaseRenderTexture();
			SetRenderTexture();
		}

#if UNITY_EDITOR

		[CustomEditor(typeof(DynamicCanvas))]
		[CanEditMultipleObjects]
		private class DynamicCanvasInspectorExtension : Editor
		{
			private Renderer renderer;
			private Material[] materials;
			private List<bool> foldOut;

			public override void OnInspectorGUI()
			{
				var instance = target as DynamicCanvas;
				if(instance.paintSet == null)
					instance.paintSet = new List<PaintSet>();

				if(renderer == null)
					renderer = instance.GetComponent<Renderer>();
				if(materials == null)
					materials = renderer.sharedMaterials;
				if(foldOut == null)
					foldOut = new List<bool>();

				if(instance.paintSet.Count < materials.Length)
				{
					for(int i = instance.paintSet.Count; i < materials.Length; ++i)
						instance.paintSet.Add(new PaintSet
						{
							mainTextureName = "_MainTex",
							bumpTextureName = "_BumpMap",
							useBumpPaint = false
						});
					foldOut.Clear();
				}

				if(instance.paintSet.Count > materials.Length)
				{
					instance.paintSet.RemoveRange(materials.Length, instance.paintSet.Count - materials.Length);
					foldOut.Clear();
				}

				if(foldOut.Count < instance.paintSet.Count)
				{
					for(int i = foldOut.Count; i < instance.paintSet.Count; ++i)
						foldOut.Add(true);
				}

				for(int i = 0; i < instance.paintSet.Count; ++i)
				{
					if(foldOut[i] = EditorGUILayout.Foldout(foldOut[i], string.Format("Material \"{0}\"", materials[i].name)))
					{
						EditorGUI.indentLevel = 1;
						instance.paintSet[i].mainTextureName = EditorGUILayout.TextField("MainTexture Property Name", instance.paintSet[i].mainTextureName);
						instance.paintSet[i].bumpTextureName = EditorGUILayout.TextField("BumpMap Property Name", instance.paintSet[i].bumpTextureName);
						instance.paintSet[i].useBumpPaint = EditorGUILayout.Toggle("Use BumpMap Paint", instance.paintSet[i].useBumpPaint);
						EditorGUI.indentLevel = 0;
					}
				}
			}
		}

#endif
	}
}