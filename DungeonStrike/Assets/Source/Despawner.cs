using UnityEngine;
using System.Collections;

namespace DungeonStrike
{
    public class Despawner : MonoBehaviour, IFastPoolItem
    {
        public float Delay { get { return _delay; } set { _delay = value; } }

        public int PoolId { get { return _poolId;  } set { _poolId = value;  } }

        [SerializeField]
        private float _delay;

        [HideInInspector]
        [SerializeField]
        private int _poolId;

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
            Preconditions.CheckState(_poolId != 0, "You must set a PoolId.");
            FastPoolManager.GetPool(_poolId, null, false).FastDestroy(gameObject);
        }
    }
}