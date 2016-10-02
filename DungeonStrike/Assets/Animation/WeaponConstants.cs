using UnityEngine;

namespace DungeonStrike
{
    public enum ModelType
    {
        AssaultCharacter,
        Orc,
        Goblin
    }

    public class WeaponConstants
    {
        public static GameObject EquipM16(Transform attachPoint, GameObject m16, ModelType modelType)
        {
            var item = GameObject.Instantiate(m16, attachPoint) as GameObject;
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
    }
}