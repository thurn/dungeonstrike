using System;

namespace DungeonStrike.Assets.Source.Core
{
    public sealed class Root : DungeonStrikeBehavior
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
                var roots = FindObjectsOfType<Root>();
                if (roots.Length != 1)
                {
                    throw new InvalidOperationException("Exactly one Root object must be created.");
                }
                _instance = roots[0];
                return _instance;
            }
        }
    }
}