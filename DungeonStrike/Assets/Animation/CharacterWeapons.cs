using UnityEngine;

namespace DungeonStrike
{
    public class CharacterWeapons : MonoBehaviour
    {
        private const string RightHandAttachPointTag = "RightHandAttachPoint";

        public ModelType ModelType;
        public WeaponType WeaponType;
        public GameObject RootObject;
        private Animator _animator;
        private Transform _rightHandAttachPoint;
        private bool _rifleEquipped;
        private GameObject _rifleObject;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _rightHandAttachPoint = Transforms.FindChildTransformWithTag(this.transform, RightHandAttachPointTag);

            WeaponConstants.EquipWeapon(_rightHandAttachPoint, WeaponType, ModelType, (GameObject rifle) =>
            {
                _rifleObject = rifle;
            });
            // Jump to a random frame to prevent different characters in the idle state
            // from synchronizing their movements
            _animator.Play(AnimationStates.RIFLE_IDLE, 0, UnityEngine.Random.Range(0.0f, 1.0f));
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
            Debug.Log("OnHolster " + this);
            Preconditions.CheckState(_rifleObject != null, "Cannot holster rifle twice");
            GameObject.Destroy(_rifleObject);
            _rifleObject = null;
        }

        public void OnEquipRifle()
        {
            Debug.Log("OnEquip " + this);
            WeaponConstants.EquipWeapon(_rightHandAttachPoint, WeaponType, ModelType, (GameObject rifle) =>
            {
                Preconditions.CheckState(_rifleObject == null, "Cannot equip rifle object twice.");
                _rifleObject = rifle;
            });
        }

        private void AddAnimationEvents()
        {
            var animationConfiguration = RootObject.GetComponent<AnimatorConfiguration>();
            animationConfiguration.AddAnimationCallback(_animator, AnimationClips.HOLSTER_RIFLE,
                "OnHolsterRifle", 1.00f);
            animationConfiguration.AddAnimationCallback(_animator, AnimationClips.EQUIP_RIFLE,
                "OnEquipRifle", 0.30f);
        }
    }
}