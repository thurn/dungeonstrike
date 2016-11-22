using UnityEngine;

namespace DungeonStrike
{
    public enum ModelType
    {
        AssaultCharacter,
        Orc,
        Goblin,
        Ogre,
        Troll
    }

    public enum WeaponType
    {
        None,
        Rifle1,
        Rifle2,
    }

    public class WeaponConstants
    {
        private static string RandomColor()
        {
            switch (Random.Range(0, 5))
            {
                case 0:
                    return "Beige";
                case 1:
                    return "Black";
                case 2:
                    return "Blue";
                case 3:
                    return "Red";
                default:
                    return "Yellow";
            }
        }

        public static string AssetNameForWeapon(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle1:
                    return "ScifiRifle1AnimatedYellow";
                case WeaponType.Rifle2:
                    return "ScifiRifle2AnimatedRed";
                default:
                    throw Preconditions.UnexpectedEnumValue(weaponType);
            }
        }

        public static void EquipWeapon(Transform attachPoint, WeaponType weaponType, ModelType modelType, System.Action<Optional<GameObject>> onSuccess)
        {
            if (weaponType == WeaponType.None)
            {
                onSuccess(Optional<GameObject>.Empty);
                return;
            }

            var assetName = AssetNameForWeapon(weaponType);
            AssetLoaderService.Instance.InstantiateGameObject("guns", assetName, (GameObject weapon) =>
            {
                weapon.transform.SetParent(attachPoint);
                switch (weaponType)
                {
                    case WeaponType.Rifle1:
                        EquipRifle1(attachPoint, weapon, modelType);
                        break;
                    case WeaponType.Rifle2:
                        EquipRifle2(attachPoint, weapon, modelType);
                        break;
                    default:
                        throw Preconditions.UnexpectedEnumValue(weaponType);
                }
                if (onSuccess != null) onSuccess(Optional<GameObject>.Of(weapon));
            });
        }

        private static GameObject EquipRifle1(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.AssaultCharacter:
                    item.transform.localPosition = new Vector3(0f, -0.02f, -0.02f);
                    item.transform.localEulerAngles = new Vector3(178, -10, 90);
                    item.transform.localScale = new Vector3(1f, 1f, 1f);
                    return item;
                default:
                    throw Preconditions.UnexpectedEnumValue(modelType);
            }
        }

        private static GameObject EquipRifle2(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Orc:
                    item.transform.localPosition = new Vector3(-0.18f, 0.05f, 0.04f);
                    item.transform.localEulerAngles = new Vector3(-15, -100, -90);
                    item.transform.localScale = new Vector3(2f, 2f, 2f);
                    return item;
                default:
                    throw Preconditions.UnexpectedEnumValue(modelType);
            }
        }
    }
}