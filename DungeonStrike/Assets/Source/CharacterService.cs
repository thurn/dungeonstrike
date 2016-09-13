using UnityEngine;

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
        private int _currentCharacterNumber;
        private MovementService _movementService;

        private void Start()
        {
            _currentCharacterNumber = 0;
            _movementService = MovementService.Instance;
            _movementService.SetCurrentMover(CurrentActiveCharacter().transform);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectNextCharacter();
                var current = CurrentActiveCharacter();
                Debug.Log("Active character: " + current.Name);
            }
        }

        private void SelectNextCharacter()
        {
            _currentCharacterNumber = (_currentCharacterNumber + 1) % _allCharacters.Length;
            _movementService.SetCurrentMover(CurrentActiveCharacter().transform);
        }

        public Character CurrentActiveCharacter()
        {
            return _allCharacters[_currentCharacterNumber];
        }
    }
}