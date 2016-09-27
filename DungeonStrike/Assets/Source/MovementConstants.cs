using UnityEngine;

namespace DungeonStrike
{
    public enum MovementStyle
    {
        NoWeapon,
        Rifle,
        Pistol
    }

    class MovementConstants
    {
        public static void UpdateAgentForStyle(NavMeshAgent agent, MovementStyle style)
        {
            switch (style)
            {
                case MovementStyle.NoWeapon:
                    agent.speed = 1.8f;
                    agent.radius = 0.5f;
                    agent.acceleration = 5.0f;
                    agent.stoppingDistance = 0.0f;
                    agent.angularSpeed = 120f;
                    return;
                default:
                    return;
            }
        }
    }
}