using UnityEngine;
using System.Collections.Generic;

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
        public Material GreenIceMaterial;
        public Material LavaMaterial;
        public Material SciFiMaterial;
        public Material GreyWoodMaterial;
        public Material FireballSelectionMaterial;
        public GameObject LinkPrefab;
        public GameObject AbilityPrefab;

        private CellSelectionService _cellSelectionService;
        private CharacterService _characterService;
        private LinkService _linkService;
        private AttackService _attackService;

        public void Start()
        {
            _cellSelectionService = CellSelectionService.Instance;
            _characterService = CharacterService.Instance;
            _linkService = LinkService.Instance;
            _attackService = AttackService.Instance;
        }

        public void PlayCard(Card card)
        {
            if (card.CardType == CardType.Link)
            {
                Material quadMaterial;
                switch (card.School)
                {
                    case School.Aeris:
                        quadMaterial = GreyWoodMaterial;
                        break;
                    case School.Aquis:
                        quadMaterial = SciFiMaterial;
                        break;
                    case School.Ignis:
                        quadMaterial = LavaMaterial;
                        break;
                    default: // School.Petra:
                        quadMaterial = GreenIceMaterial;
                        break;
                }
                _cellSelectionService.EnterCellSelectionMode(quadMaterial, card);
            }
            else
            {
                PlayAbilityCard(card);
            }
        }

        private void PlayAbilityCard(Card card)
        {
            switch (card.CardIdentity)
            {
                case CardIdentity.ArcaneMissile:
                    PlayArcaneMissileCard(card);
                    break;
                case CardIdentity.LightningBolt:
                    PlayLightningBoltCard(card);
                    break;
                case CardIdentity.Haste:
                    PlayHasteCard(card);
                    break;
                case CardIdentity.Fireball:
                    PlayFireballCard(card);
                    break;
                default:
                    throw new System.ArgumentException("Unsupported card type: " + card.CardIdentity);
            }
        }

        private void PlayArcaneMissileCard(Card card)
        {
            _attackService.MakeAttack(false, () => 3);
            _attackService.MakeAttack(false, () => 3);
            _attackService.MakeAttack(true, () => 3);
        }

        private void PlayLightningBoltCard(Card card)
        {
            _attackService.MakeAttack(true, () => _attackService.RollDice(3, 10));
        }

        private void PlayHasteCard(Card card)
        {
            var character = _characterService.CurrentTurnCharacter();
            character.MaxActionsThisRound += 2;
        }

        private void PlayFireballCard(Card card)
        {
            _cellSelectionService.EnterAreaSelectionMode(FireballSelectionMaterial, card, 1 /* radius */);
        }

        public void CellSelected(Card card, GGCell cell, GameObject selectionQuad)
        {
            if (card.CardType == CardType.Link)
            {
                CreateLink(card, cell, selectionQuad);
            }
        }

        public void AreaSelected(Card card, List<GGCell> cells, GameObject selectionQuad)
        {
            if (card.CardIdentity == CardIdentity.Fireball)
            {
                GameObject.Destroy(selectionQuad);
                Debug.Log("fireball ! " + cells.Count);
            }
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
            card.School = School.Ignis; //Random.Range(0, 2) == 0 ? School.Ignis : School.Petra;
            card.Faction = Faction.Player;

            if (card.School == School.Ignis)
            {
                switch (Random.Range(0, 5))
                {
                    case 0:
                        card.CardIdentity = CardIdentity.ArcaneMissile;
                        break;
                    case 1:
                        card.CardIdentity = CardIdentity.TimeStop;
                        break;
                    case 2:
                        card.CardIdentity = CardIdentity.LightningBolt;
                        break;
                    case 3:
                        card.CardIdentity = CardIdentity.Haste;
                        break;
                    case 4:
                        card.CardIdentity = CardIdentity.Fireball;
                        break;
                }
                card.CardIdentity = CardIdentity.Fireball;
            }
            else
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        card.CardIdentity = CardIdentity.SleepGlyph;
                        break;
                    case 1:
                        card.CardIdentity = CardIdentity.SummonMonster3;
                        break;
                    case 2:
                        card.CardIdentity = CardIdentity.Forecast;
                        break;
                    case 3:
                        card.CardIdentity = CardIdentity.MageArmor;
                        break;
                }
            }
            card.Name = NameForCard(card.CardIdentity);
            card.Cost = CostForCard(card.CardIdentity);
            card.RulesText = RulesTextForCard(card.CardIdentity);

            var sprite = card.School == School.Ignis ? LavaAbilitySprite : GreenIceAbilitySprite;
            var cardGameObject = Canvas.Instance.InstantiateObject(AbilityPrefab, startingPosition);
            var cardBehaviour = cardGameObject.GetComponent<CardBehaviour>();
            cardBehaviour.CardState = CardState.BeingDrawn;
            cardBehaviour.CardFront = sprite;
            cardBehaviour.Card = card;
            cardBehaviour.NameText.text = card.Name;
            cardBehaviour.CostText.text = "" + card.Cost;
            cardBehaviour.RulesText.text = card.RulesText;

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

        private int CostForCard(CardIdentity identity)
        {
            switch (identity)
            {
                case CardIdentity.Link:
                    return 0;
                case CardIdentity.ArcaneMissile:
                    return 3;
                case CardIdentity.TimeStop:
                    return 7;
                case CardIdentity.LightningBolt:
                    return 2;
                case CardIdentity.Haste:
                    return 2;
                case CardIdentity.Fireball:
                    return 5;
                case CardIdentity.SleepGlyph:
                    return 6;
                case CardIdentity.SummonMonster3:
                    return 3;
                case CardIdentity.Forecast:
                    return 1;
                case CardIdentity.MageArmor:
                    return 1;
            }

            throw new System.ArgumentException("Unknown card: " + identity);
        }

        private string NameForCard(CardIdentity identity)
        {
            switch (identity)
            {
                case CardIdentity.Link:
                    return "Link";
                case CardIdentity.ArcaneMissile:
                    return "Arcane Missile";
                case CardIdentity.TimeStop:
                    return "Time Stop";
                case CardIdentity.LightningBolt:
                    return "Lightning Bolt";
                case CardIdentity.Haste:
                    return "Haste";
                case CardIdentity.Fireball:
                    return "Fireball";
                case CardIdentity.SleepGlyph:
                    return "Sleep Glyph";
                case CardIdentity.SummonMonster3:
                    return "Summon Monster III";
                case CardIdentity.Forecast:
                    return "Forecast";
                case CardIdentity.MageArmor:
                    return "Mage Armor";
            }

            throw new System.ArgumentException("Unknown card: " + identity);
        }

        private string RulesTextForCard(CardIdentity identity)
        {
            switch (identity)
            {
                case CardIdentity.Link:
                    return "";
                case CardIdentity.ArcaneMissile:
                    return "Make three attacks. Each deals 3 force damage on hit.";
                case CardIdentity.TimeStop:
                    return "Take an extra turn after this one";
                case CardIdentity.LightningBolt:
                    return "Attack an enemy creature, dealing 3d10 damage on hit.";
                case CardIdentity.Haste:
                    return "Gain 2 actions this turn";
                case CardIdentity.Fireball:
                    return "Deals 4d6 fire damage to all creatures in area of effect, Agl/15 save for half damage.";
                case CardIdentity.SleepGlyph:
                    return "Enemy creatures within range of this glyph must succeed at a Mnd/15 check save or fall asleep.";
                case CardIdentity.SummonMonster3:
                    return "Summon a level 7-9 monster.";
                case CardIdentity.Forecast:
                    return "Look at the top 3 cards of your deck. Put one in your hand and shuffle the others back.";
                case CardIdentity.MageArmor:
                    return "Gain 5 damage reduction until your next turn";
            }

            throw new System.ArgumentException("Unknown card: " + identity);
        }
    }

}