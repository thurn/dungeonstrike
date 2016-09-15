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