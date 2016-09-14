using UnityEngine;
using UnityEngine.UI;
using EnergyBarToolkit;

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
        private int _currentCharacterNumber;
        private MovementService _movementService;

        private void Start()
        {
            _movementService = MovementService.Instance;
            SelectCharacter(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectCharacter((_currentCharacterNumber + 1) % _allCharacters.Length);
            }
        }

        private void SelectCharacter(int number)
        {
            _currentCharacterNumber = number;
            var characterObject = CurrentActiveCharacter().gameObject;
            _movementService.SetCurrentMover(characterObject);
            var current = CurrentActiveCharacter();
            CurrentCharacterText.text = current.Name;
            CurrentCharacterAttack.text = "" + current.Agility;
            CurrentCharacterDefense.text = "" + current.Fortitude;
            CurrentCharacterHealth.text = "" + current.CurrentHealth;
            CurrentCharacterFortitude.text = "" + current.Fortitude;
            CurrentCharacterAgility.text = "" + current.Agility;
            CurrentCharacterMind.text = "" + current.Mind;
            CurrentCharacterHealthBar.followObject = characterObject;
        }

        public Character CurrentActiveCharacter()
        {
            return _allCharacters[_currentCharacterNumber];
        }
    }
}