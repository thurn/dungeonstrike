using System;
using UnityEngine;
using System.Collections.Generic;
using AssetBundles;
using Object = UnityEngine.Object;

namespace DungeonStrike
{
    public class AssetLoaderService : MonoBehaviour
    {
        interface ILoadRequest
        {
            AssetBundleLoadAssetOperation NewLoadOperation();
            void HandleCompletedLoad(AssetBundleLoadAssetOperation operation);
        }

        class LoadRequest<T> : ILoadRequest where T : Object
        {
            public string BundleName { get; set; }
            public string AssetName { get; set; }
            public Action<T> OnLoadCallback { get; set; }

            public AssetBundleLoadAssetOperation NewLoadOperation()
            {
                return AssetBundleManager.LoadAssetAsync(BundleName, AssetName, typeof(T));
            }

            public void HandleCompletedLoad(AssetBundleLoadAssetOperation operation)
            {
                var instance = operation.GetAsset<T>();
                if (instance != null)
                {
                    OnLoadCallback(Instantiate(instance));
                }
            }
        }

        private static AssetLoaderService _instance;

        public static AssetLoaderService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<AssetLoaderService>()); }
        }

        private bool _initialized = false;
        private HashSet<ILoadRequest> _loadRequests = new HashSet<ILoadRequest>();

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

            foreach (var loadRequest in _loadRequests)
            {
                StartCoroutine(LoadAssetAsync(loadRequest));
            }
        }

        public void LoadAsset<T>(string bundleName, string assetName, Action<T> onLoadCallback) where T : Object
        {
            var loadRequest = new LoadRequest<T>
            {
                BundleName = bundleName,
                AssetName = assetName,
                OnLoadCallback = onLoadCallback
            };
            if (_initialized)
            {
                StartCoroutine(LoadAssetAsync(loadRequest));
            }
            {
                _loadRequests.Add(loadRequest);
            }
        }

        private IEnumerator<Coroutine> LoadAssetAsync(ILoadRequest loadRequest)
        {
            var loadAssetRequest = loadRequest.NewLoadOperation();
            if (loadAssetRequest == null) yield break;
            yield return StartCoroutine(loadAssetRequest);
            loadRequest.HandleCompletedLoad(loadAssetRequest);
        }
    }
}