using UnityEngine;
using UnityEngine.UI;
using EnergyBarToolkit;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class CharacterService : MonoBehaviour
    {
        private static CharacterService _instance;

        public static CharacterService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<CharacterService>()); }
        }

        public Character[] _allCharacters;
        public Text CurrentCharacterText;
        public Text CurrentCharacterAttack;
        public Text CurrentCharacterDefense;
        public Text CurrentCharacterHealth;
        public Text CurrentCharacterFortitude;
        public Text CurrentCharacterAgility;
        public Text CurrentCharacterMind;
        public EnergyBar CurrentCharacterHealthBar;
        public FollowHelper FollowHelper;
        private int _selectedCharacterNumber;
        private int _currentTurnCharacter;
        private MovementService _movementService;
        private AttackService _attackService;

        private void Start()
        {
            _movementService = MovementService.Instance;
            _attackService = AttackService.Instance;
            System.Array.Sort(_allCharacters, (a, b) => b.Agility.CompareTo(a.Agility));
            for (var i = 0; i < _allCharacters.Length; ++i)
            {
                _allCharacters[i].CharacterNumber = i;
            }
            SetCurrentTurnCharacter(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                var next = (_currentTurnCharacter + 1) % _allCharacters.Length;
                _currentTurnCharacter = next;
                SelectCharacter(_currentTurnCharacter);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                // Pass turn
                var currentTurn = CurrentTurnCharacter();
                if (currentTurn.ActionsThisRound == 1 || currentTurn.MovesThisRound == 2)
                {
                    _currentTurnCharacter++;
                    if (_currentTurnCharacter == _allCharacters.Length)
                    {
                        Debug.Log("End of round");
                    }
                    else
                    {
                        SetCurrentTurnCharacter(_currentTurnCharacter);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                // Hold
            }

            var current = SelectedCharacter();
            CurrentCharacterText.text = current.Name;
            CurrentCharacterAttack.text = "" + current.Agility;
            CurrentCharacterDefense.text = "" + current.Fortitude;
            CurrentCharacterHealth.text = "" + current.CurrentHealth;
            CurrentCharacterFortitude.text = "" + current.Fortitude;
            CurrentCharacterAgility.text = "" + current.Agility;
            CurrentCharacterMind.text = "" + current.Mind;
            CurrentCharacterHealthBar.valueCurrent = current.CurrentHealth;
            CurrentCharacterHealthBar.valueMax = current.MaxHealth;
        }

        public List<int> EnemiesOfCharacter(Character character)
        {
            switch (character.CharacterNumber)
            {
                case 0:
                case 2:
                case 3:
                    return new List<int>() { 1, 4 };
                case 1:
                case 4:
                    return new List<int>() { 0, 2, 3 };
            }
            throw new System.ArgumentException("Unknown character.");
        }

        public GameObject SetCurrentTurnCharacter(int number)
        {
            SelectCharacter(number);
            _currentTurnCharacter = number;
            var characterObject = CurrentTurnCharacter().gameObject;
            _movementService.SetCurrentMover(characterObject);
            return characterObject;
        }

        public GameObject SelectCharacter(int number)
        {
            _selectedCharacterNumber = number;
            var characterObject = SelectedCharacter().gameObject;
            var energyBarFollow = CurrentCharacterHealthBar.GetComponent<EnergyBarFollowObject>();
            energyBarFollow.followObject = characterObject;
            FollowHelper.FollowTarget = characterObject;
            return characterObject;
        }

        public Character GetCharacter(int characterNumber)
        {
            return _allCharacters[characterNumber];
        }

        public Character SelectedCharacter()
        {
            return _allCharacters[_selectedCharacterNumber];
        }

        public Character CurrentTurnCharacter()
        {
            return _allCharacters[_currentTurnCharacter];
        }

        public int SelectedCharacterNumber()
        {
            return _selectedCharacterNumber;
        }

        public int CurrentTurnCharacterNumber()
        {
            return _currentTurnCharacter;
        }
    }
}