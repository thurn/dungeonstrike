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

        private const bool DrawDebugFiringLine = false;
        private const string FiringPointTag = "FiringPoint";
        private const float AimingSpeedFactor = 0.07f;

        private Animator _animator;
        public Transform WeaponTransform { get; set; }
        public Transform _firingPoint;
        public Transform Target;
        private int _spaces;
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

            if (_state == State.Aiming)
            {
                var horizontalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Horizontal);
                _horizontalAimAngle += horizontalAngle * AimingSpeedFactor;
                _animator.SetFloat("HorizontalAimAngle", _horizontalAimAngle);

                var verticalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Vertical);
                _verticalAimAngle += verticalAngle * AimingSpeedFactor;
                _animator.SetFloat("VerticalAimAngle", _verticalAimAngle);

                if (Mathf.Abs(horizontalAngle) < 1.0f && Mathf.Abs(verticalAngle) < 1.0f)
                {
                    _state = State.Shooting;
                    _animator.SetBool("Aiming", true);
                    _animator.SetTrigger("Shoot");
                }
            }

            if (_state == State.Shooting && _animator.GetNextAnimatorStateInfo(2).IsName("Empty State"))
            {
                _animator.SetBool("Aiming", false);
                _state = State.Default;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("space " + _spaces);
                switch (_spaces++)
                {
                    case 0:
                        _firingPoint = Transforms.FindChildTransformWithTag(WeaponTransform, FiringPointTag);
                        var horizontalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Horizontal);
                        _characterTurning.TurnToAngle(horizontalAngle, StartAiming);
                        break;
                }
            }
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