using UnityEngine;

namespace DungeonStrike
{
    public enum ModelType
    {
        AssaultCharacter,
        Orc,
        Goblin
    }

    public enum WeaponType
    {
        Weapon10mmPistol,
        Weapon12GaugeAutomaticShotgun,
        Weapon357MGunRevolver,
        Weapon38Revolver,
        Weapon44MGunRevolver,
        Weapon45AutomaticPistol,
        Weapon50AutomaticPistol,
        Weapon500MGunRevolver,
        Weapon9mmAutomaticPistol,
        Weapon9mmHeavyPistol,
        WeaponAGUAssaultRifle,
        WeaponBM4Shotgun,
        WeaponCowboyRifle,
        WeaponCzechMachinePistolExtended,
        WeaponCzechMachinePistol,
        WeaponFieldRifle,
        WeaponFrenchAssaultRifle,
        WeaponKalash,
        WeaponMG5Submachinegun,
        WeaponModel10Submachinegun,
        WeaponModel16AssaultRifle,
        WeaponModel37Shotgun,
        WeaponModel500ProtectorShotgun,
        WeaponModel700Rifle,
        WeaponModel82HeavySniperRifle,
        WeaponModel870Shotgun,
        WeaponSCAssaultRifle,
        WeaponSDVSniperRifle,
        WeaponSnubnosed38Revolver,
        WeaponUZSubmachinegun,
        WeaponUMGSubmachinegun,
    }

    public class WeaponConstants
    {
        public static string AssetNameForWeapon(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Weapon10mmPistol: return "10mm Pistol";
                case WeaponType.Weapon12GaugeAutomaticShotgun: return "12 Gauge Automatic Shotgun";
                case WeaponType.Weapon357MGunRevolver: return "357 M-gun Revolver";
                case WeaponType.Weapon38Revolver: return "38 Revolver";
                case WeaponType.Weapon44MGunRevolver: return "44 M-gun Revolver";
                case WeaponType.Weapon45AutomaticPistol: return "45 Automatic Pistol";
                case WeaponType.Weapon50AutomaticPistol: return "50 Automatic Pistol";
                case WeaponType.Weapon500MGunRevolver: return "500 M-gun Revolver";
                case WeaponType.Weapon9mmAutomaticPistol: return "9mm Automatic Pistol";
                case WeaponType.Weapon9mmHeavyPistol: return "9mm Heavy Pistol";
                case WeaponType.WeaponAGUAssaultRifle: return "AGU Assault Rifle";
                case WeaponType.WeaponBM4Shotgun: return "BM4 Shotgun";
                case WeaponType.WeaponCowboyRifle: return "Cowboy Rifle";
                case WeaponType.WeaponCzechMachinePistolExtended: return "Czech Machine Pistol Extended";
                case WeaponType.WeaponCzechMachinePistol: return "Czech Machine Pistol";
                case WeaponType.WeaponFieldRifle: return "Field Rifle";
                case WeaponType.WeaponFrenchAssaultRifle: return "French Assault Rifle";
                case WeaponType.WeaponKalash: return "Kalash";
                case WeaponType.WeaponMG5Submachinegun: return "MG5 Submachinegun";
                case WeaponType.WeaponModel10Submachinegun: return "Model 10 Submachinegun";
                case WeaponType.WeaponModel16AssaultRifle: return "Model 16 Assault Rifle";
                case WeaponType.WeaponModel37Shotgun: return "Model 37 Shotgun";
                case WeaponType.WeaponModel500ProtectorShotgun: return "Model 500 Protector Shotgun";
                case WeaponType.WeaponModel700Rifle: return "Model 700 Rifle";
                case WeaponType.WeaponModel82HeavySniperRifle: return "Model 82 Heavy Sniper Rifle";
                case WeaponType.WeaponModel870Shotgun: return "Model 870 Shotgun";
                case WeaponType.WeaponSCAssaultRifle: return "SC Assault Rifle";
                case WeaponType.WeaponSDVSniperRifle: return "SDV Sniper Rifle";
                case WeaponType.WeaponSnubnosed38Revolver: return "Snub-nosed 38 Revolver";
                case WeaponType.WeaponUZSubmachinegun: return "U-Z Submachinegun";
                case WeaponType.WeaponUMGSubmachinegun: return "UMG Submachinegun";
                default: throw new System.SystemException("Unknown weapon type " + weaponType);
            }
        }

        public static void EquipWeapon(Transform attachPoint, WeaponType weaponType, ModelType modelType)
        {
            var assetName = AssetNameForWeapon(weaponType);
            AssetLoaderService.Instance.InstantiateGameObject("guns", assetName, (GameObject weapon) =>
            {
                weapon.transform.SetParent(attachPoint);
                switch (weaponType)
                {
                    case WeaponType.WeaponSCAssaultRifle:
                        EquipSCAssaultRifle(attachPoint, weapon, modelType);
                        break;
                    case WeaponType.WeaponBM4Shotgun:
                        EquipBM4Shotgun(attachPoint, weapon, modelType);
                        break;
                    default:
                        throw new System.SystemException("Unknown weapon type " + weaponType);
                }
            });

        }

        private static GameObject EquipM16(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.AssaultCharacter:
                    item.transform.localPosition = new Vector3(0.03f, -0.01f, -0.1f);
                    item.transform.localEulerAngles = new Vector3(172f, -5f, 90f);
                    item.transform.localScale = Vector3.one;
                    return item;
                case ModelType.Orc:
                    item.transform.localPosition = new Vector3(0.0f, -0.02f, 0.0f);
                    item.transform.localEulerAngles = new Vector3(-18, -100, -90);
                    item.transform.localScale = new Vector3(2, 2, 2);
                    return item;
                case ModelType.Goblin:
                    item.transform.localPosition = new Vector3(-0.22f, 0.05f, 0f);
                    item.transform.localEulerAngles = new Vector3(-10, -100, -75);
                    item.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    return item;
                default:
                    throw new System.SystemException("Unknown model type " + modelType);
            }
        }

        private static GameObject EquipSCAssaultRifle(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Goblin:
                    item.transform.localPosition = new Vector3(-0.27f, 0.1f, 0.05f);
                    item.transform.localEulerAngles = new Vector3(0, -8, 70);
                    item.transform.localScale = new Vector3(1f, 1f, 1f);
                    return item;
                default:
                    throw new System.SystemException("Unknown model type " + modelType);
            }
        }

        private static GameObject EquipBM4Shotgun(Transform attachPoint, GameObject item, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Orc:
                    item.transform.localPosition = new Vector3(-0.62f, 0.16f, 0.05f);
                    item.transform.localEulerAngles = new Vector3(0, -15, 70);
                    item.transform.localScale = new Vector3(2f, 2f, 2f);
                    return item;
                default:
                    throw new System.SystemException("Unknown model type " + modelType);
            }
        }

    }
}