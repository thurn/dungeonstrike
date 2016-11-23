using UnityEngine;
using Forge3D;
using System.Collections.Generic;

namespace DungeonStrike
{

    public class F3DEffects : MonoBehaviour
    {

        private Dictionary<string, Transform> _poolTransforms = new Dictionary<string, Transform>();
        private F3DPool _pool;
        private F3DAudioController _f3DAudioController;

        void Start()
        {
            _f3DAudioController = GetComponent<F3DAudioController>();
            _pool = F3DPoolManager.Pools["GeneratedPool"];
            foreach (var poolKey in _pool.templates)
            {
                _poolTransforms[poolKey.name] = poolKey;
            }
        }

        public void FireVulcan(Transform parentTransform)
        {
            var vulcanMuzzleFlash = _poolTransforms["vulcan_muzzle_example"];
            _pool.Spawn(vulcanMuzzleFlash, parentTransform.position, parentTransform.rotation, parentTransform);

            var vulcanProjectile = _poolTransforms["vulcan_projectile_example"];
            _pool.Spawn(vulcanProjectile, parentTransform.position, parentTransform.rotation, null /* parent */);

            _f3DAudioController.VulcanShot(parentTransform.position);
//            // Get random rotation that offset spawned projectile
//            Quaternion offset = Quaternion.Euler(UnityEngine.Random.onUnitSphere);
//            // Spawn muzzle flash and projectile with the rotation offset at current socket position
//            F3DPoolManager.Pools["GeneratedPool"].Spawn(vulcanMuzzle, TurretSocket[curSocket].position,
//                TurretSocket[curSocket].rotation, TurretSocket[curSocket]);
//            GameObject newGO =
//                F3DPoolManager.Pools["GeneratedPool"].Spawn(vulcanProjectile,
//                    TurretSocket[curSocket].position + TurretSocket[curSocket].forward,
//                    offset * TurretSocket[curSocket].rotation, null).gameObject;
//
//            F3DProjectile proj = newGO.gameObject.GetComponent<F3DProjectile>();
//            if (proj)
//            {
//                proj.SetOffset(vulcanOffset);
//            }
//
//            // Emit one bullet shell
//            if (ShellParticles.Length > 0)
//                ShellParticles[curSocket].Emit(1);
//
//            // Play shot sound effect
//            F3DAudioController.instance.VulcanShot(TurretSocket[curSocket].position);
//
//            // Advance to next turret socket
//            AdvanceSocket();
        }

        // Spawn vulcan weapon impact
//        public void VulcanImpact(Vector3 pos)
//        {
//            // Spawn impact prefab at specified position
//            F3DPoolManager.Pools["GeneratedPool"].Spawn(vulcanImpact, pos, Quaternion.identity, null);
//            // Play impact sound effect
//            F3DAudioController.instance.VulcanHit(pos);
//        }
    }
}