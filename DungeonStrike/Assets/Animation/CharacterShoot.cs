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
				Debug.Log("AngleToTarget(): " + AngleToTarget());
            }

            if (_log)
            {
                Debug.DrawRay(_firingPoint.position, _firingPoint.forward * 5, Color.yellow);
            }
        }

        private float AngleToTarget()
        {
            var targetDir = Target.position - _firingPoint.position;
            var angle = Vector3.Angle(_firingPoint.forward, targetDir);
            // Use cross product to determine the 'direction' of the angle.
            var cross = Vector3.Cross(_firingPoint.forward, targetDir);
            return cross.y < 0 ? -angle : angle;
        }
    }
}