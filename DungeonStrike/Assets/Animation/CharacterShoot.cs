using UnityEngine;


namespace DungeonStrike
{
    public class CharacterShoot : MonoBehaviour
    {
        private const string FiringPointTag = "FiringPoint";
        private const float AimingSpeedFactor = 0.07f;

        private Animator _animator;
        public Transform WeaponTransform { get; set; }
        public Transform _firingPoint;
        public Transform Target;
        private int _spaces;
        private CharacterTurning _characterTurning;
        private bool _aiming;
        private float _horizontalAimAngle;
        private float _verticalAimAngle;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _characterTurning = GetComponent<CharacterTurning>();
        }

        void Update()
        {
            if (_firingPoint)
            {
                Debug.DrawRay(_firingPoint.position, _firingPoint.forward * 5.0f, Color.yellow);
            }

            // if (_aiming)
            // {
            //     var horizontalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Horizontal);
            //     var verticalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Vertical);
            //     Debug.Log("h: " + horizontalAngle + " v: " + verticalAngle);
            // }

            if (_aiming)
            {
                var horizontalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Horizontal);
                _horizontalAimAngle += horizontalAngle * AimingSpeedFactor;
                _animator.SetFloat("HorizontalAimAngle", _horizontalAimAngle);

                var verticalAngle = Transforms.AngleToTarget(_firingPoint, Target, AngleType.Vertical);
                _verticalAimAngle += verticalAngle * AimingSpeedFactor;
                _animator.SetFloat("VerticalAimAngle", _verticalAimAngle);

                if (Mathf.Abs(horizontalAngle) < 1.0f && Mathf.Abs(verticalAngle) < 1.0f)
                {
                    _aiming = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("space " + _spaces);
                switch (_spaces++)
                {
                    case 0:
                        _firingPoint = Transforms.FindChildTransformWithTag(WeaponTransform, FiringPointTag);
                        _characterTurning.TurnToAngle(90.0f, StartAiming);
                        break;
                    case 1:
                        _animator.SetTrigger("Shoot");
                        break;
                }
            }
        }

        private void StartAiming()
        {
            _animator.SetBool("Aiming", true);
            _aiming = true;
            _horizontalAimAngle = 0.0f;
            _verticalAimAngle = 0.0f;
        }
    }
}