using UnityEngine;

namespace DungeonStrike.Core
{
    public class DungeonStrikeBehaviour : MonoBehaviour
    {
        public Component GetSingleton<T>() where T : Component
        {
            return Root.Instance.GetComponent<T>();
        }
    }
}