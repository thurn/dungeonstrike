using DungeonStrike.Source.Messaging;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Represents a component which is attached to an entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Entity is a GameObject which has a specific identity in game logic, represented by the fact that it has
    /// the <see cref="Entity"/> component attached to it containing its <see cref="Entity.EntityId"/>. It is an error
    /// to add a component which extends <c>EntityComponent</c> to a GameObject which is not an Entity.
    /// </para>
    /// <para>
    /// EntityComponents participate in message routing keyed based on both their
    /// <see cref="DungeonStrikeComponent.SupportedMessageTypes"/> and their Entity ID, meaning that a component
    /// instance will only receive a message if both <see cref="Message.MessageType"/> and
    /// <see cref="Message.EntityId"/> match.
    /// </para>
    /// </remarks>
    public abstract class EntityComponent : DungeonStrikeComponent
    {
        /// <summary>
        /// Registers this EntityComponent to receive messages requested directed at its specific EntityID. Subclasses
        /// should put their setup logic in <see cref="OnEnableEntityComponent"/>.
        /// </summary>
        protected sealed override void OnEnable()
        {
            base.OnEnable();
            if (LifecycleState != ComponentLifecycleState.NotStarted) return;
            LifecycleState = ComponentLifecycleState.Starting;

            ErrorHandler.CheckNotNull("SupportedMessageTypes", SupportedMessageTypes);
            var entity = GetComponent<Entity>();
            if (entity == null)
            {
                ErrorHandler.ReportError(
                    "EntityComponents can only be attached to GameObjects which contain the Entity component");
                return;
            }
            ErrorHandler.CheckState(entity.Initialized, "Entity component must be initialized before Awake()");

            var messageRouter = GetService<MessageRouter>();

            foreach (var messageType in SupportedMessageTypes)
            {
                messageRouter.RegisterEntityComponentForMessage(messageType, entity.EntityId, this);
            }
            OnEnableEntityComponent();

            LifecycleState = ComponentLifecycleState.Started;
        }

        /// <summary>
        /// Should be used to implement any required setup logic for EntityComponents.
        /// </summary>
        protected virtual void OnEnableEntityComponent()
        {
        }

        /// <summary>
        /// Standard Unity disable callback. Subclasses should put any required tear-down logic in
        /// <see cref="OnDisableEntityComponent"/>.
        /// </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();
            OnDisableEntityComponent();
            LifecycleState = ComponentLifecycleState.NotStarted;
        }

        /// <summary>
        /// Should be used to implement any required tear-down logic for EntityComponents.
        /// </summary>
        protected virtual void OnDisableEntityComponent()
        {
        }
    }
}