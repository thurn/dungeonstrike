using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;
using Color = UnityEngine.Color;

namespace DungeonStrike.Source.Specs
{
    public interface ISpec
    {
        void UpdateGameObject(GameObject gameObject, object value);

        ErrorHandler ErrorHandler { get; set; }
        AssetRefs AssetRefs { get; set;  }
    }

    public abstract class Spec<T> : ISpec
    {
        public void UpdateGameObject(GameObject gameObject, object value)
        {
            Update(gameObject, (T) value);
        }

        protected abstract void Update(GameObject gameObject, T value);

        public ErrorHandler ErrorHandler { get; set; }
        public AssetRefs AssetRefs { get; set;  }

        protected static TC GetOrCreateComponent<TC>(GameObject gameObject) where TC : Component
        {
            var result = gameObject.GetComponent<TC>();
            if (result == null)
            {
                result = gameObject.AddComponent<TC>();
            }
            return result;
        }

        protected static Color ToUnityColor(Messaging.Color color)
        {
            return color == null ? Color.white : new Color(color.R, color.G, color.B, color.A);
        }

        protected static UnityEngine.Vector3 ToUnityVector(Messaging.Vector3 vector)
        {
            return vector == null ? UnityEngine.Vector3.zero : new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
        }

        protected static UnityEngine.Vector2 ToUnityVector(Messaging.Vector2 vector)
        {
            return vector == null ? UnityEngine.Vector2.zero : new UnityEngine.Vector2(vector.X, vector.Y);
        }
    }
}