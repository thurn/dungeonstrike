using UnityEngine;
using UnityEngine.UI;

namespace DungeonStrike
{
    public class Card : MonoBehaviour
    {
        private Image _image;
        public Sprite CardFront;

        public void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Flip()
        {
            _image.sprite = CardFront;
        }
    }
}
