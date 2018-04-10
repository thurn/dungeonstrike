
namespace DungeonStrike
{
    public enum CardType
    {
        Link,
        Ability
    }

    public enum School
    {
        Aeris,
        Petra,
        Ignis,
        Aquis
    }

    public enum CardIdentity
    {
        Link,
        ArcaneMissile,
        TimeStop,
        LightningBolt,
        Haste,
        Fireball,
        SleepGlyph,
        SummonMonster3,
        Forecast,
        MageArmor,
    }

    public class Card
    {
        public CardType CardType;
        public School School;
        public Faction Faction;
        public CardIdentity CardIdentity;
        public string Name;
        public int Cost;
        public string RulesText;
    }
}