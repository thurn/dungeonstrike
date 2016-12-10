using System.Collections.Generic;
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

            AddAnimationEvents();
        }

        public void EquipOrHolsterWeapon()
        {
            _animator.SetInteger("WeaponNumber", _rifleObject.HasValue ? 1 : 0);
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
            Debug.Log("AddAnimationEvents");
            var animationConfiguration = RootObject.GetComponent<AnimatorConfiguration>();
            animationConfiguration.AddAnimationCallback(_animator, GetType(),
                new List<AnimationDescription>
                {
                    new AnimationDescription()
                    {
                        ClipName = AnimationClips.HolsterRifle,
                        CallbackFunctionName = "OnHolsterRifle",
                        EventTime = 1.00f
                    },
                    new AnimationDescription()
                    {
                        ClipName = AnimationClips.EquipRifle,
                        CallbackFunctionName = "OnEquipRifle",
                        EventTime = 0.30f
                    },
                });
        }
    }
}