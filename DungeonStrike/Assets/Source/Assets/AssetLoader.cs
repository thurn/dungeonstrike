using System.Threading.Tasks;
using AssetBundlesFixed;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Assets
{
    /// <summary>
    /// Handles loading named assets from Unity AssetBundles.
    /// </summary>
    public sealed class AssetLoader : Service
    {
        public static readonly Vector3 DefaultPosition = new Vector3(0, -1000, 0);
        private AssetBundleManager _assetBundleManager;

        protected override async Task<Result> OnEnableService()
        {
            _assetBundleManager = gameObject.AddComponent<AssetBundleManager>();
            //_assetBundleManager.SimulateAssetBundleInEditor = true;
            _assetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/" + Utility.GetPlatformName() + "/");
            var request = _assetBundleManager.Initialize();

            // Load the Asset Bundle Manifest if we are not in Simulation mode.
            if (request != null)
            {
                await RunOperationAsync(StartCoroutine(request));
            }

            Logger.Log("Done loading AssetBundle manifest");
            return Result.Success;
        }

        /// <summary>
        /// Asynchronously loads an asset from disk. The asset will be instantiated at <see cref="DefaultPosition"/>,
        /// and must have its position updated once all asset loading is completed.
        /// </summary>
        /// <param name="asset">Reference object describing asset's name and containing bundle.</param>
        /// <typeparam name="T">Type of asset being loaded.</typeparam>
        /// <returns>Asychronous task which will be resolved with a copy of the desired asset.</returns>
        public async Task<T> LoadAsset<T>(AssetReference asset) where T : Object
        {
            var loadRequest = _assetBundleManager.LoadAssetAsync(asset.BundleName, asset.AssetName, typeof(T));
            await RunOperationAsync(StartCoroutine(loadRequest));

            var instance = loadRequest.GetAsset<T>();
            ErrorHandler.CheckState(instance != null, "Asset not found", asset);
            return Instantiate(instance, DefaultPosition, Quaternion.identity, null);
        }
    }
}