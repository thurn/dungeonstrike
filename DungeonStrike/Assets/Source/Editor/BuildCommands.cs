using System.IO;
using AssetBundles;
using UnityEditor;
using UnityEngine;

namespace DungeonStrike.Source.Editor
{
    public class BuildCommands
    {
        public static void BuildAssetBundles()
        {
            BuildScript.BuildAssetBundles();
            BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath,
                Utility.AssetBundlesOutputPath));
            AssetDatabase.Refresh();
        }
    }
}
