using System.Threading.Tasks;
using AssetBundles;
using DungeonStrike.Source.Core;

namespace DungeonStrike.Source.Assets
{
    /// <summary>
    /// Handles loading named assets from Unity AssetBundles.
    /// </summary>
    public sealed class AssetLoader : Service
    {
        protected override async Task OnEnableService()
        {
            AssetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/" + Utility.GetPlatformName() + "/");
            var request = AssetBundleManager.Initialize();

            // Load the Asset Bundle Manifest if we are not in Simulation mode.
            if (request != null)
            {
                await RunOperationAsync(StartCoroutine(request));
            }

            Logger.Log("Done loading AssetBundle manifest");
        }

        /// <summary>
        /// Asynchronously loads an asset from disk.
        /// </summary>
        /// <param name="asset">Reference object describing asset's name and containing bundle.</param>
        /// <typeparam name="T">Type of asset being loaded.</typeparam>
        /// <returns>Asychronous task which will be resolved with a copy of the desired asset.</returns>
        public async Task<T> LoadAsset<T>(AssetReference asset) where T : UnityEngine.Object
        {
            var loadRequest = AssetBundleManager.LoadAssetAsync(asset.BundleName, asset.AssetName, typeof(T));
            await RunOperationAsync(StartCoroutine(loadRequest));

            var instance = loadRequest.GetAsset<T>();
            ErrorHandler.CheckState(instance != null, "Asset not found", asset);
            return Instantiate(instance);
        }
    }
}