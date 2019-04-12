using UnityEditor;
using System.IO;
using UnityEngine;

public class AssetsMenuExtention
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Assets/Clear Cache")]
    static void ClearAssetBundlesCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Successfully cleared the cache.");
        }
        else
        {
            Debug.Log("Cache is being used.");
        }
    }
}