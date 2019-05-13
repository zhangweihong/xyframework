using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

/// <summary>
/// 资源导入处理
/// </summary>
public class AssetImportSettingEditor : AssetPostprocessor
{

    private const string modelpathcheck = "Res/ModelTexture";
    void OnPreprocessTexture()
    {
        TextureImporter texImporter = assetImporter as TextureImporter;
        if (texImporter != null)
        {
            if (IsModeltexture(texImporter.assetPath))
            {
                texImporter.textureType = TextureImporterType.Sprite;
                texImporter.npotScale = TextureImporterNPOTScale.ToLarger;
                texImporter.maxTextureSize = 1024;
                texImporter.isReadable = false;
                texImporter.compressionQuality = 100;
                texImporter.ClearPlatformTextureSettings("Standalone");
                texImporter.SetPlatformTextureSettings("Standalone", 1024, TextureImporterFormat.ARGB16, 100, false);
                texImporter.SetPlatformTextureSettings("iPhone", 1024, TextureImporterFormat.RGBA16, 100, false);
                texImporter.SetPlatformTextureSettings("Android", 1024, TextureImporterFormat.RGBA16, 100, false);
            }
        }
    }
    private bool IsModeltexture(string path)
    {
        bool ismodel = false;
        string dicname = System.IO.Path.GetDirectoryName(path);
        dicname = dicname.ToSingleForwardSlash().ToForwardSlash();
        dicname = dicname.Substring(dicname.LastIndexOf("/") + 1);
        switch (dicname)
        {
            case "BotDoll":
            case "BothFeet":
            case "Car":
            case "Crane":
            case "Dinosaur":
            case "Duke":
            case "FlyCar":
            case "ForkLift":
            case "FourFeet":
            case "LCar":
            case "LCarPlus":
            case "Minihuman":
            case "Rhinoceros":
            case "Scene1":
            case "Scene2":
            case "Scene3":
            case "Tank":
            case "WallE":
            case "MiniLCar":
                ismodel = true;
                break;
        }
        return ismodel;
    }

    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        if (modelImporter != null)
        {
            modelImporter.importAnimation = true;
            modelImporter.animationType = ModelImporterAnimationType.Legacy;
        }
    }
}
