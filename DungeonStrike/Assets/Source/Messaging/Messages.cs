using Newtonsoft.Json;

namespace DungeonStrike.Source.Messaging
{
    // Note:
    // All of the code in this file is documented in and based on the code in the file "messages.clj" in the driver.
    // Eventually, it could be automatically generated from that specification, but currently the structure is kept in
    // sync manually. If you change anything here, it must also be changed in "messages.clj" and vice-versa.


    public abstract class Message
    {
        public string MessageId { get; set; }
        public string EntityId { get; set; }
        public string MessageType { get; set; }
        public string GameVersion { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <see cref="object.Equals(object)" />
        public override bool Equals(object obj)
        {
            return Equals(obj as Message);
        }

        /// <see cref="object.Equals(object)" />
        public bool Equals(Message obj)
        {
            return obj != null && ToString().Equals(obj.ToString());
        }

        /// <see cref="object.GetHashCode()" />
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public sealed class LoadSceneMessage : Message
    {
        public const string MessageTypeString = "LoadScene";
        public string SceneName { get; set; }
    }
}