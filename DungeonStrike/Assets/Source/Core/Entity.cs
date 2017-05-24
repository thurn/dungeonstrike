using System;

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
    public sealed class Entity : DungeonStrikeComponent
    {
        /// <summary>
        /// Type string for this entity, as defined by the driver.
        /// </summary>
        public string EntityType { get; private set; }

        /// <summary>
        /// ID for this entity. Entity IDs must be globally unique across all entity types.
        /// </summary>
        public string EntityId { get; private set; }

        /// <summary>
        /// Whether or not this entity has been initialized via <see cref="Initialize"/>.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <param name="entityType">Sets the entity type.</param>
        /// <param name="entityId">Sets the entity ID.</param>
        public void Initialize(string entityType, string entityId)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (entityId == null)
            {
                throw new ArgumentNullException(nameof(entityId));
            }
            EntityType = entityType;
            EntityId = entityId;
            Initialized = true;
        }
    }
}