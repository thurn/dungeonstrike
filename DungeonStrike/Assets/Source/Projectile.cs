using UnityEngine;
using System;

namespace DungeonStrike
{
    public class Projectile : MonoBehaviour, IPoolIdConsumer
    {
        public float Velocity;
        public Action<Vector3> OnImpact { get; set; }
        public LayerMask LayerMask;

        [HideInInspector] [SerializeField] private int _poolId;

        public int PoolId
        {
            get { return _poolId; }
            set
            {
                Preconditions.CheckArgument(value != 0, "PoolID cannot be 0");
                _poolId = value;
            }
        }

        private void Update()
        {
            // Projectile step per frame based on velocity and time
            var step = transform.forward * Time.deltaTime * Velocity;

            RaycastHit hitPoint;
            // Raycast for targets with ray length based on frame step by ray cast advance multiplier
            if (Physics.Raycast(transform.position, transform.forward, out hitPoint, step.magnitude * 2.0f, LayerMask))
            {
                if (OnImpact != null)
                {
                    OnImpact(hitPoint.point);
                }
                FastPoolManager.GetPool(_poolId, null).FastDestroy(gameObject);
            }

            // Advances projectile forward
            transform.position += step;
        }
    }
}