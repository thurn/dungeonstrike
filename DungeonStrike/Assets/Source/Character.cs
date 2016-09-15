using UnityEngine;

namespace DungeonStrike
{
    public class Character : MonoBehaviour
    {
        public int CharacterNumber;
        public string Name;
        public int MaxHealth;
        public int CurrentHealth;
        public int Agility;
        public int Fortitude;
        public int Mind;
        public int ActionsThisRound;
        public int MovesThisRound;
        public Faction Faction;

        public bool CanTakeAdditionAction()
        {
            if (MovesThisRound >= 2)
            {
                return false;
            }
            else if (MovesThisRound == 1)
            {
                return ActionsThisRound == 0;
            }
            else
            {
                return ActionsThisRound <= 1;
            }
        }
    }
}