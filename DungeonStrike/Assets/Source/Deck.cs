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
            Sprite sprite;
            switch (Random.Range(0, 4))
            {
                case 0:
                    sprite = IceSprite;
                    break;
                case 1:
                    sprite = WoodSprite;
                    break;
                case 2:
                    sprite = LavaSprite;
                    break;
                default:
                    sprite = SciFiSprite;
                    break;
            }
            Hand.Instance.Draw(transform.position, sprite);
        }
    }
}