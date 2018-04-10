using UnityEngine;
using System;

namespace DungeonStrike
{
    public class Projectile : AbstractPoolIdConsumer
    {
        public float Velocity;
        public Action<Vector3> OnImpact { get; set; }
        public LayerMask LayerMask;

        private void Update()
        {
            // Projectile step per frame based on velocity and time
            var step = transform.forward * Time.deltaTime * Velocity;

            RaycastHit raycastHit;
            // Raycast for targets with ray length based on frame step by ray cast advance multiplier
            if (Physics.Raycast(transform.position, transform.forward, out raycastHit, step.magnitude * 5.0f, LayerMask))
            {
                foreach (var onProjectileHit in raycastHit.collider.GetComponents<IOnProjectileHit>())
                {
                    onProjectileHit.OnProjectileHit();
                }
                if (OnImpact != null)
                {
                        OnImpact(raycastHit.point);
                }
                Pool.FastDestroy(gameObject);
            }

            // Advances projectile forward
            transform.position += step;
        }
    }
}