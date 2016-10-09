using System;
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
        AssaultRifle01,
        AssaultRifle02,
        AssaultRifle03,
        AssaultRifle04,
        AssaultRifle05,
        AssaultRifle06,
        AssaultRifle07,
        AssaultRifle08,
        AssaultRifle09,
        AssaultRifle10
    }

    public class WeaponConstants
    {
        public static string AssetNameForWeapon(WeaponType weaponType)
        {
            return System.Enum.GetName(typeof(WeaponType), weaponType);
        }

        public static void EquipWeapon(Transform attachPoint, WeaponType weaponType, ModelType modelType, Action<GameObject> onSuccess)
        {
            var assetName = AssetNameForWeapon(weaponType);
            AssetLoaderService.Instance.InstantiateGameObject("guns", assetName, (GameObject weapon) =>
            {
                weapon.transform.SetParent(attachPoint);
                switch (weaponType)
                {
                    case WeaponType.AssaultRifle01:
                        EquipAssaultRifle01(attachPoint, weapon, modelType);
                        break;
                    case WeaponType.AssaultRifle02:
                        EquipAssaultRifle02(attachPoint, weapon, modelType);
                        break;
                    default:
                        throw new System.SystemException("Unknown weapon type " + weaponType);
                }
                onSuccess(weapon);
            });
        }

        private static GameObject EquipAssaultRifle01(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Orc:
                    item.transform.localPosition = new Vector3(-0.30f, 0.10f, 0f);
                    item.transform.localEulerAngles = new Vector3(105, 80, 90);
                    item.transform.localScale = new Vector3(2f, 2f, 2f);
                    return item;
                default:
                    throw new System.SystemException("Unknown model type " + modelType);
            }
        }

        private static GameObject EquipAssaultRifle02(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.AssaultCharacter:
                    item.transform.localPosition = new Vector3(-0.04f, -0.04f, -0.10f);
                    item.transform.localEulerAngles = new Vector3(87, 0, 90);
                    item.transform.localScale = new Vector3(1f, 1f, 1f);
                    return item;
                default:
                    throw new System.SystemException("Unknown model type " + modelType);
            }
        }
    }
}