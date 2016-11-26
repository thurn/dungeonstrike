﻿using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;

namespace DungeonStrike
{
    public enum Prefab
    {
        Unknown,
        VulcanMuzzle,
        VulcanImpact,
        VulcanProjectile
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
                default:
                    throw Preconditions.UnexpectedEnumValue(prefab);
            }
        }
    }
}