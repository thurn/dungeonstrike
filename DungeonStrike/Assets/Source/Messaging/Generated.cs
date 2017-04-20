using System;

// =============================================================================
// WARNING: Do not modify this file by hand! This file is automatically
// generated by 'code_generator.clj' on driver startup from the message
// specifications found in 'messages.clj'. Refer to the documentation in those
// files for more information.
// =============================================================================

namespace DungeonStrike.Source.Messaging
{
    public enum SceneName
    {
        Empty,
        Flat
    }

    public enum EntityType
    {
        Orc,
        Soldier
    }

    public sealed class TestMessage : Message
    {
        public SceneName SceneName { get; set; }
    }

    public sealed class LoadSceneMessage : Message
    {
        public SceneName SceneName { get; set; }
    }

    public sealed class CreateEntityMessage : Message
    {
        public EntityType EntityType { get; set; }
        public Position Position { get; set; }
    }

    public sealed class DestroyEntityMessage : Message
    {
    }

    public sealed class MoveToPositionMessage : Message
    {
        public Position Position { get; set; }
    }

    public sealed class Messages
    {
        public static Message EmptyMessageForType(string messageType)
        {
            switch (messageType)
            {
                case "Test":
                    return new TestMessage();
                case "LoadScene":
                    return new LoadSceneMessage();
                case "CreateEntity":
                    return new CreateEntityMessage();
                case "DestroyEntity":
                    return new DestroyEntityMessage();
                case "MoveToPosition":
                    return new MoveToPositionMessage();
                default:
                    throw new InvalidOperationException(
                        "Unrecognized message type: " + messageType);
            }
        }
    }
}