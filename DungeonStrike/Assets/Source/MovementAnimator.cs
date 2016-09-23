using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class MovementAnimator : MonoBehaviour
    {
        public Transform Target;
        public MotionController MotionController;
        public GameObject Cube;
        private NavMeshAgent _navMeshAgent;

        void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.speed = 1.65f;
            _navMeshAgent.radius = 2;
            _navMeshAgent.acceleration = 2;
            _navMeshAgent.stoppingDistance = 2;
            _navMeshAgent.angularSpeed = 240;
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                _navMeshAgent.SetDestination(Target.position);
            }

            if (ReachedDestination())
            {
                MotionController.ClearTarget();
            }
            else
            {
                MotionController.SetTargetPosition(_navMeshAgent.nextPosition, 1.0f);
                Cube.transform.position = _navMeshAgent.nextPosition;
            }
        }

        private bool ReachedDestination()
        {
            return !_navMeshAgent.pathPending &&
                _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
                (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f);
        }
    }
}