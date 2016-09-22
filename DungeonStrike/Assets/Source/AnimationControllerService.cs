using UnityEngine;
using System.Collections.Generic;

namespace DungeonStrike
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class AnimationControllerService : MonoBehaviour
    {
        public Transform StartingTarget;
        private Animator _animator;
        private Vector3 _target;
        private bool _moving;
        private bool _rotating;
        private NavMeshAgent _navMeshAgent;
        private Vector2 _smoothDeltaPosition = Vector2.zero;
        private Vector2 _velocity = Vector2.zero;
        private float _horizontal;
        private bool _clearAimAngle;
        private Vector3 _nextPosition = Vector3.zero;

        public void Start()
        {
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.speed = 3.5f;
            _navMeshAgent.SetDestination(StartingTarget.position);
            //StartCoroutine(SetTargetAfterDelay(StartingTarget.position));
        }

        public void Update()
        {
            if (!_moving) {
                _moving = true;
            }

            var angleToNextPosition = AngleToTarget(_navMeshAgent.nextPosition);
            //Debug.Log("angleToNextPosition: " + angleToNextPosition / 4.0);
            _animator.SetFloat("HorAimAngle", angleToNextPosition);

            Vector3 worldDeltaPosition = _navMeshAgent.nextPosition - transform.position;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector3.Dot(transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
            {
                _velocity = _smoothDeltaPosition / Time.deltaTime;
            }

            bool shouldMove = _velocity.magnitude > 0.5f && _navMeshAgent.remainingDistance > _navMeshAgent.radius;

            // Update animation parameters
            _animator.SetFloat("InputMagnitude", shouldMove ? 1 : 0);
            _animator.SetFloat("Horizontal", _velocity.x);
            _animator.SetFloat("Vertical", _velocity.y);
        }

        public void Update2()
        {
            if (!_navMeshAgent.pathPending)
            {
                _nextPosition = _navMeshAgent.nextPosition;
                Debug.Log("_nextPosition: " + _nextPosition);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _rotating = true;
            }

            if (_rotating)
            {
                var angle = AngleToTarget(_navMeshAgent.nextPosition);
                Debug.Log("angle to next position " + angle);
                return;
                if (Mathf.Abs(angle) < 1)
                {
                    _rotating = false;
                    _animator.SetFloat("HorAimAngle", 0);
                }
                else
                {
                    _animator.SetFloat("HorAimAngle", 45);
                }
            }

            if (_moving)
            {
            }
        }

        private IEnumerator<WaitForSeconds> SetTargetAfterDelay(Vector3 target)
        {
            yield return new WaitForSeconds(1);
            SetTarget(target);
        }

        public void SetTarget(Vector3 target)
        {
            _target = target;
            _rotating = true;
            _navMeshAgent.SetDestination(_target);
            StartCoroutine(StartMoveAfterDelay());
        }

        private IEnumerator<WaitForSeconds> StartMoveAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            _animator.speed = 0.1f;
            var angle = AngleToTarget(_navMeshAgent.nextPosition) / 3.0f;
            Debug.Log("rotating to angle: " + angle);
            _animator.SetFloat("HorAimAngle", angle);
            yield return new WaitForSeconds(5f);
            _animator.SetFloat("HorAimAngle", 0);
            yield return new WaitForSeconds(5f);
            _rotating = false;
            _moving = true;
        }

        /*
         * Returns the rotational angle between this.transform.rotation and the target position. Always
         * normalizes to a result between -180 and 180 degrees.
         */
        private float AngleToTarget(Vector3 target)
        {
            var rotation = Quaternion.FromToRotation(transform.forward, target - transform.position);
            var degrees = rotation.eulerAngles.y;
            return degrees > 180 ? degrees - 360 : degrees;
        }

    }
}