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

    public class CharacterAnimation : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Vector3 _target;
        private GameObject _steeringIndicator;
        private GameObject _nextPositionIndicator;
        private GameObject _destinationIndicator;
        private AnimationState _state;
        private NavMeshPath _nextPath;

        void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _destinationIndicator = CreateIndicator(Color.yellow);
            _nextPositionIndicator = CreateIndicator(Color.blue);
            _steeringIndicator = CreateIndicator(Color.red);
            _state = AnimationState.Idle;
        }

        // Update is called once per frame
        void Update()
        {
            _destinationIndicator.transform.position = _navMeshAgent.destination;
            _nextPositionIndicator.transform.position = _navMeshAgent.nextPosition;
            if (_nextPath != null && _nextPath.corners.Length > 1)
            {
                _steeringIndicator.transform.position = _nextPath.corners[1];
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _state == AnimationState.Idle)
            {
                _animator.applyRootMotion = true;
                _target = GetClickedPoint();
                _nextPath = new NavMeshPath();
                _navMeshAgent.CalculatePath(_target, _nextPath);

                if (_nextPath.status == NavMeshPathStatus.PathComplete)
                {
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

                    var turn = TurnAngleToTarget(corner1);
                    _animator.SetFloat("TurnAngle", turn);
                    _animator.SetTrigger("Move");
                    SetAnimationState(AnimationState.Starting);
                }
                else
                {
                    throw new System.SystemException("Illegal path to target!");
                }
            }

            if ((_animator.GetNextAnimatorStateInfo(0).IsName("Running") ||
                _animator.GetNextAnimatorStateInfo(0).IsName("Walking")) &&
                _state == AnimationState.Starting)
            {
                _animator.applyRootMotion = false;
                _navMeshAgent.SetPath(_nextPath);
                SetAnimationState(AnimationState.Moving);
            }

            if (Vector3.Distance(transform.position, _target) < GetStopDistance() &&
                _state == AnimationState.Moving)
            {
                _animator.SetBool("Walking", false);
                _animator.SetBool("Running", false);
                SetAnimationState(AnimationState.Stopping);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
                _state == AnimationState.Stopping)
            {
                SetAnimationState(AnimationState.Idle);
                _nextPath = null;
            }
        }

        private Vector3 GetClickedPoint()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //var cell = GGGrid.GetCellFromRay(ray, 1000f);
            //if (cell == null) return;
            //return cell.CenterPoint3D;

            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit, Mathf.Infinity);
            return raycastHit.point;
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

        private int TurnAngleToTarget(Vector3 target)
        {
            var angle = AngleToTarget(target);
            if (angle < -135)
            {
                return -180;
            }
            else if (angle < -45)
            {
                return -90;
            }
            else if (angle < 45)
            {
                return 0;
            }
            else if (angle < 135)
            {
                return 90;
            }
            else
            {
                return 180;
            }
        }

        private float AngleToTarget(Vector3 target)
        {
            var targetDir = target - transform.position;
            var angle = Vector3.Angle(transform.forward, targetDir);
            // Use cross product to determine the 'direction' of the angle.
            var cross = Vector3.Cross(transform.forward, targetDir);
            return cross.y < 0 ? -angle : angle;
        }
    }
}