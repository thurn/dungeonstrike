using UnityEngine;
using System.Collections;

namespace DungeonStrike
{
    public class Despawner : AbstractPoolIdConsumer, IFastPoolItem
    {
        [SerializeField] private float _delay;

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
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
            Pool.FastDestroy(gameObject);
        }
    }

}