using UnityEngine;

namespace DungeonStrike
{
    public class AnimationStates
    {
        public const string ShootingEmptyState = "Empty State";
        public const string RifleIdle = "Rifle Idle";
        public const string NoWeaponIdle = "No Weapon Idle";

        public static bool IsCurrentStateIdle(Animator animator, int layerNumber = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(layerNumber).IsName(RifleIdle) ||
                animator.GetCurrentAnimatorStateInfo(layerNumber).IsName(NoWeaponIdle);
        }

        public static bool IsNextStateIdle(Animator animator, int layerNumber = 0)
        {
            return animator.GetNextAnimatorStateInfo(layerNumber).IsName(RifleIdle) ||
                animator.GetNextAnimatorStateInfo(layerNumber).IsName(NoWeaponIdle);
        }
    }
}