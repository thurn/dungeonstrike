﻿using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class CharacterShoot : MonoBehaviour
    {
        enum State
        {
            Default,
            HorizontalAiming,
            VerticalAiming,
            Shooting,
        }

        private const float AimingSpeedFactor = 0.1f;

        private Vector3 _target;
        private Animator _animator;
        private Transform _firingPoint;
        private CharacterTurning _characterTurning;
        private State _state;
        private float _horizontalAimAngle;
        private float _verticalAimAngle;
        private VectorLine _aimLine;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _characterTurning = GetComponent<CharacterTurning>();
            _state = State.Default;
        }

        void Update()
        {
            UpdateDebugLines();

            if (_state == State.VerticalAiming)
            {
                var verticalAngle = Transforms.AngleToTarget(_firingPoint, _target, AngleType.Vertical);
                _verticalAimAngle += verticalAngle * AimingSpeedFactor;
                _animator.SetFloat("VerticalAimAngle", _verticalAimAngle);

                if (Mathf.Abs(verticalAngle) < 1.0f)
                {
                    _state = State.HorizontalAiming;
                }
            }

            if (_state == State.HorizontalAiming)
            {
                var horizontalAngle = Transforms.AngleToTarget(_firingPoint, _target, AngleType.Horizontal);
                _horizontalAimAngle += horizontalAngle * AimingSpeedFactor;
                _animator.SetFloat("HorizontalAimAngle", _horizontalAimAngle);

                if (Mathf.Abs(horizontalAngle) < 1.0f)
                {
                    _state = State.Shooting;
                    _animator.SetTrigger("Shoot");
                }
            }

            // Finished playing Shoot animation
            if (_state == State.Shooting && _animator.GetNextAnimatorStateInfo(2).IsName("Empty State"))
            {
                _animator.SetBool("Aiming", false);
                _state = State.Default;
            }
        }

        public void ShootAtTarget(Transform target)
        {
            _firingPoint = Transforms.FindChildTransformWithTag(this.transform, Tags.FiringPointTag);
            _target = Transforms.FindChildTransformWithTag(target, Tags.TargetPointTag).position;
            var horizontalAngle = Transforms.AngleToTarget(_firingPoint, _target, AngleType.Horizontal);
            _characterTurning.TurnToAngle(horizontalAngle, StartAiming);
        }

        private void StartAiming()
        {
            _animator.SetFloat("HorizontalAimAngle", 0);
            _animator.SetFloat("VerticalAimAngle", 0);
            _animator.SetBool("Aiming", true);

            _state = State.VerticalAiming;
            _horizontalAimAngle = 0.0f;
            _verticalAimAngle = 0.0f;
        }

        private void UpdateDebugLines()
        {
            if (DebugManager.Instance != null && DebugManager.Instance.ShowAimLines && _firingPoint != null)
            {
                if (_aimLine == null)
                {
                    var points = new List<Vector3>(2);
                    _aimLine = new VectorLine("aimLine", points, 5.0f /* width */);
                }

                Color color;
                switch (_state)
                {
                    case State.Default:
                        color = Color.green;
                        break;
                    case State.VerticalAiming:
                        color = Color.yellow;
                        break;
                    case State.HorizontalAiming:
                        color = Color.blue;
                        break;
                    case State.Shooting:
                        color = Color.red;
                        break;
                    default:
                        throw Preconditions.UnexpectedEnumValue(_state);
                }

                _aimLine.points3[0] = _firingPoint.position;
                _aimLine.points3[1] = _firingPoint.position + (_firingPoint.forward * 5.0f);
                _aimLine.color = color;
                _aimLine.Draw3D();
            }
        }
    }
}