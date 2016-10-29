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

        void Start()
        {
            _currentCharacterNumber = 0;
            _characterSelectionService = new CharacterSelectionService();
            _worldPointSelectionService = new WorldPointSelectionService();

            _characterSelectionService.SelectCharacter(CurrentCharacter().transform);
        }

        void Update()
        {
            _characterSelectionService.Update();
            _worldPointSelectionService.Update();
        }

        public void OnNext()
        {
            _currentCharacterNumber = (_currentCharacterNumber + 1) % Characters.Length;
            _characterSelectionService.SelectCharacter(CurrentCharacter().transform);
            InputManager.SetMessage("Selected character " + _currentCharacterNumber);
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

        }

        public void OnTarget()
        {

        }

        private GameObject CurrentCharacter()
        {
            return Characters[_currentCharacterNumber];
        }
    }
}