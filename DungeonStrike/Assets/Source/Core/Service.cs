using System.Threading.Tasks;
using DungeonStrike.Source.Utilities;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Represents a singleton component attached to the <see cref="Root"/> object.
    /// </summary>
    /// <remarks>
    /// A Service is a component which only has one instance per scene. These components must be attached to the
    /// <see cref="Root"/> GameObject by being registered in <see cref="Root.Awake()"/>. The service instance can be
    /// obtained by calling <see cref="DungeonStrikeComponent.GetService{T}"/>. Services can receive messages by
    /// overriding <see cref="DungeonStrikeComponent.MessageType"/> as normal.
    /// </remarks>
    public abstract class Service : DungeonStrikeComponent
    {
        /// <summary>
        /// Registers this Service to receive messages requested based on <c>SupportedMessageTypes</c>. Services with
        /// setup logic should put it in <see cref="OnEnableService" />. This method should only be invoked from
        /// "Root"!
        /// </summary>
        public override async Task<DungeonStrikeComponent> Enable(LogContext parentContext)
        {
            Initialize(parentContext);
            ErrorHandler.CheckState(LifecycleState == ComponentLifecycleState.NotStarted,
                "Attempted to start service twice!");
            LifecycleState = ComponentLifecycleState.Starting;

            var root = GetComponent<Root>();
            ErrorHandler.CheckState(root != null, "Service components must be attached to the Root object!");

            if (GetType() != typeof(MessageRouter))
            {
                var messageRouter = await GetService<MessageRouter>();

                if (MessageType != null)
                {
                    messageRouter.RegisterServiceForMessage(MessageType, this);
                }
            }
            await OnEnableService();

            LifecycleState = ComponentLifecycleState.Started;
            return this;
        }

        /// <summary>
        /// Should be used to implement any required setup logic for services.
        /// </summary>
        protected virtual Task<Result> OnEnableService()
        {
            return Async.Success;
        }

        /// <summary>
        /// Standard Unity OnDisable method. Disable logic for services should be put in <see cref="OnDisableService"/>.
        /// </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();
            OnDisableService();
            LifecycleState = ComponentLifecycleState.NotStarted;
        }

        /// <summary>
        /// Should be used to implement any required tear-down logic for services.
        /// </summary>
        protected virtual void OnDisableService()
        {
        }
    }
}