using System.Linq;
using UnityEngine;

namespace TexturePaint
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshCollider))]
	[DisallowMultipleComponent]
	public class DynamicCanvas : MonoBehaviour
	{
		#region SerializedProperties

		[SerializeField, Tooltip("メインテクスチャのプロパティ名")]
		private string mainTextureName = "_MainTex";

		[SerializeField, Tooltip("バンプマップテクスチャのプロパティ名")]
		private string bumpTextureName = "_BumpMap";

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
		}

		public void OnDestroy()
		{
			Debug.Log("DynamicCanvasを破棄しました");
			ReleaseRenderTexture();
		}

#if UNITY_EDITOR && DEBUG_DYNAMIC_TEXTURE_PAINT
		public void OnGUI()
		{
			GUILayout.Label("Main Shader: " + paintMaterial.shader.name);
			GUILayout.Label("Bump Shader: " + paintBumpMaterial.shader.name);
			GUILayout.Label("Blush Main: " + (blush != null).ToString());
			GUILayout.Label("Blush Bump: " + (blushBump != null).ToString());
			GUILayout.Label("Support Main: " + paintMaterial.shader.isSupported);
			GUILayout.Label("Support Bump: " + paintBumpMaterial.shader.isSupported);
			GUILayout.Label("RenderTexture Main: " + (paintTexture != null).ToString());
			GUILayout.Label("RenderTexture Bump: " + (paintBumpTexture != null).ToString());
			GUILayout.Label("Main Texture ID:" + mainTexturePropertyID);
			GUILayout.Label("Bump Texture ID:" + bumpTexturePropertyID);
			//why is null ????
			//GUILayout.Label("Get Main Texture:" + (paintMaterial.GetTexture(mainTexturePropertyID) != null));
			//GUILayout.Label("Get Bump Texture:" + (paintMaterial.GetTexture(bumpTexturePropertyID) != null));
			GUILayout.Label("Paint UV ID:" + paintUVPropertyID);
			GUILayout.Label("Blush Main Texture ID:" + blushTexturePropertyID);
			GUILayout.Label("Blush Bump Texture ID:" + blushBumpTexturePropertyID);
			GUILayout.Label("Blush Scale ID:" + blushScalePropertyID);
			GUILayout.Label("Blush Color ID:" + blushColorPropertyID);
			GUILayout.Label("Blush Bump Blend ID::" + blushBumpBlendPropertyID);
		}
#endif

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
			blushColorPropertyID = Shader.PropertyToID("_BlushColor");
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
				Debug.LogWarning("ColloderはMeshColliderのみが設定されている必要があります");
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
				mainTexture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
			//DynamicPaint用RenderTextureの生成
			paintTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			//メインテクスチャのコピー
			Graphics.Blit(mainTexture, paintTexture);
			//マテリアルのテクスチャをRenderTextureに変更
			material.SetTexture(mainTexturePropertyID, paintTexture);

			if(bumpTexture != null)
			{
				//法線マップテクスチャの生成
				paintBumpTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				//法線マップのコピー
				Graphics.Blit(bumpTexture, paintBumpTexture);
				//マテリアルの法線マップテクスチャをRenderTextureに変更
				material.SetTexture(bumpTexturePropertyID, paintBumpTexture);
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
		/// ペイント処理
		/// </summary>
		/// <param name="hitInfo">RaycastのHit情報</param>
		/// <param name="blush">ブラシ</param>
		/// <returns>ペイントの成否</returns>
		public bool Paint(RaycastHit hitInfo, PaintBlush blush)
		{
			if(hitInfo.collider != null && hitInfo.collider.gameObject == gameObject)
			{
				var uv = hitInfo.textureCoord;
				RenderTexture buf = RenderTexture.GetTemporary(paintTexture.width, paintTexture.height);

				#region ErrorCheck

				if(buf == null)
				{
					Debug.LogError("テンポラリテクスチャの生成に失敗しました");
					return false;
				}
				if(blush == null)
				{
					Debug.LogError("ブラシが設定されていません");
					return false;
				}

				#endregion ErrorCheck

				//メインテクスチャへのペイント
				if(blush.BlushTexture != null && paintTexture != null && paintTexture.IsCreated())
				{
					paintMaterial.SetVector(paintUVPropertyID, uv);
					paintMaterial.SetTexture(blushTexturePropertyID, blush.BlushTexture);
					paintMaterial.SetFloat(blushScalePropertyID, blush.Scale);
					paintMaterial.SetVector(blushColorPropertyID, blush.Color);
					Graphics.Blit(paintTexture, buf, paintMaterial);
					Graphics.Blit(buf, paintTexture);
				}

				//バンプマップへのペイント
				if(blush.BlushBumpTexture != null && paintBumpTexture != null && paintBumpTexture.IsCreated())
				{
					paintBumpMaterial.SetVector(paintUVPropertyID, uv);
					paintBumpMaterial.SetTexture(blushTexturePropertyID, blush.BlushTexture);
					paintBumpMaterial.SetTexture(blushBumpTexturePropertyID, blush.BlushBumpTexture);
					paintBumpMaterial.SetFloat(blushScalePropertyID, blush.Scale);
					paintBumpMaterial.SetFloat(blushBumpBlendPropertyID, blush.BumpBlend);
					Graphics.Blit(paintBumpTexture, buf, paintBumpMaterial);
					Graphics.Blit(buf, paintBumpTexture);
				}

				RenderTexture.ReleaseTemporary(buf);
				return true;
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