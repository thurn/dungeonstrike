using UnityEngine;


namespace DungeonStrike
{
    public class CharacterShoot : MonoBehaviour
    {
        private const string FiringPointTag = "FiringPoint";

        private Animator _animator;
        public Transform WeaponTransform { get; set; }
        public Transform _firingPoint;
        public Transform Target;
        private bool _log;
        private bool _aiming;
        private float _aimAngle;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _firingPoint = GameObjects.FindChildTransformWithTag(WeaponTransform, FiringPointTag);
                _log = true;
                _animator.SetTrigger("Aim");
                _aiming = true;
                // var angleToTarget = AngleToTarget(Vector3.up);
                // Debug.Log("angleToTarget: " + angleToTarget);
                // _animator.SetFloat("AimHorizontal", angleToTarget);
                // var verticalAngle = AngleToTarget(Vector3.right);
                // _animator.SetFloat("AimVertical", -7.5f);
                _animator.SetFloat("AimVertical", -7.5f);
            }

            if (_aiming)
            {
                var angleToTarget = AngleToTarget(Vector3.up);
                Debug.Log("angleToTarget: " + angleToTarget);
                if (angleToTarget > 10)
                {
                    _aimAngle += 1.0f;
                }
                else if (angleToTarget < -10)
                {
                    _aimAngle -= 1.0f;
                }
                else
                {
					Debug.Log("done");
                    _aiming = false;
                }
                _animator.SetFloat("AimHorizontal", _aimAngle);
            }

            if (_log)
            {
                var direction = _firingPoint.forward * 5;
                Debug.DrawRay(_firingPoint.position, direction, Color.yellow);
            }
        }

        private float AngleToTarget(Vector3 projectionNormal)
        {
            var targetPosition = Vector3.ProjectOnPlane(Target.position, projectionNormal);
            var firingPosition = Vector3.ProjectOnPlane(_firingPoint.position, projectionNormal);
            var targetDir = targetPosition - firingPosition;
            var angle = Vector3.Angle(_firingPoint.forward, targetDir);
            // Use cross product to determine the 'direction' of the angle.
            var cross = Vector3.Cross(_firingPoint.forward, targetDir);
            return cross.y < 0 ? -angle : angle;
        }
    }
}