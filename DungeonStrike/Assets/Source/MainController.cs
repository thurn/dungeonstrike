using UnityEngine;
using System;

namespace DungeonStrike
{
    public class MainController : MonoBehaviour
    {
        public GameObject[] Characters;
        public InputManager InputManager;
        public F3DEffects F3DEffects;
        private CharacterSelectionService _characterSelectionService;
        private WorldPointSelectionService _worldPointSelectionService;
        private int _currentCharacterNumber;
        private int _currentTargetNumber;
        private Action _onConfirmTarget;

        private void Start()
        {
            _currentCharacterNumber = 0;
            _currentTargetNumber = -1;
            _characterSelectionService = new CharacterSelectionService();
            _worldPointSelectionService = new WorldPointSelectionService();

            UpdateSelection();
        }

        private void Update()
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
            _worldPointSelectionService.GetUserSelectedWorldPoint(point =>
            {
                InputManager.SetMessage("Moving to selected position");
                var characterMovement = CurrentCharacter().GetComponent<CharacterMovement>();
                characterMovement.MoveToPoint(point);
            });
        }

        public void OnShoot()
        {
            InputManager.SetMessage("Select Target");
            _onConfirmTarget = () =>
            {
                InputManager.SetMessage("Shooting selected target...");
                var shooting = CurrentCharacter().GetComponent<CharacterShoot>();
                shooting.ShootAtTarget(CurrentTarget().transform);
            };
        }

        public void OnConfirmTarget()
        {
            if (_onConfirmTarget != null)
            {
                _onConfirmTarget();
            }
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
        }

        public void OnEquip()
        {
            var characterWeapons = CurrentCharacter().GetComponent<CharacterWeapons>();
            characterWeapons.EquipOrHolsterWeapon();
        }

        public void OnCast()
        {
            InputManager.SetMessage("Select Spell Target");
            _onConfirmTarget = () =>
            {
                InputManager.SetMessage("Casting spell on selected target...");
                var characterSpellcasting = CurrentCharacter().GetComponent<CharacterSpellcasting>();
                characterSpellcasting.CastSpellWithTarget(CurrentTarget().transform);
            };
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
            Preconditions.CheckState(_currentTargetNumber != -1, "No target selected");
            return Characters[_currentTargetNumber];
        }
    }
}