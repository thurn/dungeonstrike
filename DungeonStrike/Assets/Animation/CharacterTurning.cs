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
        private Vector3 _target;

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

            if (_turning)
            {
                var angle = Transforms.AngleToTarget(transform, _target);
                if (angle > 1.0f)
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        transform.eulerAngles.y + 1.0f,
                        transform.eulerAngles.z
                    );
                }
                else if (angle < -1.0f)
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        transform.eulerAngles.y - 1.0f,
                        transform.eulerAngles.z
                    );
                }
            }
        }

        public void TurnToFaceTarget(Vector3 target, Action onComplete)
        {
            Preconditions.CheckState(_onComplete == null);
            var angle = Transforms.AngleToTarget(transform, target);
            if (Mathf.Abs(angle) < 5.0f)
            {
                onComplete();
                return;
            }

            _target = target;
            _onComplete = onComplete;
            _turning = true;
            _animator.applyRootMotion = true;
            _animator.SetTrigger("Turning");
            _animator.SetFloat("TurnAngle", angle);
        }

        public void TurnToAngle(float angle, Action onComplete, bool nearestNinetyDegrees = true)
        {
            Preconditions.CheckState(_onComplete == null);
            _onComplete = onComplete;
            if (nearestNinetyDegrees)
            {
                angle = Transforms.ToRightAngle(angle);
            }
            if (angle == 0.0f)
            {
                _onComplete();
                _onComplete = null;
                return;
            }
            else
            {
                _turning = true;
                _animator.applyRootMotion = true;
                _animator.SetTrigger("Turning");
                _animator.SetFloat("TurnAngle", angle);
            }
        }
    }
}