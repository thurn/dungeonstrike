using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace DungeonStrike
{
    public class Hand : MonoBehaviour
    {
        private static Hand _instance;
        private List<CardBehaviour> _cards;
        public CardBehaviour CardPrefab;
        public Transform ShowCardPosition;

        public static Hand Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<Hand>()); }
        }

        public void Awake()
        {
            _cards = new List<CardBehaviour>();
        }

        public void Draw(Vector3 deckPosition, Sprite frontSprite)
        {
            var card = Canvas.Instance.InstantiateObject<CardBehaviour>(CardPrefab, deckPosition);
            card.CardState = CardState.BeingDrawn;
            card.CardFront = frontSprite;
            _cards.Add(card);
            DOTween.Sequence()
              .Append(card.transform.DOMove(new Vector2(0, 150), 0.5f).SetRelative())
              .Append(DOTween.Sequence()
                .Append(card.transform.DOMove(ShowCardPosition.position, 1.0f))
                .Insert(0, card.transform.DOScale(new Vector3(3, 3, 1), 1.0f))
                .Insert(0, card.transform.DORotate(new Vector3(0, 90, 0), 0.5f))
                .InsertCallback(0.5f, () => card.Flip())
                .Insert(0.5f, card.transform.DORotate(Vector3.zero, 0.5f)))
              .AppendInterval(0.5f)
              .Append(DOTween.Sequence()
                .Append(card.transform.DOMove(transform.position, 1.0f))
                .Insert(0, card.transform.DOScale(Vector3.one, 1.0f)))
              .AppendCallback(() =>
              {
                  card.CardState = CardState.InHand;
              });
            transform.Translate(180, 0, 0);
        }
    }
}