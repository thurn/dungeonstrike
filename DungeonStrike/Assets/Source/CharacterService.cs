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

        private void Start()
        {
            _currentCharacterNumber = 0;
        }

        public Character CurrentActiveCharacter()
        {
            return _allCharacters[_currentCharacterNumber];
        }
    }
}