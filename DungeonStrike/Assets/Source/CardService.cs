using UnityEngine;

namespace DungeonStrike
{
    public class CardService : MonoBehaviour
    {
        private static CardService _instance;
        public static CardService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<CardService>()); }
        }

        public Sprite GreenIceLinkSprite;
        public Sprite GreyWoodLinkSprite;
        public Sprite LavaLinkSprite;
        public Sprite SciFiLinkSprite;
        public Sprite GreenIceAbilitySprite;
        public Sprite GreyWoodAbilitySprite;
        public Sprite LavaAbilitySprite;
        public Sprite SciFiAbilitySprite;
        public GameObject LinkPrefab;
        public GameObject AbilityPrefab;

        private CellSelectionService _cellSelectionService;
        private CharacterService _characterService;
        private LinkService _linkService;

        public void Start()
        {
            _cellSelectionService = CellSelectionService.Instance;
            _characterService = CharacterService.Instance;
            _linkService = LinkService.Instance;
        }

        public void PlayCard(Card card)
        {
            if (card.CardType != CardType.Link)
            {
                throw new System.ArgumentException("Unsupported card type");
            }
            Debug.Log("play card " + card.School);

            _cellSelectionService.EnterCellSelectionMode(card);
        }

        public void CellSelected(Card card, GGCell cell, GameObject selectionQuad)
        {
            CreateLink(card, cell, selectionQuad);
        }

        public CardBehaviour DrawCard(Vector3 startingPosition)
        {
            switch (Random.Range(0, 5))
            {
                case 0:
                case 1:
                    return DrawLinkCard(startingPosition);
                default:
                    return DrawAbilityCard(startingPosition);
            }
        }

        private CardBehaviour DrawLinkCard(Vector3 startingPosition)
        {
            var card = new Card();
            card.CardType = CardType.Link;
            card.School = Random.Range(0, 2) == 0 ? School.Ignis : School.Petra;
            card.Faction = Faction.Player;

            var sprite = card.School == School.Ignis ? LavaLinkSprite : GreenIceLinkSprite;
            var cardGameObject = Canvas.Instance.InstantiateObject(LinkPrefab, startingPosition);
            var cardBehaviour = cardGameObject.GetComponent<CardBehaviour>();
            cardBehaviour.CardState = CardState.BeingDrawn;
            cardBehaviour.CardFront = sprite;
            cardBehaviour.Card = card;

            return cardBehaviour;
        }

        private CardBehaviour DrawAbilityCard(Vector3 startingPosition)
        {
            var card = new Card();
            card.CardType = CardType.Ability;
            card.School = Random.Range(0, 2) == 0 ? School.Ignis : School.Petra;
            card.Faction = Faction.Player;

            var sprite = card.School == School.Ignis ? LavaAbilitySprite : GreenIceAbilitySprite;
            var cardGameObject = Canvas.Instance.InstantiateObject(AbilityPrefab, startingPosition);
            var cardBehaviour = cardGameObject.GetComponent<CardBehaviour>();
            cardBehaviour.CardState = CardState.BeingDrawn;
            cardBehaviour.CardFront = sprite;
            cardBehaviour.Card = card;

            return cardBehaviour;
        }

        private void CreateLink(Card card, GGCell cell, GameObject selectionQuad)
        {
            var character = _characterService.CurrentTurnCharacter();
            character.ActionsThisRound++;
            var link = new Link()
            {
                School = card.School,
                Faction = card.Faction
            };
            _linkService.AddLink(link);

            var ggobject = new GGObject();
        }
    }

}