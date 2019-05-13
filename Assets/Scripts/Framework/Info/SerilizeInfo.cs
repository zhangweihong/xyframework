using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 序列化简单对象
/// </summary>
[System.Serializable]
public class SInfoObject
{
    public UnityEngine.Object Object;
    public string AssetName;
    public string BundleName;
    public AssetType Type;
    public Rect SpriteRect;
    public Vector2 SpritePivot;
    public Vector4 SpriteBorder;
    public bool IsAtlas;
    public string SpName;
    public int ID = 0;
}

/// <summary>
/// ui的 上资源的序列化记录 基本信息 现在 支持Image, RawImage, SpriteRenderer所对应的资源
/// </summary>
public class SerilizeInfo : MonoBehaviour
{

    [SerializeField]
    private List<SInfoObject> m_Objects = new List<SInfoObject>();

    /// <summary>
    /// 加载迭代器使用的计数器
    /// </summary>
    [SerializeField]
    private int loadIndex = 0;

    void Start()
    {

    }

    /// <summary>
    /// 获取 只能在编辑器下面使用
    /// </summary>
    public void Serilize()
    {
#if UNITY_EDITOR
        List<Object> allO = new List<Object>();
        allO.AddRange(this.gameObject.GetComponentsInChildren<Image>(true));
        allO.AddRange(this.gameObject.GetComponentsInChildren<SpriteRenderer>(true));
        allO.AddRange(this.gameObject.GetComponentsInChildren<RawImage>(true));
        allO.AddRange(this.gameObject.GetComponentsInChildren<Animator>(true));
        allO.AddRange(this.gameObject.GetComponentsInChildren<SButtonUGUI>(true));

        for (int i = 0; i < allO.Count; i++)
        {
            if (allO[i] == null)
                continue;

            if (typeof(Image) == allO[i].GetType() || typeof(SpriteRenderer) == allO[i].GetType() || typeof(SButtonUGUI) == allO[i].GetType())
            {
                Sprite sp0 = null;
                Sprite sp1 = null;
                if (typeof(Image) == allO[i].GetType())
                {
                    sp0 = (allO[i] as Image).sprite;
                }
                else if (typeof(Image) == allO[i].GetType())
                {
                    sp0 = (allO[i] as SpriteRenderer).sprite;
                }
                else if (typeof(SButtonUGUI) == allO[i].GetType())
                {
                    sp0 = (allO[i] as SButtonUGUI).PressSprite;
                    sp1 = (allO[i] as SButtonUGUI).DisableSprite;
                }
                SInfoObject sInfoObject1 = CreateSpriteInfoObject(allO[i], sp0, 0);
                if (sInfoObject1 != null)
                {
                    m_Objects.Add(sInfoObject1);
                }
                SInfoObject sInfoObject2 = CreateSpriteInfoObject(allO[i], sp1, 1);
                if (sInfoObject2 != null)
                {
                    m_Objects.Add(sInfoObject2);
                }
            }
            else if (typeof(RawImage) == allO[i].GetType())
            {
                SInfoObject sInfoObject = new SInfoObject();
                Texture texture = (allO[i] as RawImage).texture;
                string key = string.Empty;
                string bundleName = string.Empty;
                string assetPath = string.Empty;
                if (texture == null)
                {
                    continue;
                }

                assetPath = AssetDatabase.GetAssetPath(texture);
                key = System.IO.Path.GetFileName(assetPath);
                sInfoObject.Type = Util.GetAssetType(System.IO.Path.GetExtension(key));
                bundleName = System.IO.Path.GetDirectoryName(assetPath);
                bundleName = bundleName.Replace("Assets/", "").Replace("Resources/", "").Replace("Res/", "");
                sInfoObject.BundleName = bundleName.ToKeyUrl().ToAssetBundleUrl();
                sInfoObject.AssetName = key;
                sInfoObject.Object = allO[i];
                m_Objects.Add(sInfoObject);
            }
            else if (typeof(Animator) == allO[i].GetType())
            {
                SInfoObject sInfoObject = new SInfoObject();
                RuntimeAnimatorController anictl = (allO[i] as Animator).runtimeAnimatorController;
                string key = string.Empty;
                string bundleName = string.Empty;
                string assetPath = string.Empty;
                if (anictl == null)
                {
                    continue;
                }
                assetPath = AssetDatabase.GetAssetPath(anictl);
                key = System.IO.Path.GetFileName(assetPath);
                sInfoObject.Type = Util.GetAssetType(System.IO.Path.GetExtension(key));
                bundleName = System.IO.Path.GetDirectoryName(assetPath);
                bundleName = bundleName.Replace("Assets/", "").Replace("Resources/", "").Replace("Res/", "");
                sInfoObject.BundleName = bundleName.ToKeyUrl().ToAssetBundleUrl();
                sInfoObject.AssetName = key;
                sInfoObject.Object = allO[i];
                m_Objects.Add(sInfoObject);
            }
        }
#endif
    }

    public SInfoObject CreateSpriteInfoObject(Object obj, Sprite sp, int id)
    {
        SInfoObject sInfoObject = new SInfoObject();
#if UNITY_EDITOR
        if (obj == null || sp == null)
        {
            return null;
        }
        string assetPath = AssetDatabase.GetAssetPath(sp);
        string bundleName = System.IO.Path.GetDirectoryName(assetPath);
        bundleName = bundleName.Replace("Assets/", "").Replace("Resources/", "").Replace("Res/", "");
        string key = System.IO.Path.GetFileName(assetPath);
        sInfoObject.Type = Util.GetAssetType(System.IO.Path.GetExtension(key));
        if (key == "unity_builtin_extra") //排除内部自己用的 图集 如 UIMask 等等 scrollview 用的
        {
            return null;
        }
        if (bundleName.Contains("AtlasTexture/"))//属于altas 引用的的贴图
        {
            sInfoObject.IsAtlas = true;
            bundleName = bundleName.Replace("AtlasTexture", "UIAltals");
        }
        if (sInfoObject.IsAtlas)
        {
            sInfoObject.AssetName = sp.name;
        }
        else
        {
            sInfoObject.AssetName = key;
        }
        sInfoObject.BundleName = bundleName.ToKeyUrl().ToAssetBundleUrl();
        sInfoObject.Object = obj;
        sInfoObject.SpriteRect = sp.rect;
        sInfoObject.SpriteBorder = sp.border;
        sInfoObject.SpritePivot = sp.pivot;
        sInfoObject.SpName = sp.name;
        sInfoObject.ID = id;
#endif
        return sInfoObject;
    }

    /// <summary>
    /// 卸载依赖
    /// </summary>
    public void ClearAsset()
    {
#if UNITY_EDITOR
        loadIndex = 0;
        for (int i = 0; i < m_Objects.Count; i++)
        {
            if (typeof(Image) == m_Objects[i].Object.GetType())
            {
                ((Image)m_Objects[i].Object).sprite = null;
            }
            else if (typeof(SButtonUGUI) == m_Objects[i].Object.GetType())
            {
                ((SButtonUGUI)m_Objects[i].Object).PressSprite = null;
                ((SButtonUGUI)m_Objects[i].Object).DisableSprite = null;
            }
            else if (typeof(SpriteRenderer) == m_Objects[i].Object.GetType())
            {
                ((SpriteRenderer)m_Objects[i].Object).sprite = null;
            }
            else if (typeof(RawImage) == m_Objects[i].Object.GetType())
            {
                ((RawImage)m_Objects[i].Object).texture = null;
            }
            else if (typeof(Animator) == m_Objects[i].Object.GetType())
            {
                ((Animator)m_Objects[i].Object).runtimeAnimatorController = null;
            }
        }
#endif
    }

    public void LoadSerizeInfoAsset(System.Action finish = null)
    {
        if (m_Objects.Count == 0)
        {
            CheckFinish(finish);
            return;
        }
        for (int i = 0; i < m_Objects.Count; i++)
        {
            SInfoObject info = m_Objects[i];
            if (info != null)
            {
                string bname = info.BundleName;
                string assetname = info.AssetName;
                if (info.IsAtlas)
                {
                    assetname = StringUtil.AppendFormat("{0}.{1}", System.IO.Path.GetFileNameWithoutExtension(bname), "spriteatlas");
                }
                _LoadSerizeInfoAsset(bname, assetname, info.Type, (obj) =>
                {
                    if (typeof(RawImage) == info.Object.GetType())
                    {
                        (info.Object as RawImage).texture = (obj as Texture);
                    }
                    else if (typeof(Image) == info.Object.GetType())
                    {
                        if (obj is Texture2D)
                        {
                            Texture2D tex = obj as Texture2D;
                            Image img = (info.Object as Image);
                            img.sprite = Sprite.Create(tex, info.SpriteRect, info.SpritePivot, 100, 0, SpriteMeshType.Tight, info.SpriteBorder);
                            img.sprite.name = info.SpName;
                        }
                        else if (obj is SpriteAtlas)
                        {
                            SpriteAtlas sa = obj as SpriteAtlas;
                            Image img = (info.Object as Image);
                            img.sprite = sa.GetSprite(info.AssetName);
                            img.sprite.name = info.SpName;
                        }
                    }
                    else if (typeof(SpriteRenderer) == info.Object.GetType())
                    {
                        if (obj is Texture2D)
                        {
                            Texture2D tex = obj as Texture2D;
                            SpriteRenderer spr = (info.Object as SpriteRenderer);
                            spr.sprite = Sprite.Create(tex, info.SpriteRect, info.SpritePivot, 100, 0, SpriteMeshType.Tight, info.SpriteBorder);
                            spr.name = info.SpName;
                        }
                        else if (obj is SpriteAtlas)
                        {
                            SpriteAtlas sa = obj as SpriteAtlas;
                            SpriteRenderer spr = (info.Object as SpriteRenderer);
                            spr.sprite = sa.GetSprite(info.AssetName);
                            spr.name = info.SpName;
                        }
                    }
                    else if (typeof(SButtonUGUI) == info.Object.GetType())
                    {
                        if (obj is Texture2D)
                        {
                            Texture2D tex = obj as Texture2D;
                            SButtonUGUI sb = (info.Object as SButtonUGUI);
                            if (info.ID == 0)
                            {
                                sb.PressSprite = Sprite.Create(tex, info.SpriteRect, info.SpritePivot, 100, 0, SpriteMeshType.Tight, info.SpriteBorder);
                                sb.PressSprite.name = info.SpName;
                            }
                            else if (info.ID == 1)
                            {
                                sb.DisableSprite = Sprite.Create(tex, info.SpriteRect, info.SpritePivot, 100, 0, SpriteMeshType.Tight, info.SpriteBorder);
                                sb.DisableSprite.name = info.SpName;
                            }
                        }
                        else if (obj is SpriteAtlas)
                        {
                            SpriteAtlas sa = obj as SpriteAtlas;
                            SButtonUGUI sb = (info.Object as SButtonUGUI);
                            if (info.ID == 0)
                            {
                                sb.PressSprite = sa.GetSprite(info.AssetName);
                                sb.PressSprite.name = info.SpName;
                            }
                            else if (info.ID == 1)
                            {
                                sb.DisableSprite = sa.GetSprite(info.AssetName);
                                sb.DisableSprite.name = info.SpName;
                            }
                        }
                    }
                    else if (typeof(Animator) == info.Object.GetType())
                    {
                        RuntimeAnimatorController ctrl = obj as RuntimeAnimatorController;
                        (info.Object as Animator).runtimeAnimatorController = ctrl;
                    }
                    CheckFinish(finish);
                });
            }
            else
            {
                CheckFinish(finish);
            }
        }
    }

    private void CheckFinish(System.Action finish)
    {
        loadIndex++;
        if (loadIndex >= m_Objects.Count)
        {
            if (finish != null)
            {
                finish();
            }
        }
    }

    private void _LoadSerizeInfoAsset(string abName, string assetName, AssetType type, System.Action<object> callback)
    {
        ResourcesManager.I.Load(abName, assetName, callback, type, true);
    }

    public void UnloadSerizeInfoAsset_Single(Object obj)
    {
        for (int i = 0; i < m_Objects.Count; i++)
        {
            if (m_Objects[i].Object == obj)
            {
                SInfoObject info = m_Objects[i];
                if (typeof(RawImage) == info.Object.GetType())
                {
                    (info.Object as RawImage).texture = null;
                }
                else if ((typeof(Image) == info.Object.GetType()))
                {
                    (info.Object as Image).sprite = null;
                }
                else if ((typeof(SpriteRenderer) == info.Object.GetType()))
                {
                    (info.Object as SpriteRenderer).sprite = null;
                }
                else if ((typeof(SButtonUGUI) == info.Object.GetType()))
                {
                    (info.Object as SButtonUGUI).PressSprite = null;
                    (info.Object as SButtonUGUI).DisableSprite = null;
                }
                else if (typeof(Animator) == info.Object.GetType())
                {
                    (info.Object as Animator).runtimeAnimatorController = null;
                }

                string bname = info.BundleName;
                string assetname = info.AssetName;
                if (info.IsAtlas)
                {
                    assetname = StringUtil.AppendFormat("{0}.{1}", System.IO.Path.GetFileNameWithoutExtension(bname), "spriteatlas");
                }
                if (info != null)
                {
                    ResourcesManager.I.UnLoadAssetBundleAsset(bname, assetname, false);
                }
                return;
            }
        }
    }

    public void UnLoadSerizeInfoAsset()
    {
        for (int i = 0; i < m_Objects.Count; i++)
        {
            SInfoObject info = m_Objects[i];
            if (info == null)
            {
                continue;
            }
            if (typeof(RawImage) == info.Object.GetType())
            {
                (info.Object as RawImage).texture = null;
            }
            else if ((typeof(Image) == info.Object.GetType()))
            {
                (info.Object as Image).sprite = null;
            }
            else if ((typeof(SButtonUGUI) == info.Object.GetType()))
            {
                (info.Object as SButtonUGUI).PressSprite = null;
                (info.Object as SButtonUGUI).DisableSprite = null;
            }
            else if ((typeof(SpriteRenderer) == info.Object.GetType()))
            {
                (info.Object as SpriteRenderer).sprite = null;
            }
            else if (typeof(Animator) == info.Object.GetType())
            {
                (info.Object as Animator).runtimeAnimatorController = null;
            }
            string bname = info.BundleName;
            string assetname = info.AssetName;
            if (info.IsAtlas)
            {
                assetname = StringUtil.AppendFormat("{0}.{1}", System.IO.Path.GetFileNameWithoutExtension(bname), "spriteatlas");
            }
            ResourcesManager.I.UnLoadAssetBundleAsset(bname, assetname, false);
        }
        m_Objects.Clear();
        loadIndex = 0;
    }
}
