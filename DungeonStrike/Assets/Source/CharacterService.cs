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
        public EnergyBarFollowObject CurrentCharacterHealthBar;
        public FollowHelper FollowHelper;
        private int _selectedCharacterNumber;
        private MovementService _movementService;
        private AttackService _attackService;

        private void Start()
        {
            _movementService = MovementService.Instance;
            _attackService = AttackService.Instance;
            System.Array.Sort(_allCharacters, (a, b) => b.Agility.CompareTo(a.Agility));
            SelectCharacter(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                SelectCharacter((_selectedCharacterNumber + 1) % _allCharacters.Length);
            }

            var current = SelectedCharacter();
            CurrentCharacterText.text = current.Name;
            CurrentCharacterAttack.text = "" + current.Agility;
            CurrentCharacterDefense.text = "" + current.Fortitude;
            CurrentCharacterHealth.text = "" + current.CurrentHealth;
            CurrentCharacterFortitude.text = "" + current.Fortitude;
            CurrentCharacterAgility.text = "" + current.Agility;
            CurrentCharacterMind.text = "" + current.Mind;
        }

        public List<int> EnemiesOfCharacter(Character character)
        {
            return new List<int>() { 1, 4 };
        }

        public GameObject SelectCharacter(int number)
        {
            _selectedCharacterNumber = number;
            var characterObject = SelectedCharacter().gameObject;
            _movementService.SetCurrentMover(characterObject);
            CurrentCharacterHealthBar.followObject = characterObject;
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

        public int SelectedCharacterNumber()
        {
            return _selectedCharacterNumber;
        }
    }
}