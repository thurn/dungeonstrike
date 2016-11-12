using UnityEngine;

namespace DungeonStrike
{
    public class CharacterSpellcasting : MonoBehaviour
    {
        private CharacterTurning _characterTurning;
        private Animator _animator;

        void Start()
        {
            _characterTurning = GetComponent<CharacterTurning>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {

        }

		public void CastSpellWithTarget(Transform target)
		{
            var angle = Transforms.AngleToTarget(transform, target.position);
            _characterTurning.TurnToFaceTarget(target.position, BeginCast);
        }

		private void BeginCast()
		{
			Debug.Log("casting");
            _animator.SetTrigger("Casting");
        }
    }
}