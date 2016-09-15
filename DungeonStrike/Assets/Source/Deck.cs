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
            School school;
            switch (Random.Range(0, 4))
            {
                case 0:
                    sprite = IceSprite;
                    school = School.Petra;
                    break;
                case 1:
                    sprite = WoodSprite;
                    school = School.Aeris;
                    break;
                case 2:
                    sprite = LavaSprite;
                    school = School.Ignis;
                    break;
                default:
                    sprite = SciFiSprite;
                    school = School.Aquis;
                    break;
            }
            Hand.Instance.Draw(transform.position, sprite, school);
        }
    }
}