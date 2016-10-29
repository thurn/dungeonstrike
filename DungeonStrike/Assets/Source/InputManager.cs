using UnityEngine;
using UnityEngine.UI;

namespace DungeonStrike
{
    public class InputManager : MonoBehaviour
    {
        public Text MessageText;
        public MainController MainController;
        public GameObject DebugPanel;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                OnMoveClicked();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                OnTargetClicked();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                OnShootClicked();
            } else if (Input.GetKeyDown(KeyCode.N))
            {
                OnNextClicked();
            } else if (Input.GetKeyDown(KeyCode.B))
            {
                OnDebugClicked();
            }
        }

        public void SetMessage(string message)
        {
            MessageText.text = message;
        }

        public void OnMoveClicked()
        {
            MainController.OnMove();
        }

        public void OnTargetClicked()
        {
            MainController.OnTarget();
        }

        public void OnShootClicked()
        {
            MainController.OnShoot();
        }

        public void OnNextClicked()
        {
            MainController.OnNext();
        }

        public void OnDebugClicked()
        {
            DebugPanel.SetActive(!DebugPanel.activeSelf);
        }
    }

}