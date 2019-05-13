using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ViewInstanceCache : ScriptableObject
{
    public int LangSettingindex = 0;
    public List<ViewInstance> ViewList = new List<ViewInstance>();
}
