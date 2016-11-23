using UnityEngine;

namespace DungeonStrike
{

    public class F3DEffects : MonoBehaviour
    {
        // TODO:
        // Replace with AssetLoader. Direct prefab references are scary! Mutations at
        // runtime persist in the editor!

        public GameObject VulcanMuzzle;
        public GameObject VulcanProjectile;
        public GameObject VulcanImpact;
        public GameObject VulcanShell;
        public GameObject VulcanSpark;
        public AudioClip VulcanShotAudio;
        public AudioClip VulcanImpactAudio;

        private void Start()
        {
            InitializePools(
                VulcanMuzzle,
                VulcanProjectile,
                VulcanImpact,
                VulcanShell,
                VulcanSpark
            );
        }

        public void FireVulcan(Transform parentTransform)
        {
            CreateFromPrefab(VulcanMuzzle, parentTransform);
            CreateFromPrefab(VulcanProjectile, parentTransform);

            var audioSource = parentTransform.GetComponent<AudioSource>();
            Preconditions.CheckState(audioSource != null, "Firing source must have an AudioSource.");
            audioSource.clip = VulcanShotAudio;
            audioSource.pitch = Random.Range(0.95f, 1f);
            audioSource.volume = Random.Range(0.8f, 1f);
            audioSource.minDistance = 5f;
            audioSource.loop = false;
            audioSource.Play();
        }

        private static void InitializePools(params GameObject[] prefabs)
        {
            foreach (var prefab in prefabs)
            {
                var pool = FastPoolManager.GetPool(prefab);
                pool.Capacity = 10;
                pool.PreloadCount = 1;
                pool.NotificationType = PoolItemNotificationType.Interface;

                var despawner = prefab.GetComponent<Despawner>();
                if (despawner != null)
                {
                    Debug.Log("Setting pool ID");
                    despawner.PoolId = pool.ID;
                }
            }
        }

        private static GameObject CreateFromPrefab(GameObject prefab, Transform parent)
        {
            var pool = FastPoolManager.GetPool(prefab, createIfNotExists: false);
            var result = pool.FastInstantiate(parent);
            return result;
        }
    }
}