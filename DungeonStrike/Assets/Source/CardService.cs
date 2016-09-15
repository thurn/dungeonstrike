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

		public void PlayCard(Card card)
		{
			Debug.Log("play card");
		}
    }

}