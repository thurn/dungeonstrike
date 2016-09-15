﻿using UnityEngine;
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
        private int _siblingIndex;
        private Vector3 _originalPosition;
        private CardService _cardService;

        public void Awake()
        {
            _image = GetComponent<Image>();
            _cardService = CardService.Instance;
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
            if (mousePosition.y < 0.25)
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
