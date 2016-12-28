using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;

namespace DungeonStrike
{
    public enum Prefab
    {
        Unknown,
        VulcanMuzzle,
        VulcanImpact,
        VulcanProjectile,
        VulcanShotAudio,
        Fireball1,
        FireMeteor1,
        Frostbolt1,
    }

    public sealed class Prefabs
    {
        public static string GetAssetName(Prefab prefab)
        {
            switch (prefab)
            {
                case Prefab.VulcanMuzzle:
                    return "vulcan_muzzle";
                case Prefab.VulcanProjectile:
                    return "vulcan_projectile";
                case Prefab.VulcanImpact:
                    return "vulcan_impact";
                case Prefab.VulcanShotAudio:
                    return "vulcan_shot_audio";
                case Prefab.Fireball1:
                    return "Fireball1";
                case Prefab.FireMeteor1:
                    return "FireMeteor1";
                case Prefab.Frostbolt1:
                    return "Frostbolt1";
                case Prefab.Unknown:
                    throw Preconditions.UnexpectedEnumValue(prefab);
                default:
                    throw Preconditions.UnexpectedEnumValue(prefab);
            }
        }
    }
}