using UnityEngine;

namespace DungeonStrike
{
    public class CharacterSpellcasting : MonoBehaviour
    {
        private Vector3 _target;
        private CharacterTurning _characterTurning;
        private Animator _animator;
        private bool _casting;
        private float? _spineAngle;
        private Transform _spineBone;

        void Start()
        {
            _characterTurning = GetComponent<CharacterTurning>();
            _animator = GetComponent<Animator>();
            _spineBone = Transforms.FindChildTransformWithTag(this.transform, Tags.SpineBone);
        }

        void Update()
        {

        }

        void LateUpdate()
        {
            if (AnimationTransitions.IsInTransition(_animator, AnimationTransitions.CastToIdle))
            {
                _casting = false;
            }

            if (_casting)
            {
                _spineBone.eulerAngles = new Vector3(
                        _spineBone.eulerAngles.x,
                        _spineBone.eulerAngles.y + _spineAngle.Value,
                        _spineBone.eulerAngles.z);
            }
        }

        public void CastSpellWithTarget(Transform target)
        {
            _target = target.position;
            var angle = Transforms.AngleToTarget(transform, target.position);
            _characterTurning.TurnToAngle(angle, BeginCast);
        }

        private void BeginCast()
        {
            _spineAngle = Transforms.AngleToTarget(this.transform, _target);
            _casting = true;
            _animator.SetTrigger("Casting");
        }
    }
}