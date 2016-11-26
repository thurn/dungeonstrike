using UnityEngine;

namespace DungeonStrike
{
    public class CharacterWeapons : MonoBehaviour
    {
        public ModelType ModelType;
        public WeaponType WeaponType;
        public GameObject RootObject;
        private Animator _animator;
        private Transform _rightHandAttachPoint;
        private bool _rifleEquipped;
        private Optional<GameObject> _rifleObject;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _rightHandAttachPoint = Transforms.FindChildTransformWithTag(transform, Tags.RightHandAttachPoint);

            WeaponConstants.EquipWeapon(_rightHandAttachPoint, WeaponType, ModelType, (rifle) =>
            {
                _rifleObject = rifle;
            });

            // Jump to a random frame to prevent different characters in the idle state
            // from synchronizing their movements
            _animator.Play(AnimationStates.RifleIdle, 0, Random.Range(0.0f, 1.0f));
            _rifleEquipped = true;

            AddAnimationEvents();
        }

        public void EquipOrHolsterWeapon()
        {
            if (_rifleEquipped)
            {
                _animator.SetInteger("WeaponNumber", 1);
                _rifleEquipped = false;
            }
            else
            {
                _animator.SetInteger("WeaponNumber", 0);
                _rifleEquipped = true;
            }
        }

        public void OnHolsterRifle()
        {
            Preconditions.CheckState(_rifleObject.HasValue, "Cannot holster rifle twice");
            Destroy(_rifleObject.Value);
            _rifleObject = Optional<GameObject>.Empty;
        }

        public void OnEquipRifle()
        {
            WeaponConstants.EquipWeapon(_rightHandAttachPoint, WeaponType, ModelType, (rifle) =>
            {
                Preconditions.CheckState(!_rifleObject.HasValue, "Cannot equip rifle object twice.");
                _rifleObject = rifle;
            });
        }

        private void AddAnimationEvents()
        {
            var animationConfiguration = RootObject.GetComponent<AnimatorConfiguration>();
            animationConfiguration.AddAnimationCallback(_animator, AnimationClips.HolsterRifle,
                "OnHolsterRifle", 1.00f);
            animationConfiguration.AddAnimationCallback(_animator, AnimationClips.EquipRifle,
                "OnEquipRifle", 0.30f);
        }
    }
}