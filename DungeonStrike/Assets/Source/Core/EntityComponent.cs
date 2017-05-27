using System.Threading.Tasks;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;

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
    /// <see cref="DungeonStrikeComponent.MessageType"/> and their Entity ID, meaning that a component
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
        public async Task<EntityComponent> Enable(LogContext parentContext)
        {
            Initialize(parentContext);
            ErrorHandler.CheckState(LifecycleState == ComponentLifecycleState.NotStarted,
                "Attempted to start entity component twice!");
            LifecycleState = ComponentLifecycleState.Starting;

            var entity = GetComponent<Entity>();
            if (entity == null)
            {
                throw ErrorHandler.NewException(
                        "EntityComponents can only be attached to GameObjects which contain the Entity component");
            }
            ErrorHandler.CheckState(entity.Initialized, "Entity component must be initialized before Awake()");

            var messageRouter = await GetService<MessageRouter>();

            if (MessageType != null)
            {
                messageRouter.RegisterEntityComponentForMessage(MessageType, entity.EntityId, this);
            }
            await OnEnableEntityComponent();

            LifecycleState = ComponentLifecycleState.Started;
            return this;
        }

        /// <summary>
        /// Should be used to implement any required setup logic for EntityComponents.
        /// </summary>
        protected virtual Task OnEnableEntityComponent()
        {
            return Async.Done;
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