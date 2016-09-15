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

        public void Start()
        {
            _cellSelectionService = CellSelectionService.Instance;
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
    }

}