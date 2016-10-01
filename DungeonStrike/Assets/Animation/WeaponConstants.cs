using UnityEngine;

namespace DungeonStrike
{
    public enum ModelType
    {
        AssaultCharacter
    }

    public class WeaponConstants
    {
        public static GameObject EquipM16(Transform attachPoint, GameObject m16, ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.AssaultCharacter:
                    var item = GameObject.Instantiate(m16, attachPoint) as GameObject;
                    item.transform.localPosition = new Vector3(0.03f, -0.01f, -0.1f);
                    item.transform.localEulerAngles = new Vector3(172f, -5f, 90f);
                    item.transform.localScale = Vector3.one;
                    return item;
				default:
					throw new System.SystemException("Unknown model type " + modelType);
            }
        }
    }
}