using UnityEngine;

namespace DungeonStrike
{
    public class Canvas : MonoBehaviour
    {
        private static Canvas _instance;

        public static Canvas Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<Canvas>()); }
        }

        public GameObject InstantiateObject(GameObject prefab, Vector3 position)
        {
            var value = (GameObject)Object.Instantiate(prefab, position, Quaternion.identity);
            value.transform.SetParent(this.transform, true /* worldPositionStays */);
            value.transform.localScale = Vector3.one;
            return value;
        }
    }
}