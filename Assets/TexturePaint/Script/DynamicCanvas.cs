using System.Collections;
using System.Linq;
using UnityEngine;

namespace TexturePaint
{
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshCollider))]
  public class DynamicCanvas : MonoBehaviour
  {
    #region SerializedProperties

    [SerializeField, Tooltip("メインテクスチャのプロパティ名")]
    private string mainTextureName = "_MainTex";

    [SerializeField, Tooltip("バンプマップテクスチャのプロパティ名")]
    private string bumpTextureName = "_BumpMap";

    [SerializeField, Tooltip("テクスチャペイント用マテリアル")]
    private Material paintMaterial = null;

    [SerializeField, Tooltip("ブラシバンプマップ用マテリアル")]
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

    private RenderTexture paintTexture;
    private RenderTexture paintBumpTexture;
    private Material material;

    #region UnityEventMethod

    public void Awake()
    {
      InitPropertyID();

      var meshRenderer = GetComponent<MeshRenderer>();
      material = meshRenderer.material;
      var mainTexture = material.GetTexture(mainTexturePropertyID);
      var bumpTexture = material.GetTexture(bumpTexturePropertyID);

      //Textureがもともとついてないオブジェクトには非対応
      if(mainTexture == null)
      {
        Debug.LogWarning("[DynamicPaintObject] : テクスチャの設定されていないオブジェクトに適用することはできません");
        Destroy(this);
        return;
      }
      else
      {
        //DynamicPaint用RenderTextureの生成
        paintTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        //メインテクスチャのコピー
        Graphics.Blit(mainTexture, paintTexture);
        //マテリアルのテクスチャをRenderTextureに変更
        material.SetTexture(mainTexturePropertyID, paintTexture);
      }

      if(bumpTexture == null)
      {
        Debug.LogWarning("[DynamicPaintObject] : バンプマップテクスチャの設定されていないオブジェクトに適用することはできません");
        Destroy(this);
        return;
      }
      {
        //法線マップテクスチャの生成
        paintBumpTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        //法線マップのコピー
        Graphics.Blit(bumpTexture, paintBumpTexture);
        //マテリアルの法線マップテクスチャをRenderTextureに変更
        material.SetTexture(bumpTexturePropertyID, paintBumpTexture);
      }
    }

    public void OnDestroy()
    {
      if(paintTexture != null && paintTexture.IsCreated())
        paintTexture.Release();
      if(paintBumpTexture != null && paintBumpTexture.IsCreated())
        paintBumpTexture.Release();
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
          Debug.LogError("テンポラリテクスチャの生成に失敗しました。");
          return false;
        }
        if(blush == null)
        {
          Debug.LogError("ブラシが設定されていません。値がNULLです。");
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
  }
}