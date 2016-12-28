using UnityEngine;

namespace DungeonStrike
{
    public class AnimationTransitions
    {
        public const string CastToIdle = "CastToIdle";

        public static bool IsInTransition(Animator animator, string name, int layer = 0)
        {
            return animator.IsInTransition(layer) && animator.GetAnimatorTransitionInfo(layer).IsUserName(name);
        }
    }
}