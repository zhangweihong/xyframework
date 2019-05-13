using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using Excel;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

public class ExcelExportDataEditor : Editor
{
    /*
    /// <summary>
    /// 暂时废弃 导出C#代码结构(当时想结合ILRuntime,最后选择了xlua 鹅厂的比较放心些)
    /// </summary>
    [MenuItem("ExcelToData/生成表结构")]
    static void ExportCode()
    {
        ExportAllExcelToCodeStruct();
    }
    /// <summary>
    /// 暂时废弃 保存C#结构数据为scriptdata (当时想结合ILRuntime,最后选择了xlua 鹅厂的比较放心些)
    /// </summary>
    [MenuItem("ExcelToData/生成表数据并保存")]
    static void ExportData()
    {
        ExportAllExcelData();
    }
 */
    [MenuItem("XYFramework/ExcelToData/生成Json脚本")]
    static void ExportJsonData()
    {
        ExportAllExcelToJson();
    }

    [MenuItem("XYFramework/ExcelToData/生成lua脚本")]
    static void ExportLuaData()
    {
        ExportAllExcelToLua();
    }

    public static void ExportAllExcelToJson()
    {
        string excelRoot = Application.dataPath + "/../Excel/";
        if (!Directory.Exists(excelRoot))
        {
            Debug.LogError(excelRoot + " 不存在！！！请检查路径是否有问题！！！");
            return;
        }

        string[] files = Directory.GetFiles(excelRoot, "*.xlsx", SearchOption.AllDirectories);
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i].Contains("~"))
            {
                continue;
            }
            EditorUtility.DisplayProgressBar("导表数据并保存", files[i], i / (float)length);
            ExportJson(files[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void ExportJson(string ExcelPath)
    {
        string jsonname = Path.GetFileNameWithoutExtension(ExcelPath);
        DataTableCollection result = _ReadExcel(ExcelPath);
        CreateJson(result, jsonname);
    }

    private static void CreateJson(DataTableCollection result, string name)
    {
        string script = "[";
        DataRow proRow = result[0].Rows[0];
        DataRow typeRow = result[0].Rows[1];
        DataRow descRow = result[0].Rows[2];

        int columns = result[0].Columns.Count;
        int rows = result[0].Rows.Count;
        int count = 0;
        for (int i = 3; i < rows; i++)
        {
            DataRow curRow = result[0].Rows[i];
            string dhchar = ",\n";
            if (i == rows - 1)
            {
                dhchar = "";
            }
            if (string.IsNullOrEmpty(curRow[0].ToString()))
            {
                continue;
            }
            count++;
            string idfenhaostr = "\"";
            if (typeRow[0].ToString().Contains("i"))
            {
                idfenhaostr = "";
            }
            else
            {
                idfenhaostr = "\"";
            }
            string headStr = "{";
            for (int j = 0; j < columns; j++)
            {
                string pro = "\"" + proRow[j].ToString() + "\"";
                string cur = curRow[j].ToString();
                string type = typeRow[j].ToString();

                if (string.IsNullOrEmpty(cur))
                {
                    cur = "";
                }
                cur = cur.Replace("\"", "\\\"");
                string fenhaostr = "\"";
                if (type.Contains("i"))
                {
                    fenhaostr = "";
                }
                if (type.Contains("a"))
                {
                    string[] sArry = cur.Split(';');
                    string newCur = string.Empty;
                    if (cur.Contains(";"))
                    {
                        for (int k = 0; k < sArry.Length; k++)
                        {
                            if (k == 0)
                            {
                                newCur += ("[" + fenhaostr + sArry[k] + fenhaostr);
                            }
                            else if (k == sArry.Length - 1)
                            {
                                newCur += ("," + fenhaostr + sArry[k] + fenhaostr + "]");
                            }
                            else
                            {
                                newCur += ("," + fenhaostr + sArry[k] + fenhaostr);
                            }
                        }
                    }
                    else
                    {
                        if (cur.Length > 0)
                        {
                            newCur += "[" + fenhaostr + cur + fenhaostr + "]";
                        }
                        else
                        {
                            newCur += "[]";
                        }

                    }

                    if (j == 0)
                    {
                        script += (headStr + (pro + ": " + newCur + ", "));
                    }
                    else if (j == columns - 1)
                    {
                        script += (pro + " : " + newCur + "}" + dhchar);
                    }
                    else
                    {
                        script += (pro + " : " + newCur + ", ");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(cur) && type.Contains("i"))
                    {
                        cur = "-1";
                    }
                    if (j == 0)
                    {
                        script += (headStr + (pro + " : " + fenhaostr + cur + fenhaostr + ", "));
                    }
                    else if (j == columns - 1)
                    {
                        script += (pro + " : " + fenhaostr + cur + fenhaostr + "}" + dhchar);
                    }
                    else
                    {
                        script += (pro + " : " + fenhaostr + cur + fenhaostr + ", ");
                    }
                }
            }
        }
        script += "]";
        name = name.ToKeyUrl();
        string path = Path.Combine(Application.dataPath, "ExcelTool/json/" + name + ".json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string dicname = Path.GetDirectoryName(path);
        if (!Directory.Exists(dicname))
        {
            Directory.CreateDirectory(dicname);
        }
        StreamWriter sw = File.CreateText(path);
        sw.Write(script);
        sw.Close();
        sw.Dispose();
        AssetDatabase.Refresh();
    }


    public static void ExportAllExcelToLua()
    {
        string excelRoot = Application.dataPath + "/../Excel/";
        if (!Directory.Exists(excelRoot))
        {
            Debug.LogError(excelRoot + " 不存在！！！请检查路径是否有问题！！！");
            return;
        }

        string[] files = Directory.GetFiles(excelRoot, "*.xlsx", SearchOption.AllDirectories);
        List<string> classes = new List<string>();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i].Contains("~"))
            {
                continue;
            }
            EditorUtility.DisplayProgressBar("导表数据并保存", files[i], i / (float)length);
            classes.Add(ExportLua(files[i]));
        }
        CreateLuaConfigManager(classes);
        EditorUtility.ClearProgressBar();
    }
    /* 
        /// <summary>
        /// 导出表结构代码
        /// </summary>
        static void ExportAllExcelToCodeStruct()
        {
            string excelRoot = Application.dataPath + "/../Excel/";
            if (!Directory.Exists(excelRoot))
            {
                Debug.LogError(excelRoot + " 不存在！！！请检查路径是否有问题！！！");
                return;
            }

            string[] files = Directory.GetFiles(excelRoot, "*.xlsx", SearchOption.AllDirectories);
            List<string> classes = new List<string>();
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                if (files[i].Contains("~"))
                {
                    continue;
                }
                EditorUtility.DisplayProgressBar("导数据结构", files[i], i / (float)length);
                StartCreateStruct(files[i]);
                classes.Add(Path.GetFileNameWithoutExtension(files[i]));
            }
            CreateDataManager(classes);
            EditorUtility.ClearProgressBar();
        }


        static void CreateDataManager(List<string> classNames)
        {
            string rootPath = Path.Combine(Application.dataPath, "Scripts/Framework/Manager/DataManager.cs");
            if (File.Exists(rootPath))
            {
                File.Delete(rootPath);
            }
            StreamWriter sw = File.CreateText(rootPath);
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using System.Collections;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using System;");
            sw.WriteLine("");
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// 代码自动生成");
            sw.WriteLine("/// </summary>");
            sw.WriteLine("public class DataMagager : Singleton<DataMagager>");
            sw.WriteLine("{");
            sw.WriteLine("\t/// <summary>");
            sw.WriteLine("\t/// 初始化");
            sw.WriteLine("\t/// </summary>");
            sw.WriteLine("\tpublic void Init()");
            sw.WriteLine("\t{");
            sw.WriteLine("\t\tLoadData();");
            sw.WriteLine("\t}");
            sw.WriteLine("");
            sw.WriteLine("\t/// <summary>");
            sw.WriteLine("\t/// 加载数据");
            sw.WriteLine("\t/// </summary>");
            sw.WriteLine("\tpublic void LoadData()");
            sw.WriteLine("\t{");
            for (int i = 0; i < classNames.Count; i++)
            {

                sw.WriteLine("\t\t" + classNames[i] + "Data.I = GetData<" + classNames[i] + "Data>(\"Data\", \"" + classNames[i] + "\");");
                sw.WriteLine("\t\t" + classNames[i] + "Data.I.Init();");
            }
            sw.WriteLine("\t}");
            sw.WriteLine("");
            sw.WriteLine("\t/// <summary>");
            sw.WriteLine("\t/// 获取本地表数据");
            sw.WriteLine("\t/// 永驻内存");
            sw.WriteLine("\t/// </summary>");
            sw.WriteLine("\t/// <typeparam name=\"T\"></typeparam>");
            sw.WriteLine("\t/// <param name=\"path\"></param>");
            sw.WriteLine("\t/// <param name=\"name\"></param>");
            sw.WriteLine("\t/// <returns></returns>");
            sw.WriteLine("\tprivate T GetData<T>(string path, string name)");
            sw.WriteLine("\t{");
            sw.WriteLine("\t\tT data = default(T);");
            sw.WriteLine("\t\tResourcesManager.I.Load(path, name, (object _data) =>");
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\tdata = (T)_data;");
            sw.WriteLine("\t\t}, AssetType.Data, false, true);");
            sw.WriteLine("\t\treturn data;");
            sw.WriteLine("\t}");
            sw.WriteLine("}");
            sw.Close();
            sw.Dispose();
        }

        /// <summary>
        /// 导出excel表中的数据
        /// </summary>
        static void ExportAllExcelData()
        {
            string excelRoot = Application.dataPath + "/../Excel/";
            if (!Directory.Exists(excelRoot))
            {
                Debug.LogError(excelRoot + " 不存在！！！请检查路径是否有问题！！！");
                return;
            }

            string[] files = Directory.GetFiles(excelRoot, "*.xlsx", SearchOption.AllDirectories);
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                if (files[i].Contains("~"))
                {
                    continue;
                }
                EditorUtility.DisplayProgressBar("导表数据并保存", files[i], i / (float)length);
                StartExportData(files[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 自动生成代码结构
        /// </summary>
        /// <param name="className"></param>
        /// <param name="typeRow"></param>
        /// <param name="typenameRow"></param>
        /// <param name="descRow"></param>
        /// <param name="columns"></param>
        static void CreateDataStruct(string className, DataRow typeRow, DataRow typenameRow, DataRow descRow, int columns)
        {
            string rootPath = Application.dataPath + "/Scripts/Framework/ExcelTool/Data/Code/";
            string classPath = rootPath + className + "Data.cs";

            if (File.Exists(classPath))
            {
                File.Delete(classPath);
            }

            StreamWriter classSw = File.CreateText(classPath);

            classSw.WriteLine("using UnityEngine;");
            classSw.WriteLine("using System.Collections;");
            classSw.WriteLine("using System.Collections.Generic;");
            classSw.WriteLine("using System;");
            classSw.WriteLine("");
            classSw.WriteLine("[System.Serializable]");
            classSw.WriteLine("public class " + className + "Data : BaseData");
            classSw.WriteLine("{");
            classSw.WriteLine("\tpublic static " + className + "Data I;");
            classSw.WriteLine("\tpublic List<" + className + "Vo> " + className + "Vos = new List<" + className + "Vo>();");
            classSw.WriteLine("\tprivate Dictionary<int, " + className + "Vo> _dic = new Dictionary<int, " + className + "Vo>();");
            classSw.WriteLine("");
            classSw.WriteLine("\tpublic override void Init()");
            classSw.WriteLine("\t{");
            classSw.WriteLine("\t\tfor (int i = 0; i < " + className + "Vos.Count; i++)");
            classSw.WriteLine("\t\t{");
            classSw.WriteLine("\t\t\t_dic[" + className + "Vos[i].id] = " + className + "Vos[i];");
            classSw.WriteLine("\t\t}");
            classSw.WriteLine("\t}");
            classSw.WriteLine("");
            classSw.WriteLine("\tpublic " + className + "Vo Find(int id)");
            classSw.WriteLine("\t{");
            classSw.WriteLine("\t\t" + className + "Vo vo = null;");
            classSw.WriteLine("\t\tif (_dic.TryGetValue(id, out vo))");
            classSw.WriteLine("\t\t{");
            classSw.WriteLine("\t\t}");
            classSw.WriteLine("\t\telse");
            classSw.WriteLine("\t\t{");
            classSw.WriteLine("\t\t\tDebug.LogError(\"" + className + " 数据表中没有 \" + id + \" 的数据\");");
            classSw.WriteLine("\t\t}");
            classSw.WriteLine("\t\treturn vo;");
            classSw.WriteLine("\t}");
            classSw.WriteLine("}");
            classSw.WriteLine("");
            classSw.WriteLine("[System.Serializable]");
            classSw.WriteLine("public class " + className + "Vo");
            classSw.WriteLine("{");
            for (int i = 0; i < columns; i++)
            {
                string _type = GetKeyType(typeRow[i].ToString());
                string _name = typenameRow[i].ToString();
                string _desc = descRow[i].ToString();
                classSw.WriteLine("\t/// <summary>");
                classSw.WriteLine("\t/// " + _desc);
                classSw.WriteLine("\t/// </summary>");
                classSw.WriteLine("\tpublic " + _type + " " + _name + ";");
                classSw.WriteLine("");
            }
            classSw.WriteLine("}");
            classSw.Close();
            classSw.Dispose();
        }

        /// <summary>
        /// 使用反射序列化代码
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="className"></param>
        static void SerializeData(DataTableCollection tables, string className)
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            //HeroData heroData = ScriptableObject.CreateInstance<HeroData>();
            ScriptableObject commonData = ScriptableObject.CreateInstance(className + "Data");
            object commonvos = commonData.GetType().GetField(className + "Vos").GetValue(commonData);
            MethodInfo commonvosAddMInfo = commonvos.GetType().GetMethod("Add");

            int columns = tables[0].Columns.Count;
            int rows = tables[0].Rows.Count;
            DataRow typeRow = tables[0].Rows[1];
            for (int i = 3; i < rows; i++)
            {
                DataRow curRow = tables[0].Rows[i];
                object vo = assembly.CreateInstance(className + "Vo");
                Type t = vo.GetType();
                FieldInfo[] infos = t.GetFields();
                for (int j = 0; j < columns; j++)
                {
                    string value = curRow[j].ToString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    switch (typeRow[j].ToString())
                    {
                        case "b":
                            infos[j].SetValue(vo, Byte.Parse(value));
                            break;
                        case "i16":
                            infos[j].SetValue(vo, Int16.Parse(value));
                            break;
                        case "ui16":
                            infos[j].SetValue(vo, UInt16.Parse(value));
                            break;
                        case "i":
                            infos[j].SetValue(vo, Int32.Parse(value));
                            break;
                        case "ui":
                            infos[j].SetValue(vo, UInt32.Parse(value));
                            break;
                        case "i64":
                            infos[j].SetValue(vo, Int64.Parse(value));
                            break;
                        case "ui64":
                            infos[j].SetValue(vo, UInt64.Parse(value));
                            break;
                        case "ab":
                            infos[j].SetValue(vo, GetByteList(value));
                            break;
                        case "ai16":
                            infos[j].SetValue(vo, GetInt16List(value));
                            break;
                        case "aui16":
                            infos[j].SetValue(vo, GetUInt16List(value));
                            break;
                        case "ai":
                            infos[j].SetValue(vo, GetInt32List(value));
                            break;
                        case "aui":
                            infos[j].SetValue(vo, GetUInt32List(value));
                            break;
                        case "ai64":
                            infos[j].SetValue(vo, GetInt64List(value));
                            break;
                        case "aui64":
                            infos[j].SetValue(vo, GetUInt64List(value));
                            break;
                        case "s":
                            infos[j].SetValue(vo, value);
                            break;
                        case "as":
                            infos[j].SetValue(vo, GetStringList(value));
                            break;
                    }
                }
                commonvosAddMInfo.Invoke(commonvos, new object[] { vo });
            }
            string rootPath = Application.dataPath + "/Scripts/Framework/ExcelTool/Data/";
            string serilaizeDataAbsPath = rootPath + "SerializeData/" + className + ".asset";
            string serilaizeDataAssetPath = "Assets/Scripts/Framework/ExcelTool/Data/SerializeData/" + className + ".asset";
            string resourcesDicPath = Application.dataPath + "/Resources/Data/";
            if (File.Exists(serilaizeDataAbsPath))
            {
                File.Delete(serilaizeDataAbsPath);
            }

            AssetDatabase.CreateAsset(commonData, serilaizeDataAssetPath);
            if (Directory.Exists(resourcesDicPath))
            {
                File.Copy(serilaizeDataAbsPath, resourcesDicPath + className + ".asset", true);
            }
            AssetDatabase.Refresh();
        }
    */
    static List<string> GetStringList(string str)
    {
        List<string> lst = new List<string>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(valueArry[i]);
        }

        return lst;
    }

    static List<Byte> GetByteList(string str)
    {
        List<Byte> lst = new List<Byte>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(Byte.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<Int16> GetInt16List(string str)
    {
        List<Int16> lst = new List<Int16>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(Int16.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<UInt16> GetUInt16List(string str)
    {
        List<UInt16> lst = new List<UInt16>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(UInt16.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<Int32> GetInt32List(string str)
    {
        List<Int32> lst = new List<Int32>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(Int32.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<UInt32> GetUInt32List(string str)
    {
        List<UInt32> lst = new List<UInt32>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(UInt32.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<Int64> GetInt64List(string str)
    {
        List<Int64> lst = new List<Int64>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(Int64.Parse(valueArry[i]));
        }

        return lst;
    }

    static List<UInt64> GetUInt64List(string str)
    {
        List<UInt64> lst = new List<UInt64>();
        string[] valueArry = str.Split(';');
        for (int i = 0; i < valueArry.Length; i++)
        {
            lst.Add(UInt64.Parse(valueArry[i]));
        }

        return lst;
    }

    static string GetKeyType(string key)
    {
        string type = "string";
        switch (key)
        {
            case "b":
                type = "Byte";
                break;
            case "i16":
                type = "Int16";
                break;
            case "ui16":
                type = "UInt16";
                break;
            case "i":
                type = "Int32";
                break;
            case "ui":
                type = "UInt32";
                break;
            case "i64":
                type = "Int64";
                break;
            case "ui64":
                type = "UInt64";
                break;
            case "ab":
                type = "List<Byte>";
                break;
            case "ai16":
                type = "List<Int16>";
                break;
            case "aui16":
                type = "List<UInt16>";
                break;
            case "ai":
                type = "List<Int32>";
                break;
            case "aui":
                type = "List<UInt32>";
                break;
            case "ai64":
                type = "List<Int64>";
                break;
            case "aui64":
                type = "List<UInt64>";
                break;
            case "s":
                type = "string";
                break;
            case "as":
                type = "List<string>";
                break;
        }

        return type;
    }
    /*
        static void StartCreateStruct(string ExcelPath)
        {
            string excelName = Path.GetFileNameWithoutExtension(ExcelPath);
            DataTableCollection result = _ReadExcel(ExcelPath);
            DataRow typenameameRow = result[0].Rows[0];
            DataRow typeRow = result[0].Rows[1];
            DataRow descRow = result[0].Rows[2];
            int columns = result[0].Columns.Count;

            CreateDataStruct(excelName, typeRow, typenameameRow, descRow, columns);
            AssetDatabase.Refresh();
        }


        static void StartExportData(string ExcelPath)
        {
            string excelName = Path.GetFileNameWithoutExtension(ExcelPath);
            DataTableCollection result = _ReadExcel(ExcelPath);
            SerializeData(result, excelName);
        }
     */
    static DataTableCollection _ReadExcel(string ExcelPath)
    {
        FileStream stream = File.Open(ExcelPath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        int columns = result.Tables[0].Columns.Count;
        int rows = result.Tables[0].Rows.Count;

        if (rows < 2 || columns < 1)
        {
            Debug.LogError(ExcelPath + " 数据表有问题，请及时检查");
            return null;
        }
        return result.Tables;
    }

    static string ExportLua(string ExcelPath)
    {
        string excelName = Path.GetFileNameWithoutExtension(ExcelPath);
        DataTableCollection result = _ReadExcel(ExcelPath);
        CreateLua(result, excelName);
        return excelName;
    }

    static void CreateLua(DataTableCollection result, string name)
    {
        string script = "local data = {} \n";
        DataRow proRow = result[0].Rows[0];
        DataRow typeRow = result[0].Rows[1];
        DataRow descRow = result[0].Rows[2];

        int columns = result[0].Columns.Count;
        int rows = result[0].Rows.Count;
        int count = 0;
        for (int i = 3; i < rows; i++)
        {
            DataRow curRow = result[0].Rows[i];
            if (string.IsNullOrEmpty(curRow[0].ToString()))
            {
                continue;
            }
            count++;
            string idfenhaostr = "\"";
            if (typeRow[0].ToString().Contains("i"))
            {
                idfenhaostr = "";
            }
            else
            {
                idfenhaostr = "\"";
            }
            string headStr = "data[" + idfenhaostr + curRow[0] + idfenhaostr + "] = { ";
            for (int j = 0; j < columns; j++)
            {
                string pro = proRow[j].ToString();
                string cur = curRow[j].ToString();
                string type = typeRow[j].ToString();

                if (string.IsNullOrEmpty(cur))
                {
                    cur = "";
                }
                cur = cur.Replace("\"", "\\\"");
                string fenhaostr = "\"";
                if (type.Contains("i"))
                {
                    fenhaostr = "";
                }
                if (type.Contains("a"))
                {
                    string[] sArry = cur.Split(';');
                    string newCur = string.Empty;
                    if (cur.Contains(";"))
                    {
                        for (int k = 0; k < sArry.Length; k++)
                        {
                            if (k == 0)
                            {
                                newCur += ("{" + fenhaostr + sArry[k] + fenhaostr);
                            }
                            else if (k == sArry.Length - 1)
                            {
                                newCur += ("," + fenhaostr + sArry[k] + fenhaostr + "}");
                            }
                            else
                            {
                                newCur += ("," + fenhaostr + sArry[k] + fenhaostr);
                            }
                        }
                    }
                    else
                    {
                        if (cur.Length > 0)
                        {
                            newCur += "{" + fenhaostr + cur + fenhaostr + "}";
                        }
                        else
                        {
                            newCur += "{}";
                        }

                    }

                    if (j == 0)
                    {
                        script += (headStr + (pro + " =" + newCur + ", "));
                    }
                    else if (j == columns - 1)
                    {
                        script += (pro + " = " + newCur + "}\n");
                    }
                    else
                    {
                        script += (pro + " = " + newCur + ", ");
                    }


                }
                else
                {
                    if (string.IsNullOrEmpty(cur) && type.Contains("i"))
                    {
                        cur = "-1";
                    }
                    if (j == 0)
                    {
                        script += (headStr + (pro + " =" + fenhaostr + cur + fenhaostr + ", "));
                    }
                    else if (j == columns - 1)
                    {
                        script += (pro + " = " + fenhaostr + cur + fenhaostr + "}\n");
                    }
                    else
                    {
                        script += (pro + " = " + fenhaostr + cur + fenhaostr + ", ");
                    }
                }


            }
        }
        script += "data.count = " + count + "\n";
        script += "return data";
        name = name.ToKeyUrl();
        string path = Path.Combine(Application.dataPath, "Res/Lua/data/" + name + ".lua.txt");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        StreamWriter sw = File.CreateText(path);
        sw.Write(script);
        sw.Close();
        sw.Dispose();
        AssetDatabase.Refresh();
    }

    static void CreateLuaConfigManager(List<string> configs)
    {
        string path = Path.Combine(Application.dataPath, "Res/Lua/app/config.lua.txt");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        StreamWriter sw = File.CreateText(path);
        sw.WriteLine("--这是一个自动生成的类 数据表的承载类");
        sw.WriteLine("local config = {}");
        sw.WriteLine("");
        sw.WriteLine("--加载所有的表数据");
        sw.WriteLine("function config.loaddata()");
        for (int i = 0; i < configs.Count; i++)
        {
            string name = configs[i].ToKeyUrl();
            sw.WriteLine("\tconfig[\'" + name + "\'] = require(\"Lua/data/" + name + "\")");
        }
        sw.WriteLine("end");
        sw.WriteLine("");
        sw.WriteLine("--找到整张表通过名字 是个table");
        sw.WriteLine("function config.getdata(tablename)");
        sw.WriteLine("\tlocal t = config[tablename]");
        sw.WriteLine("\treturn t");
        sw.WriteLine("end");
        sw.WriteLine("");
        sw.WriteLine("--找到一条数据 是个table");
        sw.WriteLine("function config.finddatawithid(tablename,id)");
        sw.WriteLine("\tlocal t = config.getdata(tablename)[id]");
        sw.WriteLine("\treturn t");
        sw.WriteLine("end");
        sw.WriteLine("");
        sw.WriteLine("--找到一个值 主要是用这个 是个具体value");
        sw.WriteLine("function config.finddatawithidkey(tablename,id,key)");
        sw.WriteLine("\tlocal t = config.finddatawithid(tablename,id)[key]");
        sw.WriteLine("\treturn t");
        sw.WriteLine("end");
        sw.WriteLine("return config");
        sw.Close();
        sw.Dispose();
        AssetDatabase.Refresh();
    }


}
