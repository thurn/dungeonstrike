using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetBundles;
using DungeonStrike.Source.Core;
using UnityEngine;

namespace DungeonStrike.Source.Assets
{
    /// <summary>
    /// Handles loading named assets from Unity AssetBundles.
    /// </summary>
    public sealed class AssetLoader : Service
    {
        protected override void OnEnableService(Action onStarted)
        {
            StartCoroutine(LoadAssetBundleManifest(onStarted));
        }

        private IEnumerator<Coroutine> LoadAssetBundleManifest(Action onStarted)
        {
            AssetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/" + Utility.GetPlatformName() + "/");
            var request = AssetBundleManager.Initialize();

            // Load the Asset Bundle Manifest if we are not in Simulation mode.
            if (request != null)
            {
                yield return StartCoroutine(request);
            }

            Logger.Log("Done loading AssetBundle manifest");
            onStarted();
        }

        /// <summary>
        /// Asynchronously loads an asset from disk.
        /// </summary>
        /// <param name="assetReference">Reference object describing asset's name and containing bundle.</param>
        /// <typeparam name="T">Type of asset being loaded.</typeparam>
        /// <returns>Asychronous task which will be resolved with a copy of the desired asset.</returns>
        public Task<T> LoadAsset<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var completionSource = new TaskCompletionSource<T>();
            StartCoroutine(LoadAssetAsync(assetReference, completionSource));
            return completionSource.Task;
        }

        /// <summary>
        /// Coroutine logic to load an asset from an Asset Bundle.
        /// </summary>
        private IEnumerator<Coroutine> LoadAssetAsync<T>(AssetReference asset,
            TaskCompletionSource<T> completionSource) where T : UnityEngine.Object
        {
            var loadRequest = AssetBundleManager.LoadAssetAsync(asset.BundleName, asset.AssetName, typeof(T));
            yield return StartCoroutine(loadRequest);
            var instance = loadRequest.GetAsset<T>();
            if (instance == null)
            {
                completionSource.SetException(ErrorHandler.NewException("Asset not found", asset));
            }
            else
            {
                completionSource.SetResult(Instantiate(instance));
            }
        }
    }
}