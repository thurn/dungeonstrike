using System.Collections.Generic;
using UnityEngine;

namespace DungeonStrike
{
    public class F3DEffects : MonoBehaviour
    {
        private static F3DEffects _instance;

        public static F3DEffects Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<F3DEffects>()); }
        }

        private void Start()
        {
            LoadPrefab(Prefab.VulcanMuzzle);
            LoadPrefab(Prefab.VulcanProjectile);
            LoadPrefab(Prefab.VulcanImpact);
            LoadPrefab(Prefab.VulcanShotAudio);
        }

        public void FireVulcan(Transform parentTransform)
        {
            CreateFromPrefab(Prefab.VulcanMuzzle, parentTransform);

            var shotAudio = CreateFromPrefab(Prefab.VulcanShotAudio, parentTransform);
            var audioSource = shotAudio.GetComponent<AudioSource>();
            audioSource.pitch = Random.Range(0.95f, 1f);
            audioSource.volume = Random.Range(0.8f, 1f);
            audioSource.minDistance = 5f;
            audioSource.Play();

            var projectileObject = CreateFromPrefab(Prefab.VulcanProjectile, parentTransform);
            var projectile = projectileObject.GetComponent<Projectile>();
            projectile.OnImpact = (impactPosition) =>
            {
                var impact = CreateFromPrefab(Prefab.VulcanImpact, null);
                impact.transform.position = impactPosition;
            };
        }

        private static void LoadPrefab(Prefab prefab)
        {
            var prefabName = Prefabs.GetAssetName(prefab);
            AssetLoaderService.Instance.InstantiateGameObject("gun_effects", prefabName, (instance) =>
            {
                foreach (var poolIdConsumer in instance.GetComponents<IPoolIdConsumer>()) {
                    poolIdConsumer.PoolId = (int)prefab;
                }

                var pool = FastPoolManager.CreatePool((int)prefab, instance, true, 1, 10);
                pool.NotificationType = PoolItemNotificationType.Interface;
            });
        }

        private static GameObject CreateFromPrefab(Prefab prefab, Transform parent)
        {
            var pool = FastPoolManager.GetPool((int)prefab, null, createIfNotExists: false);
            var result = pool.FastInstantiate(parent);
            return result;
        }
    }
}