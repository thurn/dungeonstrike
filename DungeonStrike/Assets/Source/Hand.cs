using DG.Tweening;
using UnityEngine;

namespace DungeonStrike
{
    public class Hand : MonoBehaviour
    {
        public CardBehaviour CardPrefab;
        public Transform ShowCardPosition;
        private static Hand _instance;
        private CardService _cardService;

        public void Start()
        {
            _cardService = CardService.Instance;
        }

        public static Hand Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<Hand>()); }
        }

        public void Draw(Vector3 deckPosition)
        {
            var cardObject = _cardService.DrawCard(deckPosition);

            DOTween.Sequence()
              .Append(cardObject.transform.DOMove(new Vector2(0, 150), 0.5f).SetRelative())
              .Append(DOTween.Sequence()
                .Append(cardObject.transform.DOMove(ShowCardPosition.position, 1.0f))
                .Insert(0, cardObject.transform.DOScale(new Vector3(3, 3, 1), 1.0f))
                .Insert(0, cardObject.transform.DORotate(new Vector3(0, 90, 0), 0.5f))
                .InsertCallback(0.5f, () => cardObject.Flip())
                .Insert(0.5f, cardObject.transform.DORotate(Vector3.zero, 0.5f)))
              .AppendInterval(0.5f)
              .Append(DOTween.Sequence()
                .Append(cardObject.transform.DOMove(transform.position, 1.0f))
                .Insert(0, cardObject.transform.DOScale(Vector3.one, 1.0f)))
              .AppendCallback(() =>
              {
                  cardObject.CardState = CardState.InHand;
              });
            transform.Translate(180, 0, 0);
        }
    }
}