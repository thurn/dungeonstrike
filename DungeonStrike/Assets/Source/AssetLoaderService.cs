using UnityEngine;
using System.Collections;
using AssetBundles;
using System;

namespace DungeonStrike
{
    public class AssetLoaderService : MonoBehaviour
    {
        private const string AssetBundlesOutputPath = "/AssetBundles/";

        private static AssetLoaderService _instance;

        public static AssetLoaderService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<AssetLoaderService>()); }
        }

        // Use this for initialization
        IEnumerator Start()
        {
            yield return StartCoroutine(Initialize());
        }

        // Initialize the downloading url and AssetBundleManifest object.
        private IEnumerator Initialize()
        {
            // Don't destroy this gameObject as we depend on it to run the loading script.
            DontDestroyOnLoad(gameObject);

            // With this code, when in-editor or using a development builds: Always use the AssetBundle Server
            // (This is very dependent on the production workflow of the project.
            // 	Another approach would be to make this configurable in the standalone player.)
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            AssetBundleManager.SetDevelopmentAssetBundleServer();
#else
		// Use the following code if AssetBundles are embedded in the project for example via StreamingAssets folder etc:
		AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
		// Or customize the URL based on your deployment or configuration
		//AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
#endif

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.Initialize();
            if (request != null)
                yield return StartCoroutine(request);
        }

        public void InstantiateGameObject(string bundleName, string assetName, Action<GameObject> onLoadCallback)
        {
            StartCoroutine(InstantiateGameObjecCoroutine(bundleName, assetName, onLoadCallback));
        }

        private IEnumerator InstantiateGameObjecCoroutine(string assetBundleName, string assetName, Action<GameObject> onLoadCallback)
        {
            // Load asset from assetBundle.
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
            if (request == null)
                yield break;
            yield return StartCoroutine(request);

            // Get the asset.
            GameObject prefab = request.GetAsset<GameObject>();

            if (prefab != null)
            {
                var result = GameObject.Instantiate(prefab);
                onLoadCallback(result);
            }
        }
    }
}