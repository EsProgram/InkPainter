using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

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

			[SerializeField, Tooltip("法線マップテクスチャのプロパティ名")]
			public string normalTextureName = "_BumpMap";

			[SerializeField, Tooltip("ハイトマップテクスチャのプロパティ名")]
			public string heightTextureName = "_ParallaxMap";

			[SerializeField, Tooltip("ペイントをするか")]
			public bool useMainPaint = true;

			[SerializeField, Tooltip("法線マップペイントをするか(マテリアルに法線マップが設定されている必要があります)")]
			public bool useNormalPaint = false;

			[SerializeField, Tooltip("ハイトマップペイントをするか(マテリアルにハイトマップが設定されている必要があります)")]
			public bool useHeightPaint = false;

			/// <summary>
			/// 最初にマテリアルにセットされているメインテクスチャ
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Texture mainTexture;

			/// <summary>
			/// メインテクスチャをコピーしたペイント用RenderTexture
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintMainTexture;

			/// <summary>
			/// 最初にマテリアルにセットされている法線マップ
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Texture normalTexture;

			/// <summary>
			/// 法線マップをコピーしたペイント用RenderTexture
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintNormalTexture;

			/// <summary>
			/// 最初にマテリアルにセットされているハイトマップ
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public Texture heightTexture;

			/// <summary>
			/// ハイトマップをコピーしたペイント用RenderTexture
			/// </summary>
			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintHeightTexture;

			#region ShaderPropertyID

			[HideInInspector]
			[NonSerialized]
			public int mainTexturePropertyID;

			[HideInInspector]
			[NonSerialized]
			public int normalTexturePropertyID;

			[HideInInspector]
			[NonSerialized]
			public int heightTexturePropertyID;

			#endregion ShaderPropertyID
		}

		private const int DEFAULT_TEXTURE_SIZE = 256;
		private static Material paintMaterial = null;
		private static Material paintNormalMaterial = null;
		private static Material paintHeightMaterial = null;

		#region SerializedField

		[SerializeField]
		private List<PaintSet> paintSet = null;

		#endregion SerializedField

		#region ShaderPropertyID

		private int paintUVPropertyID;

		private int brushTexturePropertyID;
		private int brushScalePropertyID;
		private int brushColorPropertyID;
		private int brushNormalTexturePropertyID;
		private int brushNormalBlendPropertyID;
		private int brushHeightTexturePropertyID;
		private int brushHeightBlendPropertyID;
		private int brushHeightColorPropertyID;

		#endregion ShaderPropertyID

		#region ShaderKeywords

		private const string COLOR_BLEND_USE_CONTROL = "TEXTURE_PAINT_COLOR_BLEND_USE_CONTROL";
		private const string COLOR_BLEND_USE_BRUSH = "TEXTURE_PAINT_COLOR_BLEND_USE_BRUSH";
		private const string COLOR_BLEND_NEUTRAL = "TEXTURE_PAINT_COLOR_BLEND_NEUTRAL";
		private const string COLOR_BLEND_ALPHA_ONLY = "TEXTURE_PAINT_COLOR_BLEND_ALPHA_ONLY";

		private const string NORMAL_BLEND_USE_BRUSH = "TEXTURE_PAINT_NORMAL_BLEND_USE_BRUSH";
		private const string NORMAL_BLEND_ADD = "TEXTURE_PAINT_NORMAL_BLEND_ADD";
		private const string NORMAL_BLEND_SUB = "TEXTURE_PAINT_NORMAL_BLEND_SUB";
		private const string NORMAL_BLEND_MIN = "TEXTURE_PAINT_NORMAL_BLEND_MIN";
		private const string NORMAL_BLEND_MAX = "TEXTURE_PAINT_NORMAL_BLEND_MAX";

		private const string HEIGHT_BLEND_USE_BRUSH = "TEXTURE_PAINT_HEIGHT_BLEND_USE_BRUSH";
		private const string HEIGHT_BLEND_ADD = "TEXTURE_PAINT_HEIGHT_BLEND_ADD";
		private const string HEIGHT_BLEND_SUB = "TEXTURE_PAINT_HEIGHT_BLEND_SUB";
		private const string HEIGHT_BLEND_MIN = "TEXTURE_PAINT_HEIGHT_BLEND_MIN";
		private const string HEIGHT_BLEND_MAX = "TEXTURE_PAINT_HEIGHT_BLEND_MAX";
		private const string HEIGHT_BLEND_COLOR_RGB_HEIGHT_A = "TEXTURE_PAINT_HEIGHT_BLEND_COLOR_RGB_HEIGHT_A";

		#endregion ShaderKeywords

		#region MeshData

		private MeshOperator meshOperator;
		public MeshOperator MeshOperator { get { return meshOperator; } }

		#endregion MeshData

		#region UnityEventMethod

		private void Awake()
		{
			InitPropertyID();
			SetMaterial();
			SetTexture();
			SetRenderTexture();
			MeshDataCache();
		}

		private void OnDestroy()
		{
			Debug.Log("DynamicCanvasを破棄しました");
			ReleaseRenderTexture();
		}

		#endregion UnityEventMethod

		#region PrivateMethod

		/// <summary>
		/// メッシュから取得できるデータをキャッシュする
		/// </summary>
		private void MeshDataCache()
		{
			meshOperator = new MeshOperator(GetComponent<MeshFilter>().sharedMesh);
		}

		/// <summary>
		/// シェーダーのプロパティIDを初期化する
		/// </summary>
		private void InitPropertyID()
		{
			foreach(var p in paintSet)
			{
				p.mainTexturePropertyID = Shader.PropertyToID(p.mainTextureName);
				p.normalTexturePropertyID = Shader.PropertyToID(p.normalTextureName);
				p.heightTexturePropertyID = Shader.PropertyToID(p.heightTextureName);
			}
			paintUVPropertyID = Shader.PropertyToID("_PaintUV");
			brushTexturePropertyID = Shader.PropertyToID("_Brush");
			brushScalePropertyID = Shader.PropertyToID("_BrushScale");
			brushColorPropertyID = Shader.PropertyToID("_ControlColor");
			brushNormalTexturePropertyID = Shader.PropertyToID("_BrushNormal");
			brushNormalBlendPropertyID = Shader.PropertyToID("_NormalBlend");
			brushHeightTexturePropertyID = Shader.PropertyToID("_BrushHeight");
			brushHeightBlendPropertyID = Shader.PropertyToID("_HeightBlend");
			brushHeightColorPropertyID = Shader.PropertyToID("_Color");
		}

		/// <summary>
		/// マテリアルを取得しセットする
		/// </summary>
		private void SetMaterial()
		{
			if(paintMaterial == null)
				paintMaterial = new Material(Shader.Find("Es/TexturePaint/Paint"));
			if(paintNormalMaterial == null)
				paintNormalMaterial = new Material(Shader.Find("Es/TexturePaint/PaintNormal"));
			if(paintHeightMaterial == null)
				paintHeightMaterial = new Material(Shader.Find("Es/TexturePaint/PaintHeight"));
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
				if(p.useNormalPaint)
					p.normalTexture = p.material.GetTexture(p.normalTexturePropertyID);
				if(p.useHeightPaint)
					p.heightTexture = p.material.GetTexture(p.heightTexturePropertyID);
			}
		}

		/// <summary>
		/// RenderTextureを生成しマテリアルにセットする
		/// </summary>
		private void SetRenderTexture()
		{
			foreach(var p in paintSet)
			{
				if(p.useMainPaint)
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
				}
				if(p.useNormalPaint)
				{
					//NormalTextureが設定されている場合
					if(p.normalTexture != null)
					{
						//法線マップテクスチャの生成
						p.paintNormalTexture = new RenderTexture(p.normalTexture.width, p.normalTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
						//法線マップのコピー
						Graphics.Blit(p.normalTexture, p.paintNormalTexture);
						//マテリアルの法線マップテクスチャをRenderTextureに変更
						p.material.SetTexture(p.normalTexturePropertyID, p.paintNormalTexture);
					}
					else
						Debug.LogWarning("法線マップペイントを利用するにはマテリアルに法線マップテクスチャが設定されている必要があります");
				}
				if(p.useHeightPaint)
				{
					//HeightTextureが設定されている場合
					if(p.heightTexture != null)
					{
						//ハイトマップテクスチャの生成
						p.paintHeightTexture = new RenderTexture(p.heightTexture.width, p.heightTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
						//ハイトマップのコピー
						Graphics.Blit(p.heightTexture, p.paintHeightTexture);
						//マテリアルのハイトマップテクスチャをRenderTextureに変更
						p.material.SetTexture(p.heightTexturePropertyID, p.paintHeightTexture);
					}
					else
						Debug.LogWarning("ハイトマップペイントを利用するにはマテリアルにハイトマップテクスチャが設定されている必要があります");
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
				if(RenderTexture.active != p.paintNormalTexture && p.paintNormalTexture != null && p.paintNormalTexture.IsCreated())
					p.paintNormalTexture.Release();
				if(RenderTexture.active != p.paintHeightTexture && p.paintHeightTexture != null && p.paintHeightTexture.IsCreated())
					p.paintHeightTexture.Release();
			}
		}

		/// <summary>
		/// ペイントに必要なデータをシェーダーにセットする
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		private void SetPaintMainData(PaintBrush brush, Vector2 uv)
		{
			paintMaterial.SetVector(paintUVPropertyID, uv);
			paintMaterial.SetTexture(brushTexturePropertyID, brush.BrushTexture);
			paintMaterial.SetFloat(brushScalePropertyID, brush.Scale);
			paintMaterial.SetVector(brushColorPropertyID, brush.Color);

			foreach(var key in paintMaterial.shaderKeywords)
				paintMaterial.DisableKeyword(key);
			switch(brush.ColorBlending)
			{
				case PaintBrush.ColorBlendType.UseColor:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_CONTROL);
					break;

				case PaintBrush.ColorBlendType.UseBrush:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_BRUSH);
					break;

				case PaintBrush.ColorBlendType.Neutral:
					paintMaterial.EnableKeyword(COLOR_BLEND_NEUTRAL);
					break;

				case PaintBrush.ColorBlendType.AlphaOnly:
					paintMaterial.EnableKeyword(COLOR_BLEND_ALPHA_ONLY);
					break;

				default:
					paintMaterial.EnableKeyword(COLOR_BLEND_USE_CONTROL);
					break;
			}
		}

		/// <summary>
		/// 法線マップペイントに必要なデータをシェーダーにセットする
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		private void SetPaintNormalData(PaintBrush brush, Vector2 uv)
		{
			paintNormalMaterial.SetVector(paintUVPropertyID, uv);
			paintNormalMaterial.SetTexture(brushTexturePropertyID, brush.BrushTexture);
			paintNormalMaterial.SetTexture(brushNormalTexturePropertyID, brush.BrushNormalTexture);
			paintNormalMaterial.SetFloat(brushScalePropertyID, brush.Scale);
			paintNormalMaterial.SetFloat(brushNormalBlendPropertyID, brush.NormalBlend);

			foreach(var key in paintNormalMaterial.shaderKeywords)
				paintNormalMaterial.DisableKeyword(key);
			switch(brush.NormalBlending)
			{
				case PaintBrush.NormalBlendType.UseBrush:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_USE_BRUSH);
					break;

				case PaintBrush.NormalBlendType.Add:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_ADD);
					break;

				case PaintBrush.NormalBlendType.Sub:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_SUB);
					break;

				case PaintBrush.NormalBlendType.Min:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_MIN);
					break;

				case PaintBrush.NormalBlendType.Max:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_MAX);
					break;

				default:
					paintNormalMaterial.EnableKeyword(NORMAL_BLEND_USE_BRUSH);
					break;
			}
		}

		/// <summary>
		/// ハイトマップペイントに必要なデータをシェーダーにセットする
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		private void SetPaintHeightData(PaintBrush brush, Vector2 uv)
		{
			paintHeightMaterial.SetVector(paintUVPropertyID, uv);
			paintHeightMaterial.SetTexture(brushTexturePropertyID, brush.BrushTexture);
			paintHeightMaterial.SetTexture(brushHeightTexturePropertyID, brush.BrushHeightTexture);
			paintHeightMaterial.SetFloat(brushScalePropertyID, brush.Scale);
			paintHeightMaterial.SetFloat(brushHeightBlendPropertyID, brush.HeightBlend);
			paintHeightMaterial.SetVector(brushHeightColorPropertyID, brush.Color);

			foreach(var key in paintHeightMaterial.shaderKeywords)
				paintHeightMaterial.DisableKeyword(key);
			switch(brush.HeightBlending)
			{
				case PaintBrush.HeightBlendType.UseBrush:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_USE_BRUSH);
					break;

				case PaintBrush.HeightBlendType.Add:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_ADD);
					break;

				case PaintBrush.HeightBlendType.Sub:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_SUB);
					break;

				case PaintBrush.HeightBlendType.Min:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_MIN);
					break;

				case PaintBrush.HeightBlendType.Max:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_MAX);
					break;

				case PaintBrush.HeightBlendType.ColorRGB_HeightA:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_COLOR_RGB_HEIGHT_A);
					break;

				default:
					paintHeightMaterial.EnableKeyword(HEIGHT_BLEND_ADD);
					break;
			}
		}

		#endregion PrivateMethod

		#region PublicMethod

		/// <summary>
		/// 直接UV座標を指定したペイント処理を行う
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="uv">ヒット位置のUV座標</param>
		/// <returns>ペイントの成否</returns>
		public bool PaintUVDirect(PaintBrush brush, Vector2 uv)
		{
			foreach(var p in paintSet)
			{
				//メインテクスチャへのペイント
				if(p.useMainPaint && brush.BrushTexture != null && p.paintMainTexture != null && p.paintMainTexture.IsCreated())
				{
					var mainPaintTextureBuffer = RenderTexture.GetTemporary(p.mainTexture.width, p.mainTexture.height);
					SetPaintMainData(brush, uv);
					Graphics.Blit(p.paintMainTexture, mainPaintTextureBuffer, paintMaterial);
					Graphics.Blit(mainPaintTextureBuffer, p.paintMainTexture);
					RenderTexture.ReleaseTemporary(mainPaintTextureBuffer);
				}

				//法線マップへのペイント
				if(p.useNormalPaint && brush.BrushNormalTexture != null && p.paintNormalTexture != null && p.paintNormalTexture.IsCreated())
				{
					var normalPaintTextureBuffer = RenderTexture.GetTemporary(p.normalTexture.width, p.normalTexture.height);
					SetPaintNormalData(brush, uv);
					Graphics.Blit(p.paintNormalTexture, normalPaintTextureBuffer, paintNormalMaterial);
					Graphics.Blit(normalPaintTextureBuffer, p.paintNormalTexture);
					RenderTexture.ReleaseTemporary(normalPaintTextureBuffer);
				}
				//ハイトマップへのペイント
				if(p.useHeightPaint && brush.BrushHeightTexture != null && p.paintHeightTexture != null && p.paintHeightTexture.IsCreated())
				{
					var heightPaintTextureBuffer = RenderTexture.GetTemporary(p.heightTexture.width, p.heightTexture.height);
					SetPaintHeightData(brush, uv);
					Graphics.Blit(p.paintHeightTexture, heightPaintTextureBuffer, paintHeightMaterial);
					Graphics.Blit(heightPaintTextureBuffer, p.paintHeightTexture);
					RenderTexture.ReleaseTemporary(heightPaintTextureBuffer);
				}
			}
			return true;
		}

		/// <summary>
		/// 与えられたworldPosに近いMeshSurface上の点に対してペイント処理を行う
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="worldPos">近似点</param>
		/// <param name="renderCamera">レンダリングに利用するカメラ</param>
		/// <returns></returns>
		public bool PaintNearestTriangleSurface(PaintBrush brush, Vector3 worldPos, Camera renderCamera = null)
		{
			var p = transform.worldToLocalMatrix.MultiplyPoint(worldPos);
			var pd = meshOperator.NearestLocalSurfacePoint(p);

			return Paint(brush, transform.localToWorldMatrix.MultiplyPoint(pd), renderCamera);
		}

		/// <summary>
		/// ペイント処理を行う
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="worldPos">
		/// キャンバス上の塗る点(World-Space)
		/// メッシュが構成する形状のサーフェスの上の点
		/// </param>
		/// <param name="renderCamera">レンダリングに利用するカメラ</param>
		/// <returns>ペイント成否</returns>
		public bool Paint(PaintBrush brush, Vector3 worldPos, Camera renderCamera = null)
		{
			Vector2 uv;

			if(renderCamera == null)
				renderCamera = Camera.main;

			Vector3 p = transform.InverseTransformPoint(worldPos);
			Matrix4x4 mvp = renderCamera.projectionMatrix * renderCamera.worldToCameraMatrix * transform.localToWorldMatrix;
			if(meshOperator.LocalPointToUV(p, mvp, out uv))
				return PaintUVDirect(brush, uv);

			return false;
		}

		/// <summary>
		/// ペイント処理を行う
		/// CanvasにはMeshColliderが設定されている必要があります
		/// </summary>
		/// <param name="brush">ブラシ</param>
		/// <param name="hitInfo">RaycastのHit情報</param>
		/// <returns>ペイントの成否</returns>
		public bool Paint(PaintBrush brush, RaycastHit hitInfo)
		{
			if(hitInfo.collider != null && hitInfo.collider.gameObject == gameObject)
			{
				#region ErrorCheck

				if(brush == null)
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
					return PaintNearestTriangleSurface(brush, hitInfo.point);
				}
				return PaintUVDirect(brush, hitInfo.textureCoord);
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

		/// <summary>
		/// 元のメインテクスチャを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>元のメインテクスチャ</returns>
		public Texture GetMainTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.mainTexture;
		}

		/// <summary>
		/// ペイント中のメインテクスチャを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>ペイント中のメインテクスチャ</returns>
		public RenderTexture GetPaintMainTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.paintMainTexture;
		}

		/// <summary>
		/// 元の法線マップを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>元の法線マップ</returns>
		public Texture GetNormalTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.normalTexture;
		}

		/// <summary>
		/// ペイント中の法線マップを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>ペイント中の法線マップ</returns>
		public RenderTexture GetPaintNormalTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.paintNormalTexture;
		}

		/// <summary>
		/// 元のハイトマップを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>元のハイトマップ</returns>
		public Texture GetHeightTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.heightTexture;
		}

		/// <summary>
		/// ペイント中のハイトマップを取得する
		/// </summary>
		/// <param name="materialName">取得するテクスチャが設定されているマテリアル名</param>
		/// <returns>ペイント中のハイトマップ</returns>
		public RenderTexture GetPaintHeightTexture(string materialName)
		{
			var data = paintSet.FirstOrDefault(p => p.material.name == materialName);
			if(data == null)
				return null;
			return data.paintHeightTexture;
		}

		#endregion PublicMethod

		#region CustomEditor

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
							normalTextureName = "_BumpMap",
							heightTextureName = "_ParallaxMap",
							useMainPaint = true,
							useNormalPaint = false
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

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].mainTextureName = EditorGUILayout.TextField("MainTexture Property Name", instance.paintSet[i].mainTextureName);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Main Texture Name", p => p.mainTextureName = instance.paintSet[i].mainTextureName);

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].normalTextureName = EditorGUILayout.TextField("NormalMap Property Name", instance.paintSet[i].normalTextureName);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Normal Texture Name", p => p.normalTextureName = instance.paintSet[i].normalTextureName);

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].heightTextureName = EditorGUILayout.TextField("HeightMap Property Name", instance.paintSet[i].heightTextureName);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Height Texture Name", p => p.heightTextureName = instance.paintSet[i].heightTextureName);

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].useMainPaint = EditorGUILayout.Toggle("Use Main Paint", instance.paintSet[i].useMainPaint);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Use Main Paint", p => p.useMainPaint = instance.paintSet[i].useMainPaint);

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].useNormalPaint = EditorGUILayout.Toggle("Use NormalMap Paint", instance.paintSet[i].useNormalPaint);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Use Normal Paint", p => p.useNormalPaint = instance.paintSet[i].useNormalPaint);

						EditorGUI.BeginChangeCheck();
						instance.paintSet[i].useHeightPaint = EditorGUILayout.Toggle("Use HeightMap Paint", instance.paintSet[i].useHeightPaint);
						if(EditorGUI.EndChangeCheck())
							ChangeValue(i, "Use Height Paint", p => p.useHeightPaint = instance.paintSet[i].useHeightPaint);

						EditorGUI.indentLevel = 0;
					}
				}
			}

			/// <summary>
			/// 値の変更を行う
			/// </summary>
			/// <param name="paintSetIndex">変更対象のPaintSetのIndex</param>
			/// <param name="recordName">Undo登録時のレコード名</param>
			/// <param name="assign">代入処理</param>
			private void ChangeValue(int paintSetIndex, string recordName, Action<PaintSet> assign)
			{
				Undo.RecordObjects(targets, "Change " + recordName);
				foreach(var t in targets.Where(_t => _t is DynamicCanvas).Select(_t => _t as DynamicCanvas))
					if(t.paintSet.Count > paintSetIndex)
					{
						assign(t.paintSet[paintSetIndex]);
						EditorUtility.SetDirty(t);
					}
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

#endif

		#endregion CustomEditor
	}
}