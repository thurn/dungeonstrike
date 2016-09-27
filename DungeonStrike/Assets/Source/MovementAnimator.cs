using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class MovementAnimator : MonoBehaviour
    {
        public MovementStyle MovementStyle;
        private MotionController _motionController;
        private NavMeshAgent _navMeshAgent;
        private GameObject _steeringIndicator;
        private GameObject _nextPositionIndicator;
        private GameObject _destinationIndicator;
        private bool _moving;
        private bool _steer;
        private Vector3? _target;

        void Start()
        {
            _motionController = GetComponent<MotionController>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            MovementConstants.UpdateAgentForStyle(_navMeshAgent, MovementStyle.NoWeapon);
        }

        void Update()
        {
            if (_moving && ReachedDestination())
            {
                _motionController.ClearTarget();
                Debug.Log("ClearTarget");
                _navMeshAgent.ResetPath();
                _moving = false;
            }
            else if (_steer)
            {
                _motionController.SetTargetPosition(_navMeshAgent.steeringTarget, 1.0f);
                _moving = true;
                _steer = false;
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
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    _steer = true;
                    _navMeshAgent.SetDestination(hit.point);
                    _target = hit.point;
                }
            }

            _steeringIndicator.transform.position = _navMeshAgent.steeringTarget;
            _nextPositionIndicator.transform.position = _navMeshAgent.nextPosition;
            _destinationIndicator.transform.position = _navMeshAgent.destination;
        }

        private bool ReachedDestination()
        {
            return Vector3.Distance(transform.position, _target.Value) < 0.75f;
        }
    }
}