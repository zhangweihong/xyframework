using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(XLuaMonoBehaviour))]
public class XLuaMonoBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        XLuaMonoBehaviour xluamono = target as XLuaMonoBehaviour;
        GUILayout.BeginVertical();
        GUILayout.Label("说明：\n自动检索当前物体中含有SButtonUGUI,Text(想要自动获取什么可以自行添加在GetAllSobject函数中)等组件\n并自动命名Lua的对象名且自动写入对应的Lua脚本中(见Lua脚本中的injectobject函数) \n如果Serialize Objects列表中已存在检索的物体则该物体将不会执行操作");
        if (GUILayout.Button("自动搜索并注入Lua"))
        {
            if (!string.IsNullOrEmpty(xluamono.LuaRelativePath))
            {
                List<SerializeObject> sobjls = GetAllSobject(xluamono);
                List<SerializeObject> newsobjls = new List<SerializeObject>();
                newsobjls.AddRange(xluamono.SerializeObjects);
                newsobjls.AddRange(sobjls);
                xluamono.SerializeObjects = newsobjls.ToArray(); //重新赋值
                InjectToLua(sobjls, xluamono.LuaRelativePath);//注入
            }
            else
            {
                Debug.LogError("无法获取lua脚本路径 请检查LuaRelativePath是否正确");
            }
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 自动注入lua的文件中
    /// </summary>
    /// <param name="sobjls"></param>
    /// <param name="relapath"></param>
    public void InjectToLua(List<SerializeObject> sobjls, string relapath)
    {
        if (sobjls != null && sobjls.Count > 0)
        {
            string luapath = Path.Combine("Assets/Res", relapath).ToForwardSlash();
            string luaobjname = Path.GetFileNameWithoutExtension(luapath).Replace(".lua", "");
            TextAsset appAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(luapath, typeof(TextAsset)) as TextAsset;
            string luastr = appAsset.text;
            int startindex = luastr.IndexOf("--s") + 3;
            int endindex = luastr.IndexOf("--e") - 3;
            int len = endindex - startindex;
            luastr = luastr.Remove(startindex, len);
            string newstr = "\n";
            for (int i = 0; i < sobjls.Count; i++)
            {
                if (sobjls[i].Type == typeof(SButtonUGUI))
                {
                    newstr += "\t" + luaobjname + "." + sobjls[i].Name + "_but = " + luaobjname + "." + sobjls[i].Name + ":GetComponent(typeof(SButtonUGUI))\n";
                    newstr += "\t" + luaobjname + "." + sobjls[i].Name + "_but:SetClick(" + luaobjname + ".clickfun)\n";
                }
                else if (sobjls[i].Type == typeof(Text))
                {
                    newstr += "\t" + luaobjname + "." + sobjls[i].Name + "_label = " + luaobjname + "." + sobjls[i].Name + ":GetComponent(typeof(Text))\n";
                }
            }
            luastr = luastr.Insert(startindex, newstr);
            File.WriteAllText(Path.Combine(Application.dataPath, "Res/" + relapath), luastr);
        }
        else
        {
            Debug.LogWarning("没有可注入的东西");
        }
    }

    /// <summary>
    /// 获取常用组件
    /// </summary>
    /// <param name="xluamono"></param>
    /// <returns></returns>
    public List<SerializeObject> GetAllSobject(XLuaMonoBehaviour xluamono)
    {
        GameObject gameobject = xluamono.gameObject;
        List<SerializeObject> olobjls = Util.ArryConvertList<SerializeObject>(xluamono.SerializeObjects);
        List<SerializeObject> sobjectls = new List<SerializeObject>();

        SButtonUGUI parentSButton = gameobject.GetComponent<SButtonUGUI>();
        SButtonUGUI[] sButtonBarry = gameobject.GetComponentsInChildren<SButtonUGUI>(true);
        Text[] textBarry = gameobject.GetComponentsInChildren<Text>(true);

        // UGUIInputField[] uInputarry = gameobject.GetComponentsInChildren<UGUIInputField>(true);
        // UGUIDropdown[] udropdwonarry = gameobject.GetComponentsInChildren<UGUIDropdown>(true);
        // UGUISlider[] usliderarry = gameobject.GetComponentsInChildren<UGUISlider>(true);
        // UGUIScrollRect[] uscrollrectarry = gameobject.GetComponentsInChildren<UGUIScrollRect>(true);
        if (parentSButton != null)
        {
            SerializeObject so = new SerializeObject();
            so.Name = parentSButton.gameObject.name.ToLower();
            so.Value = parentSButton.gameObject;
            if (olobjls.Find((obj) => obj.Value == parentSButton) == null)
            {
                sobjectls.Add(so);
            }
        }
        for (int i = 0; i < sButtonBarry.Length; i++)
        {
            if (sButtonBarry[i] == null)
            {
                continue;
            }
            SerializeObject _sbo = new SerializeObject();
            _sbo.Name = sButtonBarry[i].gameObject.name.ToLower();
            _sbo.Value = sButtonBarry[i].gameObject;
            _sbo.Type = typeof(SButtonUGUI);
            if (olobjls.Find((obj) => obj.Value == _sbo.Value) == null)
            {
                sobjectls.Add(_sbo);
            }
        }

        for (int i = 0; i < textBarry.Length; i++)
        {
            if (textBarry[i] == null)
            {
                continue;
            }
            SerializeObject _sto = new SerializeObject();
            string pname = "";
            if (textBarry[i].transform.parent != null)
            {
                pname = textBarry[i].transform.parent.gameObject.name;
                _sto.Name = pname + "_" + textBarry[i].gameObject.name;
            }
            else
            {
                _sto.Name = textBarry[i].gameObject.name;
            }
            _sto.Name = _sto.Name.ToLower();
            _sto.Value = textBarry[i].gameObject;
            _sto.Type = typeof(Text);
            if (olobjls.Find((obj) => obj.Value == _sto.Value) == null)
            {
                sobjectls.Add(_sto);
            }
        }
        return sobjectls;
    }
}
