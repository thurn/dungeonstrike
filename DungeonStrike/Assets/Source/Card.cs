using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DungeonStrike
{
    public enum CardState
    {
        BeingDrawn,
        InHand,
        BeingDragged
    }

    public class Card : MonoBehaviour
    {
        private Image _image;
        public CardState CardState;
        public Sprite CardFront;
        private int _siblingIndex;
        private Vector3 _originalPosition;

        public void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Flip()
        {
            _image.sprite = CardFront;
        }

        public void StartHover()
        {
            if (CardState == CardState.InHand)
            {
                transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.2f);
                _siblingIndex = transform.GetSiblingIndex();
                transform.SetSiblingIndex(99999);
            }
        }

        public void StopHover()
        {
            if (CardState == CardState.InHand)
            {
                transform.DOScale(new Vector3(1, 1, 1), 0.2f);
                transform.SetSiblingIndex(_siblingIndex);
            }
        }

        public void OnBeginDrag()
        {
            _originalPosition = transform.position;
        }

        public void OnDrag()
        {
            if (CardState == CardState.InHand)
            {
                transform.position = Input.mousePosition;
            }
        }

        public void OnEndDrag()
        {
            transform.DOMove(_originalPosition, 0.3f);
        }
    }
}
