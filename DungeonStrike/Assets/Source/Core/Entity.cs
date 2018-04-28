using System;
using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Represents a GameObject with a known identity in game logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DungeonStrike driver, the independent process which implements the actual game logic, manages objects
    /// and communicates to the Unity3D frontend about them. In order to distinguish these objects from regular Unity
    /// GameObjects, we refer to them as "entities". The Entity component is attached to GameObjects corresponding to
    /// entities. Adding the Entity component enables some special functionality such as <see cref="EntityId"/>-based
    /// message routing, as described in <see cref="DungeonStrikeComponent"/>.
    /// </para>
    /// <para>
    /// After adding this component to a GameObject, the caller *must* immediately invoke the <see cref="Initialize"/>
    /// method.
    /// </para>
    /// </remarks>
    public sealed class Entity : MonoBehaviour
    {
        /// <summary>
        /// Type string for this entity, as defined by the driver.
        /// </summary>
        public PrefabName PrefabName;

        /// <summary>
        /// ID for this entity. Entity IDs must be globally unique across all prefab types.
        /// </summary>
        public string EntityId;

        /// <summary>
        /// Whether or not this entity has been initialized via <see cref="Initialize"/>.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <param name="prefabName">Sets the name of the entity's prefab.</param>
        /// <param name="entityId">Sets the entity ID.</param>
        public void Initialize(PrefabName prefabName, string entityId)
        {
            if (entityId == null)
            {
                throw new ArgumentNullException(nameof(entityId));
            }
            PrefabName = prefabName;
            EntityId = entityId;
            Initialized = true;
        }

        public override string ToString()
        {
            return $"<{PrefabName} {EntityId}>";
        }
    }
}