
using UnityEngine;

namespace DungeonStrike
{

    enum AnimationState
    {
        Idle,
        Starting,
        Moving,
        Stopping
    }

    public class CharacterMovement : MonoBehaviour
    {
        private UnityEngine.AI.NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Vector3 _target;
        private AnimationState _state;
        private UnityEngine.AI.NavMeshPath _nextPath;
        private CharacterTurning _characterTurning;
        private GameObject _steeringIndicator;
        private GameObject _nextPositionIndicator;
        private GameObject _destinationIndicator;

        void Start()
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _state = AnimationState.Idle;
            _characterTurning = GetComponent<CharacterTurning>();
        }

        void Update()
        {
            // if ((_animator.GetNextAnimatorStateInfo(0).IsName("Running") ||
            //     _animator.GetNextAnimatorStateInfo(0).IsName("Walking")) &&
            //     _state == AnimationState.Starting)
            // {
            //     _animator.applyRootMotion = false;
            //     _navMeshAgent.SetPath(_nextPath);
            //     SetAnimationState(AnimationState.Moving);
            // }

            if (Vector3.Distance(transform.position, _target) < GetStopDistance() &&
                _state == AnimationState.Moving)
            {
                _animator.SetBool("Walking", false);
                _animator.SetBool("Running", false);
                SetAnimationState(AnimationState.Stopping);
            }

            if (AnimationStates.IsCurrentStateIdle(_animator) && _state == AnimationState.Stopping)
            {
                SetAnimationState(AnimationState.Idle);
                _nextPath = null;
            }

            UpdateDebugIndicators();
        }

        public void MoveToPoint(Vector3 point)
        {
            Preconditions.CheckState(_state == AnimationState.Idle);
            _target = point;
            var angle = Transforms.AngleToTarget(this.transform, _target, AngleType.Horizontal);
            _characterTurning.TurnToAngle(angle, BeginMoving);
        }

        private void BeginMoving()
        {
            _animator.applyRootMotion = false;
            _nextPath = new UnityEngine.AI.NavMeshPath();
            _navMeshAgent.CalculatePath(_target, _nextPath);

            Preconditions.CheckState(_nextPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete);
            var corner1 = _nextPath.corners[1];
            var targetDistance = Vector3.Distance(transform.position, _target);
            var corner1Distance = Vector3.Distance(transform.position, corner1);
            if (Mathf.Max(targetDistance, corner1Distance) < 5.0f)
            {
                _animator.SetBool("Walking", true);
                _navMeshAgent.speed = 2.5f;
                _navMeshAgent.angularSpeed = 250;
                _navMeshAgent.acceleration = 25;
            }
            else
            {
                _animator.SetBool("Running", true);
                _navMeshAgent.speed = 5.0f;
                _navMeshAgent.angularSpeed = 500;
                _navMeshAgent.acceleration = 50;
            }

            _navMeshAgent.SetPath(_nextPath);
            SetAnimationState(AnimationState.Moving);
        }

        private float GetStopDistance()
        {
            if (_animator.GetBool("Walking"))
            {
                return 0.1f;
            }
            else
            {
                return 0.2f;
            }
        }

        private void UpdateDebugIndicators()
        {
            if (DebugManager.Instance != null && DebugManager.Instance.ShowNavigationDebug)
            {
                if (_destinationIndicator == null)
                {
                    _destinationIndicator = CreateIndicator(Color.yellow);
                    _nextPositionIndicator = CreateIndicator(Color.blue);
                    _steeringIndicator = CreateIndicator(Color.red);
                }

                _destinationIndicator.transform.position = _navMeshAgent.destination;
                _nextPositionIndicator.transform.position = _navMeshAgent.nextPosition;
                if (_nextPath != null && _nextPath.corners.Length > 1)
                {
                    _steeringIndicator.transform.position = _nextPath.corners[1];
                }
            }
        }

        private void SetAnimationState(AnimationState newState)
        {
            _state = newState;
        }

        private GameObject CreateIndicator(Color color)
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            result.transform.localScale = new Vector3(0.1f, 1, 0.1f);
            GameObject.Destroy(result.GetComponent<BoxCollider>());
            result.GetComponent<Renderer>().material.color = color;
            return result;
        }
    }
}