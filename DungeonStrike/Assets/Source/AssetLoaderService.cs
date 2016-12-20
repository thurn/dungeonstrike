using System;
using UnityEngine;
using System.Collections.Generic;
using AssetBundles;
using Object = UnityEngine.Object;

namespace DungeonStrike
{
    public interface IAssetLoader
    {
        void InstantiateObject<T>(string bundleName, string assetName, System.Action<T> onLoadCallback)
            where T : Object;
    }

    public class AssetLoaderService : MonoBehaviour, IAssetLoader
    {
        private static AssetLoaderService _instance;

        public static AssetLoaderService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<AssetLoaderService>()); }
        }

        private bool _initialized = false;
        private readonly List<System.Action<IAssetLoader>> _loadAssetBlocks = new List<Action<IAssetLoader>>();

        void Awake()
        {
            Debug.Log("AssetLoaderService Start()");
            StartCoroutine(Initialize());
        }

        // Initialize the downloading url and AssetBundleManifest object.
        private IEnumerator<Coroutine> Initialize()
        {
            Debug.Log("Starting asset Initialize()");
            // Don't destroy this gameObject as we depend on it to run the loading script.
            DontDestroyOnLoad(gameObject);

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            AssetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/" + Utility.GetPlatformName() + "/");
            var request = AssetBundleManager.Initialize();
            if (request != null)
                yield return StartCoroutine(request);

            Debug.Log("Done initializing AssetManager.");
            _initialized = true;
            foreach (var actionBlock in _loadAssetBlocks)
            {
                actionBlock(this);
            }
        }

        public void LoadAssetsWithBlock(System.Action<IAssetLoader> actionBlock)
        {
            if (_initialized)
            {
                actionBlock(this);
            }
            else
            {
                _loadAssetBlocks.Add(actionBlock);
            }
        }

        public void InstantiateObject<T>(string bundleName, string assetName, System.Action<T> onLoadCallback)
            where T : Object
        {
            Preconditions.CheckState(_initialized,
                "You must wait for AssetLoaderService to be initialized before invoking InstantiateObject()!");
            StartCoroutine(InstantiateObjectAsync<T>(bundleName, assetName, onLoadCallback));
        }

        private IEnumerator<Coroutine> InstantiateObjectAsync<T>(string bundleName, string assetName,
            System.Action<T> onLoadCallback) where T : Object
        {
            var loadRequest = AssetBundleManager.LoadAssetAsync(bundleName, assetName, typeof(T));
            if (loadRequest == null) yield break;
            yield return StartCoroutine(loadRequest);
            var instance = loadRequest.GetAsset<T>();
            if (instance != null)
            {
                onLoadCallback(Instantiate(instance));
            }
        }

    }
}