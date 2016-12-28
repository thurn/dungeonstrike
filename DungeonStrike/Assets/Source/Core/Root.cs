using UnityEngine;

namespace DungeonStrike.Core
{
    public class Root : MonoBehaviour
    {
        private static Root _instance;

        public static Root Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                var roots = Object.FindObjectsOfType<Root>();
                Errors.CheckState(roots.Length == 1, "Exactly one Root object must be created.");
                _instance = roots[0];
                return _instance;
            }
        }
    }
}