using UnityEngine;
using System;

namespace DungeonStrike
{
    public class CharacterTurning : MonoBehaviour
    {
        private const float TurnSpeed = 10.0f;

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
            if (_idleNext && AnimationStates.IsCurrentStateIdle(_animator))
            {
                if (_onComplete != null)
                {
                    _onComplete();
                }
                _turning = false;
                _onComplete = null;
                _idleNext = false;
            }

            if (_turning && AnimationStates.IsNextStateIdle(_animator))
            {
                _idleNext = true;
            }
        }

        public void TurnToAngle(float angle, Action onComplete)
        {
            Preconditions.CheckState(_onComplete == null);
            angle = Transforms.ToRightAngle(angle);
            Debug.Log("Turning to angle: " + angle);
            if (angle == 0.0f)
            {
                onComplete();
            }
            else
            {
                _turning = true;
                _onComplete = onComplete;
                _animator.applyRootMotion = true;
                _animator.SetTrigger("Turning");
                _animator.SetFloat("TurnAngle", angle);
            }
        }
    }
}