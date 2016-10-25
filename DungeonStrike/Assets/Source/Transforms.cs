using UnityEngine;

namespace DungeonStrike
{
    public enum AngleType
    {
        Vertical,
        Horizontal

    }
    public class Transforms
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

        /// <summary>
        /// Computes the angle in degrees between <c>source.forward </c> and <c>target</c>.
        /// </summary>
        /// <param name="source">The source transform.</param>
        /// <param name="target">The target transform.</param>
        /// <param name="angleType">
        /// The type of angle to return, either vertical (the angle projected onto the source's forward/upward plane)
        /// or horizontal (the angle projected onto the source's forward/rightward plane).
        /// </param>
        /// <returns>
        /// The angle in degrees between <c>source.forward </c> and a vector from <c>source.position </c> to
        /// <c>target.position</c>. The angle is projected onto a two-dimensional plane indicated by
        /// <paramref name="angleType" />.
        /// </returns>
        public static float AngleToTarget(Transform source, Transform target, AngleType angleType = AngleType.Horizontal)
        {
            var projectionNormal = angleType == AngleType.Horizontal ? Vector3.up : source.right;
            var forwardDirection = Vector3.ProjectOnPlane(source.forward, projectionNormal);
            var targetDir = Vector3.ProjectOnPlane(target.position - source.position, projectionNormal);
            var angle = Vector3.Angle(forwardDirection, targetDir);
            // Use cross product to determine the 'direction' of the angle.
            var cross = Vector3.Cross(forwardDirection, targetDir);
            return cross.y < 0 ? -angle : angle;
        }

        /// <summary>Rounds an angle to the nearest 90 degrees.</summary>
        /// <param name="angle">Angle to round.</param>
        /// <returns>Whichever of -180, -90, 0, 90, or 180 is closest to <paramref name="angle" />.</returns>
        public static float ToRightAngle(float angle)
        {
            if (angle < -135) return -180.0f;
            if (angle < -45) return -90.0f;
            if (angle < 45) return 0.0f;
            if (angle < 135) return 90.0f;
            return 180.0f;
        }
    }
}