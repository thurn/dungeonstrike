using UnityEngine;
using System;

namespace DungeonStrike
{
    public class CharacterTurning : MonoBehaviour
    {

        private Animator _animator;
        private Action _onComplete;
        private bool _turning;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
			if (_turning && _animator.GetNextAnimatorStateInfo(0).IsName("Idle"))
			{
                _onComplete();
                _turning = false;
                _onComplete = null;
            }
        }

        public void TurnToAngle(float angle, Action onComplete)
        {
            var rightAngle = Transforms.ToRightAngle(angle);
            if (rightAngle == 0.0f)
            {
                onComplete();
                return;
            }
            else
            {
                _onComplete = onComplete;
                _turning = true;
                _animator.SetTrigger("Turning");
                _animator.SetFloat("TurnAngle", rightAngle);
            }
        }
    }
}