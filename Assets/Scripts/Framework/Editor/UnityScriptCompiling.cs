using UnityEngine;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
public class UnityScriptCompiling : AssetPostprocessor
{
    [UnityEditor.Callbacks.DidReloadScripts]
    static void AllScriptsReloaded()
    {
        Debug.Log("AllScriptsReloaded");
    }
}

