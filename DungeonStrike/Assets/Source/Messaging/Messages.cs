namespace DungeonStrike.Assets.Source.Messaging
{
    public abstract class Message
    {
        public string MessageId { get; set; }
        public string EntityId { get; set; }
        public string MessageType { get; set; }
        public string GameVersion { get; set; }
    }

    public sealed class LoadSceneMessage : Message
    {
        public string SceneName { get; set; }
    }
}