using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// 创建view工具
/// </summary>
public class ViewCreateEditor : EditorWindow
{
    private static ViewCreateEditor _uiWindow;
    private static string _rootCSharpPath = "";
    private static string _rootLuaPath = "";
    private static string _uiCacheAssetPath = "";
    private static string _uiCacheAbsPath = "";
    private static bool _IsInit = false;

    private static List<ViewInstance> _uiList = new List<ViewInstance>();
    private static string[] _langArry = new string[] { "Lua", "C#" };
    private static int _LangSettingIndex = 0;

    [MenuItem("XYFramework/View编辑器")]
    static void ShowWindow()
    {
        _uiWindow = EditorWindow.GetWindow(typeof(ViewCreateEditor), false, "View编辑器", true) as ViewCreateEditor;
        _uiWindow.Show();
    }

    private static GUILayoutOption[] smallOptions;
    private static GUILayoutOption[] bigOptions;
    private static GUILayoutOption[] biggerOptions;
    private static GUILayoutOption[] widthOptions;
    static void Init()
    {
        if (_IsInit)
        {
            return;
        }
        _IsInit = true;
        widthOptions = new GUILayoutOption[1];
        widthOptions[0] = GUILayout.Width(50);
        smallOptions = new GUILayoutOption[2];
        smallOptions[0] = GUILayout.Width(50);
        smallOptions[1] = GUILayout.Height(20);
        bigOptions = new GUILayoutOption[2];
        bigOptions[0] = GUILayout.Width(100);
        bigOptions[1] = GUILayout.Height(20);
        biggerOptions = new GUILayoutOption[2];
        biggerOptions[0] = GUILayout.Width(150);
        biggerOptions[1] = GUILayout.Height(20);
        _rootCSharpPath = Application.dataPath + "/Scripts/Framework/CustomUI/";
        _rootLuaPath = Application.dataPath + "/Res/Lua/ui/";
        _uiCacheAssetPath = "Assets/Scripts/Framework/CustomUI/UIListCache.asset";
        _uiCacheAbsPath = _rootCSharpPath + "UIListCache.asset";
        ReLoadData();
    }


    private void OnGUI()
    {
        DrawWindow();
    }

    static void DrawWindow()
    {
        Init();
        _LangSettingIndex = EditorGUILayout.Popup("当前的语言环境", _LangSettingIndex, _langArry);
        EditorGUILayout.BeginVertical();
        int _removeIndex = -1;
        for (int i = 0; i < _uiList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View类型->", smallOptions);
            if (!_uiList[i].IsCreate)
            {
                _uiList[i].ID = (UIID)EditorGUILayout.EnumPopup(_uiList[i].ID, biggerOptions);
                _uiList[i].Name = EditorGUILayout.TextField(_uiList[i].Name, biggerOptions);
                if (GUILayout.Button("生成脚本", bigOptions))
                {
                    _uiList[i].IsCreate = true;
                    if (_LangSettingIndex == 1)//暂时废弃
                    {
                        SaveToCSharpData();
                        CreateUICSharpScript(_uiList[i].Name, _uiList[i].ID);
                    }
                    else
                    {
                        SaveToLuaData();
                        CreateUILuaScript(_uiList[i].Name, _uiList[i].ID);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(_uiList[i].ID.ToString(), biggerOptions);
                EditorGUILayout.LabelField("已创建->" + _uiList[i].Name, biggerOptions);
            }

            if (GUILayout.Button("删除", smallOptions))
            {
                string scriptPath = string.Empty;
                if (_LangSettingIndex == 1)
                {
                    scriptPath = _rootCSharpPath + _uiList[i].Name;
                }
                else
                {
                    scriptPath = _rootLuaPath + _uiList[i].Name.ToKeyUrl();
                }
                if (Directory.Exists(scriptPath) && _uiList[i].IsCreate)
                {
                    if (EditorUtility.DisplayDialog("删除现有的脚本", "删除现有的脚本,慎重选择确认已经备份！！！", "确定", "取消"))
                    {
                        _removeIndex = i;
                        Directory.Delete(scriptPath, true);
                    }
                }
                else
                {
                    _removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (_uiList.Count > 0 && _removeIndex > -1)
        {
            _uiList.RemoveAt(_removeIndex);
            _removeIndex = -1;
            if (_LangSettingIndex == 1)
            {
                SaveToCSharpData();
            }
            else
            {
                SaveToLuaData();
            }
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("添加", bigOptions))
        {
            ViewInstance _i = new ViewInstance();
            _i.Name = "";
            _i.ID = UIID.None;
            _uiList.Add(_i);
        }
        if (GUILayout.Button("刷新", bigOptions))
        {
            ReLoadData();
        }
        if (GUILayout.Button("保存", bigOptions))
        {
            if (_LangSettingIndex == 1)
            {
                SaveToCSharpData();
            }
            else
            {
                SaveToLuaData();
            }
        }
        EditorGUILayout.EndVertical();
    }


    static void ReLoadData()
    {
        if (File.Exists(_uiCacheAbsPath))
        {
            ViewInstanceCache uilistCache = AssetDatabase.LoadAssetAtPath(_uiCacheAssetPath, typeof(ViewInstanceCache)) as ViewInstanceCache;
            _LangSettingIndex = uilistCache.LangSettingindex;
            _uiList = uilistCache.ViewList;
        }
        else
        {
            Debug.Log("缓存目录不存在");
        }
    }


    #region Lua 脚本 开始
    static void CreateUILuaScript(string name, UIID id)
    {
        string luaname = name.ToKeyUrl();
        string targetDic = _rootLuaPath + luaname;

        if (Directory.Exists(targetDic))
        {
            if (EditorUtility.DisplayDialog("确定是否覆盖原来脚本", "是否覆盖写好的脚本！！！", "确定", "取消"))
            {
                Directory.Delete(targetDic, true);
                StartLuaCreate(targetDic, name);
            }
        }
        else
        {
            StartLuaCreate(targetDic, name);
        }
    }

    static void StartLuaCreate(string targetDic, string name)
    {
        Directory.CreateDirectory(targetDic);
        string path = WirteLuaScript(targetDic, name);
        AssetDatabase.Refresh();
        UIComponentEditor.CreateViewRoot(path, name);
    }

    static string WirteLuaScript(string dic, string scripename)
    {
        string viewPath = Path.Combine(dic, scripename + "view.lua.txt").ToSingleForwardSlash().ToForwardSlash();
        string tempViewPath = Path.Combine(Application.dataPath, "Scripts/Framework/Lua/templateview.lua.txt");
        string viewStr = File.ReadAllText(tempViewPath);
        viewStr = viewStr.Replace("temp", scripename);
        StreamWriter viewSw = File.CreateText(viewPath);
        viewSw.Write(viewStr);
        viewSw.Close();
        viewSw.Dispose();

        string modelPath = Path.Combine(dic, scripename + "model.lua.txt");
        string tempModelPath = Path.Combine(Application.dataPath, "Scripts/Framework/Lua/templatemodel.lua.txt");
        string modelStr = File.ReadAllText(tempModelPath);
        modelStr = modelStr.Replace("temp", scripename);
        StreamWriter modelSw = File.CreateText(modelPath);
        modelSw.Write(modelStr);
        modelSw.Close();
        modelSw.Dispose();

        string controllerPath = Path.Combine(dic, scripename + "controller.lua.txt");
        string tempControllerPath = Path.Combine(Application.dataPath, "Scripts/Framework/Lua/templatecontroller.lua.txt");
        string controllerStr = File.ReadAllText(tempControllerPath);
        controllerStr = controllerStr.Replace("temp", scripename);
        StreamWriter controllerSw = File.CreateText(controllerPath);
        controllerSw.Write(controllerStr);
        controllerSw.Close();
        controllerSw.Dispose();
        string targetRootPath = Path.Combine(Application.dataPath, "Res/").ToSingleForwardSlash().ToForwardSlash();
        return viewPath.Replace(targetRootPath, "");
    }

    static void SaveToLuaData()
    {
        ViewInstanceCache uilistCache = ScriptableObject.CreateInstance<ViewInstanceCache>();
        uilistCache.ViewList = _uiList;
        uilistCache.LangSettingindex = _LangSettingIndex;
        if (File.Exists(_uiCacheAbsPath))
        {
            File.Delete(_uiCacheAbsPath);
        }
        WirteLuaUIManager();
        AssetDatabase.CreateAsset(uilistCache, _uiCacheAssetPath);
        AssetDatabase.Refresh();

    }

    static void WirteLuaUIManager()
    {
        string luaUIManagerPath = Path.Combine(Application.dataPath, "Res/Lua/app/uimanagerauto.lua.txt");
        if (File.Exists(luaUIManagerPath))
        {
            File.Delete(luaUIManagerPath);
        }
        StreamWriter luauimSw = File.CreateText(luaUIManagerPath);
        luauimSw.WriteLine("function UIManager.registercontroller()");
        for (int i = 0; i < _uiList.Count; i++)
        {
            luauimSw.WriteLine("\tUIManager.controllerdic[UIID." + _uiList[i].ID.ToString() + "] = require(\"Lua/ui/" + _uiList[i].Name.ToKeyUrl() + "/" + _uiList[i].Name.ToKeyUrl() + "controller\")");
        }
        luauimSw.WriteLine("end");
        luauimSw.WriteLine("");
        luauimSw.WriteLine("function UIManager.registermodel()");
        for (int i = 0; i < _uiList.Count; i++)
        {
            luauimSw.WriteLine("\tUIManager.modeldic[UIID." + _uiList[i].ID.ToString() + "] = require(\"Lua/ui/" + _uiList[i].Name.ToKeyUrl() + "/" + _uiList[i].Name.ToKeyUrl() + "model\")");
        }
        luauimSw.WriteLine("end");
        luauimSw.WriteLine("");
        luauimSw.WriteLine("function UIManager.initmvc()");
        luauimSw.WriteLine("\tfor i, v in ipairs(UIManager.controllerdic) do");
        luauimSw.WriteLine("\t\tif v ~= nil then");
        luauimSw.WriteLine("\t\t\tv.init()");
        luauimSw.WriteLine("\t\tend");
        luauimSw.WriteLine("\tend");
        luauimSw.WriteLine("");
        luauimSw.WriteLine("\tfor i, v in ipairs(UIManager.modeldic) do");
        luauimSw.WriteLine("\t\tif v ~= nil then");
        luauimSw.WriteLine("\t\t\tv.init()");
        luauimSw.WriteLine("\t\tend");
        luauimSw.WriteLine("\tend");
        luauimSw.WriteLine("end");
        luauimSw.Close();
        luauimSw.Dispose();
    }

    #endregion

    #region C# 脚本相关 开始
    static void CreateUICSharpScript(string name, UIID id)
    {
        string targetPath = _rootCSharpPath + name;
        if (Directory.Exists(targetPath))
        {
            if (EditorUtility.DisplayDialog("确定是否覆盖原来脚本", "是否覆盖写好的脚本！！！", "确定", "取消"))
            {
                Directory.Delete(targetPath, true);
                StartCSharpCreate(targetPath, name);
            }
        }
        else
        {
            StartCSharpCreate(targetPath, name);
        }
        AssetDatabase.Refresh();
    }


    static void StartCSharpCreate(string targetPath, string name)
    {
        string mvcPath = targetPath + "/MVC/";
        Directory.CreateDirectory(mvcPath);
        WirteCSharpScript(mvcPath, name);
    }

    static void SaveToCSharpData()
    {
        ViewInstanceCache uilistCache = ScriptableObject.CreateInstance<ViewInstanceCache>();
        uilistCache.ViewList = _uiList;
        if (File.Exists(_uiCacheAbsPath))
        {
            File.Delete(_uiCacheAbsPath);
        }
        WirteCSharpUIManager();
        AssetDatabase.CreateAsset(uilistCache, _uiCacheAssetPath);
        AssetDatabase.Refresh();
    }

    static void WirteCSharpUIManager()
    {
        string uimanagercspath = _rootCSharpPath + "../Framework/Manager/UIManagerPartial.cs";
        if (File.Exists(uimanagercspath))
        {
            File.Delete(uimanagercspath);
        }
        StreamWriter swUImanager = File.CreateText(uimanagercspath);
        swUImanager.WriteLine("using UnityEngine;");
        swUImanager.WriteLine("using System.Collections;");
        swUImanager.WriteLine("");
        swUImanager.WriteLine("/// <summary>");
        swUImanager.WriteLine("/// 自动生成类，请勿手动修改");
        swUImanager.WriteLine("/// </summary>");
        swUImanager.WriteLine("public partial class UIManager : SingletonMono<UIManager>");
        swUImanager.WriteLine("{");
        swUImanager.WriteLine("\tprivate void RegisterController()");
        swUImanager.WriteLine("\t{");
        for (int i = 0; i < _uiList.Count; i++)
        {
            swUImanager.WriteLine("\t\tcontrollerDic[UIID." + _uiList[i].ID.ToString() + "] = new " + _uiList[i].Name + "Controller();");
        }
        swUImanager.WriteLine("\t}");
        swUImanager.WriteLine("");
        swUImanager.WriteLine("\tprivate void RegisterModel()");
        swUImanager.WriteLine("\t{");
        for (int i = 0; i < _uiList.Count; i++)
        {
            swUImanager.WriteLine("\t\tmodelDic[UIID." + _uiList[i].ID.ToString() + "] = new " + _uiList[i].Name + "Model();");
        }
        swUImanager.WriteLine("\t}");
        swUImanager.WriteLine("");
        swUImanager.WriteLine("\tprivate void InitMVC()");
        swUImanager.WriteLine("\t{");
        swUImanager.WriteLine("\t\tforeach (var item in controllerDic)");
        swUImanager.WriteLine("\t\t{");
        swUImanager.WriteLine("\t\t\titem.Value.Init();");
        swUImanager.WriteLine("\t\t}");
        swUImanager.WriteLine("\t\tforeach (var item in modelDic)");
        swUImanager.WriteLine("\t\t{");
        swUImanager.WriteLine("\t\t\titem.Value.Init();");
        swUImanager.WriteLine("\t\t}");
        swUImanager.WriteLine("\t}");
        swUImanager.WriteLine("}");
        swUImanager.Close();
        swUImanager.Dispose();
        AssetDatabase.Refresh();
    }

    static void WirteCSharpScript(string rootPath, string name)
    {
        StreamWriter swController = File.CreateText(rootPath + name + "Controller.cs");
        swController.WriteLine("using UnityEngine;");
        swController.WriteLine("using System.Collections;");
        swController.WriteLine("");
        swController.WriteLine("public class " + name + "Controller : BaseController");
        swController.WriteLine("{");
        swController.WriteLine("\tpublic static " + name + "Controller I");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tget;");
        swController.WriteLine("\t\tprivate set;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tprivate " + name + "Model M");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tget;");
        swController.WriteLine("\t\tset;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tprivate " + name + "View V");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tget;");
        swController.WriteLine("\t\tset;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic override void Init()");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tI = this;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic override void RegisterModel(BaseModle model)");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tM = model as " + name + "Model;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic override void RefreshModel(BaseModle model)");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tM = model as " + name + "Model;");
        swController.WriteLine("\t\tV.SetData(M);");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("public override void RegisterView(BaseView view)");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tV = view as " + name + "View;");
        swController.WriteLine("\t\tV.OnClose += OnClose;");
        swController.WriteLine("\t\tV.OnOpen += OnOpen;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic override void UnregisterView()");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tV.OnClose -= OnClose;");
        swController.WriteLine("\t\tV.OnOpen -= OnOpen;");
        swController.WriteLine("\t\tV = null;");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic override void Clear()");
        swController.WriteLine("\t{");
        swController.WriteLine("\t}");
        swController.WriteLine("");
        swController.WriteLine("\tpublic void OnOpen()");
        swController.WriteLine("\t{");
        swController.WriteLine("");
        swController.WriteLine("\t}");
        swController.WriteLine("\tpublic void OnClose()");
        swController.WriteLine("\t{");
        swController.WriteLine("");
        swController.WriteLine("\t}");
        swController.WriteLine("\tpublic override void UnLoad()");
        swController.WriteLine("\t{");
        swController.WriteLine("\t\tif (V != null)");
        swController.WriteLine("\t\t{");
        swController.WriteLine("\t\t\tV.UnLoad();");
        swController.WriteLine("\t\t\tUnregisterView();");
        swController.WriteLine("\t\t\tResourcesManager.I.UnLoadAssetBundleAsset(this.ABName, this.AssetName);");
        swController.WriteLine("\t\t}");
        swController.WriteLine("\t}");
        swController.WriteLine("}");
        swController.Close();
        swController.Dispose();

        StreamWriter swModel = File.CreateText(rootPath + name + "Model.cs");
        swModel.WriteLine("using UnityEngine;");
        swModel.WriteLine("using System.Collections;");
        swModel.WriteLine("");
        swModel.WriteLine("public class " + name + "Model : BaseModle");
        swModel.WriteLine("{");
        swModel.WriteLine("\tpublic override void Init()");
        swModel.WriteLine("\t{");
        swModel.WriteLine("\t\tbase.Init();");
        swModel.WriteLine("\t}");
        swModel.WriteLine("");
        swModel.WriteLine("\tpublic override void Clear()");
        swModel.WriteLine("\t{");
        swModel.WriteLine("\t\tbase.Clear();");
        swModel.WriteLine("\t}");
        swModel.WriteLine("");
        swModel.WriteLine("\tpublic override void InitTableData()");
        swModel.WriteLine("\t{");
        swModel.WriteLine("\t\tbase.InitTableData();");
        swModel.WriteLine("\t}");
        swModel.WriteLine("\t}");
        swModel.Close();
        swModel.Dispose();

        StreamWriter swView = File.CreateText(rootPath + name + "View.cs");
        swView.WriteLine("using UnityEngine;");
        swView.WriteLine("using System.Collections;");
        swView.WriteLine("");
        swView.WriteLine("public class " + name + "View : BaseView");
        swView.WriteLine("{");
        swView.WriteLine("\tvoid Awake()");
        swView.WriteLine("\t{");
        swView.WriteLine("");
        swView.WriteLine("\t}");
        swView.WriteLine("");
        swView.WriteLine("\tvoid Start()");
        swView.WriteLine("\t{");
        swView.WriteLine("");
        swView.WriteLine("\t}");
        swView.WriteLine("\tpublic void SetData(" + name + "Model model)");
        swView.WriteLine("\t{");
        swView.WriteLine("");
        swView.WriteLine("\t}");
        swView.WriteLine("\tpublic override void Init()");
        swView.WriteLine("\t{");
        swView.WriteLine("\t\tbase.Init();");
        swView.WriteLine("\t}");
        swView.WriteLine("");
        swView.WriteLine("\tpublic override void OpenView()");
        swView.WriteLine("\t{");
        swView.WriteLine("\t\tbase.OpenView();");
        swView.WriteLine("\t}");
        swView.WriteLine("");
        swView.WriteLine("\tpublic override void CloseView(bool isFalseActive = false, bool isShowCloseAni = true)");
        swView.WriteLine("\t{");
        swView.WriteLine("\t\tbase.CloseView(isFalseActive, isShowCloseAni);");
        swView.WriteLine("\t}");
        swView.WriteLine("");
        swView.WriteLine("\tpublic override void Clear()");
        swView.WriteLine("\t{");
        swView.WriteLine("\t\tbase.Clear();");
        swView.WriteLine("\t}");
        swView.WriteLine("");
        swView.WriteLine("\tpublic override void UnLoad()");
        swView.WriteLine("\t{");
        swView.WriteLine("\t\tbase.UnLoad();");
        swView.WriteLine("\t}");
        swView.WriteLine("}");
        swView.Close();
        swView.Dispose();
    }
    #endregion

    [MenuItem("Assets/Image转换为RawImage")]
    static void ImgChangRawImage()
    {
        Transform canvastr = GameObject.Find("DontDestroy/UICamera/Canvas").transform;
        GameObject obj = Selection.activeGameObject;
        GameObject view = GameObject.Instantiate(obj, canvastr);
        view.name = obj.name;
        Image img = view.GetComponent<Image>();
        Image[] childtrs = view.GetComponentsInChildren<Image>(true);
        List<Image> imgs = new List<Image>();
        imgs.Add(img);
        imgs.AddRange(childtrs);
        for (int i = 0; i < imgs.Count; i++)
        {
            if (imgs[i] == null)
                continue;

            string assetpath = AssetDatabase.GetAssetPath(imgs[i].sprite);
            if (assetpath.Contains("unity_builtin_extra")) //排除内部自己用的 图集 如 UIMask 等等 scrollview 用的
            {
                continue;
            }
            Debug.Log(assetpath);
            Texture tex = AssetDatabase.LoadAssetAtPath(assetpath, typeof(Texture)) as Texture;
            GameObject imgobj = imgs[i].gameObject;
            DestroyImmediate(imgs[i]);
            RawImage rwimg = imgobj.AddComponent<RawImage>();
            rwimg.texture = tex;
        }
    }

    [MenuItem("Assets/RawImage转换为Image")]
    static void RawImageChangImg()
    {
        Transform canvastr = GameObject.Find("DontDestroy/UICamera/Canvas").transform;
        GameObject obj = Selection.activeGameObject;
        GameObject view = GameObject.Instantiate(obj, canvastr);
        view.name = obj.name;
        RawImage rawimg = view.GetComponent<RawImage>();
        RawImage[] childtrs = view.GetComponentsInChildren<RawImage>(true);
        List<RawImage> imgs = new List<RawImage>();
        imgs.Add(rawimg);
        imgs.AddRange(childtrs);
        for (int i = 0; i < imgs.Count; i++)
        {
            if (imgs[i] == null)
                continue;
            string assetpath = AssetDatabase.GetAssetPath(imgs[i].texture);
            if (assetpath.Contains("unity_builtin_extra") || assetpath.Contains("UITexture/")) //排除内部自己用的 图集 和 大的背景图 或者 使用rawimage的不替换 如 UIMask 等等 scrollview 用的
            {
                continue;
            }
            Debug.Log(assetpath);
            Sprite tex = AssetDatabase.LoadAssetAtPath(assetpath, typeof(Sprite)) as Sprite;
            GameObject imgobj = imgs[i].gameObject;
            DestroyImmediate(imgs[i]);
            Image rwimg = imgobj.AddComponent<Image>();
            rwimg.sprite = tex;//Sprite.Create(tex as Texture2D,new Rect(0,0,tex.width,tex.height),Vector2.zero);
        }
    }

    /// <summary>
    /// 移除多余的事件接受
    /// </summary>
    [MenuItem("Assets/去除多余的Raycast Target")]
    static void RemovemoreRaycast()
    {
        Transform canvastr = GameObject.Find("DontDestroy/UICamera/Canvas").transform;
        GameObject obj = Selection.activeGameObject;
        GameObject view = GameObject.Instantiate(obj, canvastr);
        view.name = obj.name;

        RawImage rawimg = view.GetComponent<RawImage>();
        RawImage[] childtrs = view.GetComponentsInChildren<RawImage>(true);
        List<RawImage> rawimgs = new List<RawImage>();
        rawimgs.Add(rawimg);
        rawimgs.AddRange(childtrs);

        Image img = view.GetComponent<Image>();
        Image[] childtrimgs = view.GetComponentsInChildren<Image>(true);
        List<Image> imgs = new List<Image>();
        imgs.Add(img);
        imgs.AddRange(childtrimgs);

        Text text = view.GetComponent<Text>();
        Text[] childtrTexts = view.GetComponentsInChildren<Text>(true);
        List<Text> Texts = new List<Text>();
        Texts.Add(text);
        Texts.AddRange(childtrTexts);

        foreach (var item in rawimgs)
        {
            if (item == null)
            {
                continue;
            }
            if (item.GetComponent<SButtonUGUI>() || item.GetComponent<CameraOrRootSizeScale>())
            {
                item.raycastTarget = true;
            }
            else
            {
                item.raycastTarget = false;
            }
        }

        foreach (var item in imgs)
        {
            if (item == null)
            {
                continue;
            }
            if (item.GetComponent<SButtonUGUI>() || item.GetComponent<InputField>() || item.GetComponent<Mask>())
            {
                item.raycastTarget = true;
            }
            else
            {
                item.raycastTarget = false;
            }
        }

        foreach (var item in Texts)
        {
            if (item == null)
            {
                continue;
            }
            if (item.GetComponent<SButtonUGUI>())
            {
                item.raycastTarget = true;
            }
            else
            {
                item.raycastTarget = false;
            }
        }
    }
}
