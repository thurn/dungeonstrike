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

    public class CardBehaviour : MonoBehaviour
    {
        private Image _image;
        public CardState CardState;
        public Sprite CardFront;
        public Card Card;
        public Text CostText;
        public Text NameText;
        public Text RulesText;
        private int _siblingIndex;
        private Vector3 _originalPosition;
        private CardService _cardService;
        private CharacterService _characterService;

        public void Awake()
        {
            _image = GetComponent<Image>();
            _cardService = CardService.Instance;
            _characterService = CharacterService.Instance;
        }

        public void Flip()
        {
            _image.sprite = CardFront;
            if (CostText != null)
            {
                CostText.enabled = true;
            }

            if (NameText != null)
            {
                NameText.enabled = true;
            }

            if (RulesText != null)
            {
                RulesText.enabled = true;
            }
        }

        public void StartHover()
        {
            if (CardState == CardState.InHand)
            {
                transform.DOScale(new Vector3(1.3f, 1.3f, 1), 0.2f);
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
            CardState = CardState.BeingDragged;
        }

        public void OnDrag()
        {
            if (CardState == CardState.InHand || CardState == CardState.BeingDragged)
            {
                transform.position = Input.mousePosition;
            }
        }

        public void OnEndDrag()
        {
            var mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            var currentCharacter = _characterService.CurrentTurnCharacter();
            if (mousePosition.y < 0.25 || !currentCharacter.CanTakeAdditionAction())
            {
                // Card is still over UI, return to hand
                CardState = CardState.InHand;
                transform.DOMove(_originalPosition, 0.3f);
            }
            else
            {
                // Play card
                _cardService.PlayCard(Card);
                transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.1f);
                GameObject.Destroy(this.gameObject, 0.1f);
            }
        }
    }
}
