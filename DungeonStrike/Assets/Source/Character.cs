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
        public int MaxActionsThisRound = 2;
        public Faction Faction;

        public bool CanTakeAdditionAction()
        {
            if (MovesThisRound >= 2)
            {
                return false;
            }
            else if (MovesThisRound == 1)
            {
                return ActionsThisRound < MaxActionsThisRound - 1;
            }
            else
            {
                return ActionsThisRound < MaxActionsThisRound;
            }
        }
    }
}