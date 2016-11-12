using UnityEngine;

namespace DungeonStrike
{
    public class MainController : MonoBehaviour
    {
        public GameObject[] Characters;
        public InputManager InputManager;
        private CharacterSelectionService _characterSelectionService;
        private WorldPointSelectionService _worldPointSelectionService;
        private int _currentCharacterNumber;
        private int _currentTargetNumber;

        void Start()
        {
            _currentCharacterNumber = 0;
            _characterSelectionService = new CharacterSelectionService();
            _worldPointSelectionService = new WorldPointSelectionService();

            UpdateSelection();
        }

        void Update()
        {
            _characterSelectionService.Update();
            _worldPointSelectionService.Update();
        }

        public void OnNext()
        {
            IncrementCharacterNumber(ref _currentCharacterNumber);
            UpdateSelection();
            InputManager.SetMessage("Selected character " + _currentCharacterNumber);
        }

        private void UpdateSelection()
        {
            _characterSelectionService.SelectCharacter("current", CurrentCharacter().transform, Color.green);
        }

        public void OnMove()
        {
            InputManager.SetMessage("Select movement destination");
            _worldPointSelectionService.GetUserSelectedWorldPoint((Vector3 point) =>
            {
                InputManager.SetMessage("Moving to selected position");
                var characterMovement = CurrentCharacter().GetComponent<CharacterMovement>();
                characterMovement.MoveToPoint(point);
            });
        }

        public void OnShoot()
        {
            InputManager.SetMessage("Shooting selected target...");
            var shooting = CurrentCharacter().GetComponent<CharacterShoot>();
            shooting.ShootAtTarget(CurrentTarget().transform);
        }

        public void OnTarget()
        {
            IncrementCharacterNumber(ref _currentTargetNumber);
            if (_currentTargetNumber == _currentCharacterNumber)
            {
				// Avoid self-targeting
                IncrementCharacterNumber(ref _currentTargetNumber);
            }
            _characterSelectionService.SelectCharacter("target", CurrentTarget().transform, Color.red);
            InputManager.SetMessage("Targeted character " + _currentTargetNumber);
        }

        public void OnEquip()
        {
            var characterWeapons = CurrentCharacter().GetComponent<CharacterWeapons>();
            characterWeapons.EquipOrHolsterWeapon();
        }

        public void OnCast()
        {

        }

        private void IncrementCharacterNumber(ref int number)
        {
            number = (number + 1) % Characters.Length;
        }

        private GameObject CurrentCharacter()
        {
            return Characters[_currentCharacterNumber];
        }

        private GameObject CurrentTarget()
        {
            return Characters[_currentTargetNumber];
        }
    }
}