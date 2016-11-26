using UnityEngine;
using System.Collections;

namespace DungeonStrike
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioDespawner : AbstractPoolIdConsumer, IFastPoolItem
    {
        private float _delay;

        private void Start()
        {
            var audioSource = GetComponent<AudioSource>();
            _delay = audioSource.clip.length + 1.0f;
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
            yield return new WaitForSeconds(_delay);
            StopAllCoroutines();
            Pool.FastDestroy(gameObject);
        }
    }

}