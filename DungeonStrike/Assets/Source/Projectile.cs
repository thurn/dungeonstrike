using UnityEngine;

namespace DungeonStrike
{
    public class Projectile : MonoBehaviour
    {
        public float Velocity;
        public LayerMask LayerMask;
        private ParticleSystem[] _childParticleSystems;

        private void Update()
        {
            // Projectile step per frame based on velocity and time
            var step = transform.forward * Time.deltaTime * Velocity;

            RaycastHit hitPoint;
            // Raycast for targets with ray length based on frame step by ray cast advance multiplier
            if (Physics.Raycast(transform.position, transform.forward, out hitPoint, step.magnitude * 2.0f, LayerMask))
            {
                FastPoolManager.GetPool(gameObject).FastDestroy(gameObject);
            }

            // Advances projectile forward
            transform.position += step;
        }
    }
}