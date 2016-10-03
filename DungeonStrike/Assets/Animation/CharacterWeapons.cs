using UnityEngine;

namespace DungeonStrike
{
    public class CharacterWeapons : MonoBehaviour
    {
        public Transform RightHandAttachPoint;
        public ModelType ModelType;
        public WeaponType WeaponType;
        private Animator _animator;

        void Start()
        {
            _animator = GetComponent<Animator>();
            WeaponConstants.EquipWeapon(RightHandAttachPoint, WeaponType, ModelType);

            // Jump to a random frame to avoid different characters in the idle state
            // from synchronizing their movements
            _animator.Play("Idle", 0, UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}