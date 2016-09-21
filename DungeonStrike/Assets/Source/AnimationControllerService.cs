using UnityEngine;

namespace DungeonStrike
{
    public class AnimationControllerService : MonoBehaviour
    {
        public Transform StartingTarget;
        private Animator _animator;
        private Vector3 _target;

        public void Start()
        {
            _animator = GetComponent<Animator>();
            SetTarget(StartingTarget.position);
        }

        public void Update()
        {

        }

        public void SetTarget(Vector3 target)
        {
            _target = target;
            var rotationQuaternion = Quaternion.LookRotation(_target - transform.position);
            var rotation = Quaternion.FromToRotation(transform.forward, _target - transform.position);
            var degrees = rotation.eulerAngles.y;
            var result = degrees > 180 ? degrees - 360 : degrees;
            Debug.Log("rotationDegrees: " + result);
        }

    }
}