
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

    public class Card
    {
        public CardType CardType;
        public School School;
    }
}