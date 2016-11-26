using UnityEngine;
using System.Collections;

namespace DungeonStrike
{
    public class Despawner : MonoBehaviour, IFastPoolItem, IPoolIdConsumer
    {
        [SerializeField] private float _delay;
        // Cannot be an auto-generated property because Unity doesn't serialize them.
        // Cannot be 'int?' because Unity doesn't serialize those either.
        [HideInInspector] [SerializeField] private int _poolId;

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public int PoolId
        {
            get { return _poolId; }
            set
            {
                Preconditions.CheckArgument(value != 0, "PoolID cannot be 0");
                _poolId = value;
            }
        }

        private void Start()
        {
            StartCoroutine(Despawn());
        }

        public void OnFastInstantiate()
        {
            StartCoroutine(Despawn());
        }

        public void OnFastDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(Delay);
            StopAllCoroutines();
            Preconditions.CheckState(_poolId != 0, "Must specify a Pool ID");
            FastPoolManager.GetPool(_poolId, null).FastDestroy(gameObject);
        }
    }
}