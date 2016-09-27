using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class MovementAnimator : MonoBehaviour
    {
        public Transform Target;
        public MovementStyle MovementStyle;
        private MotionController _motionController;
        private NavMeshAgent _navMeshAgent;
        private GameObject _indicator;
        private bool _moving;

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
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                _moving = true;
                _navMeshAgent.SetDestination(Target.position);
            }

            if (_moving && ReachedDestination())
            {
                _motionController.ClearTarget();
                Debug.Log("ClearTarget");
                //                _navMeshAgent.Stop();
                //                _navMeshAgent.ResetPath();
                _moving = false;
            }
            else if (_moving)
            {
                _motionController.SetTargetPosition(_navMeshAgent.nextPosition, 1.0f);
            }

            if (_indicator == null)
            {
                _indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _indicator.transform.localScale = new Vector3(0.1f, 1, 0.1f);
                GameObject.Destroy(_indicator.GetComponent<BoxCollider>());
            }

            _indicator.transform.position = _navMeshAgent.nextPosition;
        }

        private bool ReachedDestination()
        {
            return !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= 0.25f;
        }
    }
}