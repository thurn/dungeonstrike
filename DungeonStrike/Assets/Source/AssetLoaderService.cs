using UnityEngine;
using System.Collections.Generic;
using AssetBundles;

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
        private IEnumerator<Coroutine> Start()
        {
            yield return StartCoroutine(Initialize());
        }

        // Initialize the downloading url and AssetBundleManifest object.
        private IEnumerator<Coroutine> Initialize()
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
#endif

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.Initialize();
            if (request != null)
                yield return StartCoroutine(request);
        }

        public void InstantiateGameObject(string bundleName, string assetName, System.Action<GameObject> onLoadCallback)
        {
            StartCoroutine(InstantiateGameObjectCoroutine(bundleName, assetName, onLoadCallback));
        }

        private IEnumerator<Coroutine> InstantiateGameObjectCoroutine(string assetBundleName,
            string assetName,
            System.Action<GameObject> onLoadCallback)
        {
            // Load asset from assetBundle.
            var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
            if (request == null)
                yield break;
            yield return StartCoroutine(request);

            // Get the asset.
            var prefab = request.GetAsset<GameObject>();

            if (prefab != null)
            {
                var result = Object.Instantiate(prefab);
                onLoadCallback(result);
            }
        }
    }
}