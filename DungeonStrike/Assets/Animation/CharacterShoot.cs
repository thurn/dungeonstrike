using UnityEngine;


namespace DungeonStrike
{
    public class CharacterShoot : MonoBehaviour
    {
        enum State
        {
            Default,
            Aiming,
            Shooting,
        }

        public Transform WeaponTransform { get; set; }

        private const bool DrawDebugFiringLine = false;
        private const string FiringPointTag = "FiringPoint";
        private const float AimingSpeedFactor = 0.07f;

        private Transform _target;
        private Animator _animator;
        private Transform _firingPoint;
        private CharacterTurning _characterTurning;
        private State _state;
        private float _horizontalAimAngle;
        private float _verticalAimAngle;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _characterTurning = GetComponent<CharacterTurning>();
            _state = State.Default;
        }

        void Update()
        {
            if (DrawDebugFiringLine && _firingPoint)
            {
                Debug.DrawRay(_firingPoint.position, _firingPoint.forward * 5.0f, Color.yellow);
            }

            // Slowly turn to aim at target
            if (_state == State.Aiming)
            {
                var horizontalAngle = Transforms.AngleToTarget(_firingPoint, _target.position, AngleType.Horizontal);
                _horizontalAimAngle += horizontalAngle * AimingSpeedFactor;
                _animator.SetFloat("HorizontalAimAngle", _horizontalAimAngle);

                var verticalAngle = Transforms.AngleToTarget(_firingPoint, _target.position, AngleType.Vertical);
                _verticalAimAngle += verticalAngle * AimingSpeedFactor;
                _animator.SetFloat("VerticalAimAngle", _verticalAimAngle);

                if (Mathf.Abs(horizontalAngle) < 1.0f && Mathf.Abs(verticalAngle) < 1.0f)
                {
                    _state = State.Shooting;
                    _animator.SetBool("Aiming", true);
                    _animator.SetTrigger("Shoot");
                }
            }

            // Finished playing Shoot animation
            if (_state == State.Shooting && _animator.GetNextAnimatorStateInfo(2).IsName("Empty State"))
            {
                _animator.SetBool("Aiming", false);
                _state = State.Default;
                _target = null;
            }
        }

        public void ShootAtTarget(Transform target)
        {
            _target = target;
            _firingPoint = Transforms.FindChildTransformWithTag(WeaponTransform, FiringPointTag);
            var horizontalAngle = Transforms.AngleToTarget(_firingPoint, _target.position, AngleType.Horizontal);
            _characterTurning.TurnToAngle(horizontalAngle, StartAiming);
        }

        private void StartAiming()
        {
            _animator.SetFloat("HorizontalAimAngle", 0);
            _animator.SetFloat("VerticalAimAngle", 0);
            _animator.SetBool("Aiming", true);
            _state = State.Aiming;
            _horizontalAimAngle = 0.0f;
            _verticalAimAngle = 0.0f;
        }
    }
}