using System.Collections;

namespace DungeonStrike
{
    public enum SpellType
    {
        Fireball1,
        FireMeteor1,
        Frostbolt1,
    }

    public class SpellConstants
    {
        public static Prefab PrefabForSpell(SpellType spellType)
        {
            switch (spellType)
            {
                case SpellType.Fireball1:
                    return Prefab.Fireball1;
                case SpellType.FireMeteor1:
                    return Prefab.FireMeteor1;
                case SpellType.Frostbolt1:
                    return Prefab.Frostbolt1;
                default:
                    throw Preconditions.UnexpectedEnumValue(spellType);
            }
        }

        public static string AssetNameForSpell(SpellType spellType)
        {
            return Prefabs.GetAssetName(PrefabForSpell(spellType));
        }
    }
}