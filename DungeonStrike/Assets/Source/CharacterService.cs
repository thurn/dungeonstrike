using UnityEngine;
using UnityEngine.UI;

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
            _movementService.SetCurrentMover(CurrentActiveCharacter().gameObject);
            var current = CurrentActiveCharacter();
            CurrentCharacterText.text = current.Name;
        }

        public Character CurrentActiveCharacter()
        {
            return _allCharacters[_currentCharacterNumber];
        }
    }
}