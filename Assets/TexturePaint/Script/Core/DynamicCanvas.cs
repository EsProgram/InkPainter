using Es.Utility;
using System.Linq;
using UnityEngine;

namespace Es.TexturePaint
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshCollider))]
	[RequireComponent(typeof(MeshFilter))]
	[DisallowMultipleComponent]
	public class DynamicCanvas : MonoBehaviour
	{
		private const int DEFAULT_TEXTURE_SIZE = 256;

		#region SerializedProperties

		[SerializeField, Tooltip("メインテクスチャのプロパティ名")]
		private string mainTextureName = "_MainTex";

		[SerializeField, Tooltip("バンプマップテクスチャのプロパティ名")]
		private string bumpTextureName = "_BumpMap";

		[SerializeField]
		private bool useBumpPaint = true;

		[SerializeField, HideInInspector, Tooltip("テクスチャペイント用マテリアル")]
		private Material paintMaterial = null;

		[SerializeField, HideInInspector, Tooltip("ブラシバンプマップ用マテリアル")]
		private Material paintBumpMaterial = null;

		#endregion SerializedProperties

		#region ShaderPropertyID

		private int mainTexturePropertyID;
		private int bumpTexturePropertyID;
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

		/// <summary>
		/// 最初にマテリアルにセットされているメインテクスチャ
		/// </summary>
		private Texture mainTexture;

		/// <summary>
		/// 最初にマテリアルにセットされているバンプマップ
		/// </summary>
		private Texture bumpTexture;

		/// <summary>
		/// メインテクスチャをコピーしたペイント用RenderTexture
		/// </summary>
		private RenderTexture paintTexture;

		/// <summary>
		/// バンプマップをコピーしたペイント用RenderTexture
		/// </summary>
		private RenderTexture paintBumpTexture;

		private Material material;

		private MeshFilter meshFilter;
		private Mesh mesh;
		private int[] meshTriangles;
		private Vector3[] meshVertices;
		private Vector2[] meshUV;

		#region UnityEventMethod

		public void Awake()
		{
			InitPropertyID();
			ColliderCheck();

			var meshRenderer = GetComponent<MeshRenderer>();
			material = meshRenderer.material;
			mainTexture = material.GetTexture(mainTexturePropertyID);
			bumpTexture = material.GetTexture(bumpTexturePropertyID);

			SetRenderTexture();

			meshFilter = GetComponent<MeshFilter>();
			mesh = meshFilter.sharedMesh;
			meshTriangles = mesh.triangles;
			meshVertices = mesh.vertices;
			meshUV = mesh.uv;
		}

		public void OnDestroy()
		{
			Debug.Log("DynamicCanvasを破棄しました");
			ReleaseRenderTexture();
		}

		#endregion UnityEventMethod

		/// <summary>
		/// シェーダーのプロパティIDを初期化する
		/// </summary>
		private void InitPropertyID()
		{
			mainTexturePropertyID = Shader.PropertyToID(mainTextureName);
			bumpTexturePropertyID = Shader.PropertyToID(bumpTextureName);

			paintUVPropertyID = Shader.PropertyToID("_PaintUV");
			blushTexturePropertyID = Shader.PropertyToID("_Blush");
			blushScalePropertyID = Shader.PropertyToID("_BlushScale");
			blushColorPropertyID = Shader.PropertyToID("_ControlColor");
			blushBumpTexturePropertyID = Shader.PropertyToID("_BlushBump");
			blushBumpBlendPropertyID = Shader.PropertyToID("_BumpBlend");
		}

		/// <summary>
		/// コライダーが正しく設定されているかどうかをチェックする
		/// コライダーはMeshColliderがただ一つアタッチされている必要がある
		/// </summary>
		private void ColliderCheck()
		{
			var colliders = GetComponents<Collider>();
			if(colliders.Length != 1 || !(colliders.First() is MeshCollider))
			{
				Debug.LogWarning("ColliderはMeshColliderのみが設定されている必要があります");
				Destroy(this);
			}
		}

		/// <summary>
		/// RenderTextureを生成しマテリアルにセットする
		/// </summary>
		private void SetRenderTexture()
		{
			//MainTextureが設定されていない場合は白テクスチャ
			if(mainTexture == null)
				mainTexture = new Texture2D(DEFAULT_TEXTURE_SIZE, DEFAULT_TEXTURE_SIZE, TextureFormat.RGBA32, false);
			//DynamicPaint用RenderTextureの生成
			paintTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			//メインテクスチャのコピー
			Graphics.Blit(mainTexture, paintTexture);
			//マテリアルのテクスチャをRenderTextureに変更
			material.SetTexture(mainTexturePropertyID, paintTexture);
			if(useBumpPaint)
			{
				//BumpTextureが設定されている場合
				if(bumpTexture != null)
				{
					//法線マップテクスチャの生成
					paintBumpTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
					//法線マップのコピー
					Graphics.Blit(bumpTexture, paintBumpTexture);
					//マテリアルの法線マップテクスチャをRenderTextureに変更
					material.SetTexture(bumpTexturePropertyID, paintBumpTexture);
				}
				else
					Debug.LogWarning("バンプマップペイントを利用するにはマテリアルにバンプマップテクスチャが設定されている必要があります");
			}
		}

		/// <summary>
		/// RenderTextureリリース処理
		/// </summary>
		private void ReleaseRenderTexture()
		{
			if(RenderTexture.active != paintTexture && paintTexture != null && paintTexture.IsCreated())
				paintTexture.Release();
			if(RenderTexture.active != paintBumpTexture && paintBumpTexture != null && paintBumpTexture.IsCreated())
				paintBumpTexture.Release();
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
		/// ペイント処理(UVを直接指定して書き込み)
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		/// <returns>ペイントの成否</returns>
		public bool PaintUVDirect(PaintBlush blush, Vector2 uv)
		{
			RenderTexture buf = RenderTexture.GetTemporary(paintTexture.width, paintTexture.height);
			if(buf == null)
			{
				Debug.LogError("RenderTextureの生成に失敗しました");
				return false;
			}
			//メインテクスチャへのペイント
			if(blush.BlushTexture != null && paintTexture != null && paintTexture.IsCreated())
			{
				SetPaintData(blush, uv);
				Graphics.Blit(paintTexture, buf, paintMaterial);
				Graphics.Blit(buf, paintTexture);
			}

			//バンプマップへのペイント
			if(useBumpPaint && blush.BlushBumpTexture != null && paintBumpTexture != null && paintBumpTexture.IsCreated())
			{
				SetPaintBumpData(blush, uv);

				Graphics.Blit(paintBumpTexture, buf, paintBumpMaterial);
				Graphics.Blit(buf, paintBumpTexture);
			}
			RenderTexture.ReleaseTemporary(buf);
			return true;
		}

		/// <summary>
		/// ペイント処理
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="worldPos">キャンバス上の塗る店(World-Space)</param>
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
				if(!Math.ExistPointInPlane(p, t1, t2, t3))
					continue;
				//三角形の辺または内部に存在しない
				if(!Math.ExistPointOnTriangleEdge(p, t1, t2, t3) && !Math.ExistPointInTriangle(p, t1, t2, t3))
					continue;

				var uv1 = meshUV[meshTriangles[index0]];
				var uv2 = meshUV[meshTriangles[index1]];
				var uv3 = meshUV[meshTriangles[index2]];
				Matrix4x4 mvp = renderCamera.projectionMatrix * renderCamera.worldToCameraMatrix * transform.localToWorldMatrix;
				var uv = Math.TextureCoordinateCalculation(p, t1, uv1, t2, uv2, t3, uv3, mvp);

				return PaintUVDirect(blush, uv);
			}
			return false;
		}

		/// <summary>
		/// ペイント処理
		/// </summary>
		/// <param name="blush">ブラシ</param>
		/// <param name="hitInfo">RaycastのHit情報</param>
		/// <returns>ペイントの成否</returns>
		public bool Paint(PaintBlush blush, RaycastHit hitInfo)
		{
			if(hitInfo.collider != null && hitInfo.collider.gameObject == gameObject)
			{
				var uv = hitInfo.textureCoord;

				#region ErrorCheck

				if(blush == null)
				{
					Debug.LogError("ブラシが設定されていません");
					return false;
				}

				#endregion ErrorCheck

				return PaintUVDirect(blush, uv);
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
	}
}