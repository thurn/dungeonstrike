using UnityEngine;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class Deck : MonoBehaviour
    {
        public Sprite IceSprite;
        public Sprite WoodSprite;
        public Sprite LavaSprite;
        public Sprite SciFiSprite;

        public void Start()
        {
            StartCoroutine(DrawCardsAfterDelay());
        }

        private IEnumerator<WaitForSeconds> DrawCardsAfterDelay()
        {
            for (var i = 0; i < 7; ++i)
            {
                yield return new WaitForSeconds(0.1f);
                DrawCard();
            }
        }

        public void DrawCard()
        {
            Hand.Instance.Draw(transform.position);
        }
    }
}