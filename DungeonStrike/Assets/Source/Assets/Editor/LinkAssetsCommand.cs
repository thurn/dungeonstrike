using UnityEngine;
using UnityEditor;

namespace DungeonStrike.Source.Assets.Editor
{
    public class LinkAssetsCommand
    {
        [MenuItem("Tools/Link Assets")]
        public static void LinkAssets()
        {
            var assetRefs = GameObject.Find("Root/Assets").GetComponent<AssetRefs>();
            AssetLinker.LinkAssets(assetRefs);
        }
    }
}
