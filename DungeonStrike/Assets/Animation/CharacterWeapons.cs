using UnityEngine;

namespace DungeonStrike
{
    public class CharacterWeapons : MonoBehaviour
    {
        private const string RightHandAttachPointTag = "RightHandAttachPoint";

        public ModelType ModelType;
        public WeaponType WeaponType;
        private Animator _animator;
        private Transform _rightHandAttachPoint;

        void Start()
        {
            _animator = GetComponent<Animator>();
            foreach (var childTransform in GetComponentsInChildren<Transform>())
            {
                if (childTransform.tag == RightHandAttachPointTag)
                {
                    _rightHandAttachPoint = childTransform;
                    Debug.Log("Got attach point: " + _rightHandAttachPoint);
                    break;
                }
            }

            WeaponConstants.EquipWeapon(_rightHandAttachPoint, WeaponType, ModelType);

            // Jump to a random frame to prevent different characters in the idle state
            // from synchronizing their movements
            _animator.Play("Idle", 0, UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}