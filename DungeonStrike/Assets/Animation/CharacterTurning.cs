using UnityEngine;
using System;

namespace DungeonStrike
{
    public class CharacterTurning : MonoBehaviour
    {
        private Animator _animator;
        private Action _onComplete;
        private bool _turning;
        private bool _idleNext;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (_idleNext && _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                _onComplete();
                _turning = false;
                _onComplete = null;
                _idleNext = false;
            }

            if (_turning && _animator.GetNextAnimatorStateInfo(0).IsName("Idle"))
            {
                _idleNext = true;
            }
        }

        public void TurnToAngle(float angle, Action onComplete)
        {
            Preconditions.CheckState(_onComplete == null);
            _onComplete = onComplete;
            var rightAngle = Transforms.ToRightAngle(angle);
            if (rightAngle == 0.0f)
            {
                _onComplete();
                return;
            }
            else
            {
                _turning = true;
                _animator.applyRootMotion = true;
                _animator.SetTrigger("Turning");
                _animator.SetFloat("TurnAngle", rightAngle);
            }
        }
    }
}