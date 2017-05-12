using System;

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
        public override void Enable(LogContext parentContext)
        {
            base.Enable(parentContext);
            ErrorHandler.CheckState(LifecycleState == ComponentLifecycleState.NotStarted,
                "Attempted to start service twice!");
            LifecycleState = ComponentLifecycleState.Starting;
            Root.BeganStartingService();

            var root = GetComponent<Root>();
            ErrorHandler.CheckState(root != null, "Service components must be attached to the Root object!");

            if (GetType() != typeof(MessageRouter))
            {
                var messageRouter = GetService<MessageRouter>();

                if (MessageType != null)
                {
                    messageRouter.RegisterServiceForMessage(MessageType, this);
                }
            }
            OnEnableService(() =>
            {
                LifecycleState = ComponentLifecycleState.Started;
                Root.FinishedStartingService();
            });
        }

        /// <summary>
        /// Should be used to implement any required setup logic for services.
        /// </summary>
        protected virtual void OnEnableService(Action onStarted)
        {
            onStarted();
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