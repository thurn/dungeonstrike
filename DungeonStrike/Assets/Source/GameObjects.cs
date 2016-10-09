using UnityEngine;

namespace DungeonStrike
{
    public class GameObjects
    {
        public static Transform FindChildTransformWithTag(Transform transform, string tag)
        {
			if (transform == null)
			{
                throw new System.SystemException("Transform cannot be null!");
            }

            foreach (var childTransform in transform.GetComponentsInChildren<Transform>())
            {
                if (childTransform.tag == tag)
                {
                    return childTransform;
                }
            }
            throw new System.SystemException("Transform with tag not found: " + tag);
        }
    }
}