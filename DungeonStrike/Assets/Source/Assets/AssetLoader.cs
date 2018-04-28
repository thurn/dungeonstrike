using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using UnityEngine;

namespace DungeonStrike.Source.Assets
{
    /// <summary>
    /// Handles loading named assets from Unity.
    /// </summary>
    public sealed class AssetLoader : Service
    {
        /// <summary>
        /// Loads assets by name.
        /// </summary>
        /// <param name="assetNames">List of asset name strings.</param>
        /// <returns>Asychronous task which will be resolved with an 'AssetRefs' object. The fields
        /// corresponding to the requested assets are guaranteed to be populated.</returns>
        public AssetRefs GetAssets()
        {
            var assets = Root.transform.Find("Assets");
            return assets.GetComponent<AssetRefs>();
        }
    }
}