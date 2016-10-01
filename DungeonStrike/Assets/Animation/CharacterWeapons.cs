using UnityEngine;

namespace DungeonStrike
{
    public class CharacterWeapons : MonoBehaviour
    {
        public Transform RightHandAttachPoint;
        public GameObject M16;
        private GameObject _currentWeapon;

        void Start()
        {
            _currentWeapon = WeaponConstants.EquipM16(RightHandAttachPoint, M16, ModelType.AssaultCharacter);
        }
    }
}