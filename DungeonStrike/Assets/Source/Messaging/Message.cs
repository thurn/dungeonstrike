using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DungeonStrike.Source.Messaging
{
    /// <summary>
    /// Base class for all generated Messages. Messages are values which are sent from the driver to the client when
    /// the game's state changes. The set of possible messages are specified in 'messages.clj', and code is generated
    /// by 'code_generator.clj' to create a strongly-typed interface for each message object.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Unique identifier for this message. Defaults to a randomly-generated message ID.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// ID of Entity which should consume this message, or null if this message is not scoped to any particular
        /// entity.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Type of this message, used to determine how to deserialize it.
        /// </summary>
        public string MessageType { get; set; }

        protected Message(string messageType)
        {
            MessageType = messageType;
        }

        public override string ToString()
        {
            return "<[" + MessageType + "] " + MessageId + ">";
        }

        public string ToJson()
        {
            var settings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            settings.Converters.Add(new StringEnumConverter());
            return JsonConvert.SerializeObject(this, settings);
        }

        /// <see cref="object.Equals(object)" />
        public override bool Equals(object obj)
        {
            return Equals(obj as Message);
        }

        /// <see cref="object.Equals(object)" />
        public bool Equals(Message obj)
        {
            return obj != null && MessageId.Equals(obj.MessageId);
        }

        /// <see cref="object.GetHashCode()" />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return MessageId.GetHashCode();
        }
    }
}