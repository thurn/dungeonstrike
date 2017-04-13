﻿namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Represents a singleton component attached to the <see cref="Root"/> object.
    /// </summary>
    /// <remarks>
    /// A Service is a component which only has one instance per scene. These components must be attached to the
    /// <see cref="Root"/> GameObject by being registered in <see cref="Root.Awake()"/>. The service instance can be
    /// obtained by calling <see cref="DungeonStrikeComponent.GetService{T}"/>. Services can receive messages by
    /// overriding <see cref="DungeonStrikeComponent.SupportedMessageTypes"/> as normal.
    /// </remarks>
    public abstract class Service : DungeonStrikeComponent
    {
        /// <summary>
        /// Registers this Service to receive messages requested based on <c>SupportedMessageTypes</c>.
        /// </summary>
        /// <para>
        /// Note that if you override this method, you *must* call <c>base.Awake()</c> before running any other code.
        /// </para>
        protected override void Awake()
        {
            ErrorHandler.CheckNotNull("SupportedMessageTypes", SupportedMessageTypes);
            var root = GetComponent<Root>();
            ErrorHandler.CheckState(root != null, "Service components must be attached to the Root object!");
            var messageRouter = GetService<MessageRouter>();
            foreach (var messageType in SupportedMessageTypes)
            {
                messageRouter.RegisterServiceForMessage(messageType, this);
            }
        }
    }
}