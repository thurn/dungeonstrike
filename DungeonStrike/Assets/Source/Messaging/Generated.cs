using System;

// =============================================================================
// WARNING: Do not modify this file by hand! This file is automatically
// generated by 'code_generator.clj' on driver startup from the message
// specifications found in 'messages.clj'. Refer to the documentation in those
// files for more information.
// =============================================================================

namespace DungeonStrike.Source.Messaging
{
    public enum EntityType
    {
        Soldier
    }

    public enum SceneName
    {
        Empty,
        Flat
    }

    public sealed class TestMessage : Message
    {
        public static readonly string Type = "Test";
        public SceneName SceneName { get; set; }
    }

    public sealed class LoadSceneMessage : Message
    {
        public static readonly string Type = "LoadScene";
        public SceneName SceneName { get; set; }
    }

    public sealed class QuitGameMessage : Message
    {
        public static readonly string Type = "QuitGame";
    }

    public sealed class CreateEntityMessage : Message
    {
        public static readonly string Type = "CreateEntity";
        public string NewEntityId { get; set; }
        public EntityType EntityType { get; set; }
        public Position Position { get; set; }
    }

    public sealed class DestroyEntityMessage : Message
    {
        public static readonly string Type = "DestroyEntity";
    }

    public sealed class MoveToPositionMessage : Message
    {
        public static readonly string Type = "MoveToPosition";
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
                case "QuitGame":
                    return new QuitGameMessage();
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