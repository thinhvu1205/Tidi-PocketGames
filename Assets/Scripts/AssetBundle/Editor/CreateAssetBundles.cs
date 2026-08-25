using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;
using System;
using PLATFORM = BundleHandler.PLATFORM;

public class CreateAssetBundles
{
    [MenuItem("Asset Bundles/Clear Cache")] private static void _ClearCache() => Debug.Log(Caching.ClearCache() ? "Cache cleared!" : "Cache not cleared!!!");
    [MenuItem("Asset Bundles/Clear Prefs")] private static void _ClearPrefs() => PlayerPrefs.DeleteAll();
    [MenuItem("Asset Bundles/Build/Android")] private static void _BuildForAndroid() => _BuildForATargetPlatform(PLATFORM.Android);
    [MenuItem("Asset Bundles/Build/iOS")] private static void _BuildForiOS() => _BuildForATargetPlatform(PLATFORM.iOS);
    [MenuItem("Asset Bundles/Build/WebGL")] private static void _BuildForWebGL() => _BuildForATargetPlatform(PLATFORM.WebGL);
    private static void _BuildForATargetPlatform(PLATFORM _platformType)
    {
        string path = _PrepareRootFolder(_platformType);
        if (!string.IsNullOrEmpty(path)) _BuildCategory(path, _platformType);
    }
    private static string _PrepareRootFolder(PLATFORM _platformType)
    {
        string basePath = BundleHandler.BASE_PATH + _platformType.ToString();
        if (string.IsNullOrEmpty(basePath)) return "";
        if (Directory.Exists(basePath)) Directory.Delete(basePath, true);
        Directory.CreateDirectory(basePath);
        return basePath;
    }
    private static void _BuildCategory(string _path, PLATFORM _platformType)
    {
        BuildTarget aBT = _platformType switch
        {
            PLATFORM.Android => BuildTarget.Android,
            PLATFORM.iOS => BuildTarget.iOS,
            PLATFORM.WebGL => BuildTarget.WebGL,
            _ => throw new NotImplementedException()
        };
        AssetBundleManifest aABM = BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.None, aBT);
        List<string> hashedNames = new();
        FileInfo[] infoFIs = new DirectoryInfo(_path).GetFiles();
        foreach (FileInfo infoFI in infoFIs) _RenameBundle(aABM, AssetDatabase.GetAllAssetBundleNames(), infoFI, hashedNames);
        string categoryPath = _path + "/" + BundleHandler.CATEGORY;
        if (File.Exists(categoryPath)) File.Delete(categoryPath);
        File.WriteAllText(categoryPath, "[" + string.Join(",", hashedNames) + "]");
    }
    private static void _RenameBundle(AssetBundleManifest _manifestABM, string[] _assetBundleNames, FileInfo _infoFI, List<string> _nodes)
    {
        if (!_infoFI.Name.EndsWith(".manifest") && _assetBundleNames.Contains(_infoFI.Name))
        {
            string suffix = BundleHandler.SPLIT + _manifestABM.GetAssetBundleHash(_infoFI.Name).ToString();
            _nodes.Add('"' + _infoFI.Name + suffix + '"');
            File.Move(_infoFI.FullName, _infoFI.FullName + suffix);
        }
    }
}
