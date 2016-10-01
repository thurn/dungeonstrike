using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class MovementAnimator : MonoBehaviour
    {
        private MotionController _motionController;
        private NavMeshAgent _navMeshAgent;
        private GameObject _steeringIndicator;
        private GameObject _nextPositionIndicator;
        private GameObject _destinationIndicator;
        private bool _moving;
        private bool _steer;
        private bool _stopping;
        private int _steerCount;
        private Vector3? _target;

        void Start()
        {
            _motionController = GetComponent<MotionController>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.speed = 1.8f;
            _navMeshAgent.radius = 0.5f;
            _navMeshAgent.acceleration = 5.0f;
            _navMeshAgent.stoppingDistance = 0.0f;
            _navMeshAgent.angularSpeed = 120f;
        }

        void Update()
        {
            if (_stopping)
            {
                if (Vector3.Distance(transform.position, _target.Value) < 0.001f)
                {
                    _stopping = false;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, _target.Value, 0.1f * Time.deltaTime);
                }
            }
            if (_moving && ReachedDestination())
            {
                _motionController.ClearTarget();
                _navMeshAgent.ResetPath();
                _moving = false;
                _stopping = true;
            }
            else if (_steer)
            {
                _motionController.SetTargetPosition(_navMeshAgent.steeringTarget, 1.0f);
                _steerCount++;
                if (_steerCount > 5)
                {
                    _steer = false;
                    _moving = true;
                    _steerCount = 0;
                }
            }
            else if (_moving)
            {
                _motionController.SetTargetPosition(_navMeshAgent.nextPosition, 1.0f);
            }

            if (_steeringIndicator == null)
            {
                _steeringIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _steeringIndicator.transform.localScale = new Vector3(0.1f, 1, 0.1f);
                GameObject.Destroy(_steeringIndicator.GetComponent<BoxCollider>());
                _steeringIndicator.GetComponent<Renderer>().material.color = Color.red;

                _nextPositionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _nextPositionIndicator.transform.localScale = new Vector3(0.1f, 1, 0.1f);
                GameObject.Destroy(_nextPositionIndicator.GetComponent<BoxCollider>());
                _nextPositionIndicator.GetComponent<Renderer>().material.color = Color.blue;

                _destinationIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _destinationIndicator.transform.localScale = new Vector3(0.1f, 1, 0.1f);
                GameObject.Destroy(_destinationIndicator.GetComponent<BoxCollider>());
                _destinationIndicator.GetComponent<Renderer>().material.color = Color.green;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _stopping = false;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var cell = GGGrid.GetCellFromRay(ray, 1000f);
                if (cell == null) return;
                _steer = true;
                _navMeshAgent.Warp(transform.position);
                _navMeshAgent.SetDestination(cell.CenterPoint3D);
                _target = cell.CenterPoint3D;
            }

            _steeringIndicator.transform.position = _navMeshAgent.steeringTarget;
            _nextPositionIndicator.transform.position = _navMeshAgent.nextPosition;
            _destinationIndicator.transform.position = _navMeshAgent.destination;
        }

        private bool ReachedDestination()
        {
            return Vector3.Distance(transform.position, _target.Value) < 0.9f;
        }
    }
}