using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DungeonStrike.Source.Messaging
{
    /// <summary>
    /// Handles parsing JSON from the driver via JSON.Net and creating <see cref="Message"/> instances.
    /// </summary>
    public sealed class MessageConverter : CustomCreationConverter<Message>
    {
        /// <summary>
        /// Should not be invoked during the parsing process due to our implementation of <see cref="ReadJson"/>.
        /// </summary>
        public override Message Create(Type objectType)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// JSON parsing implementation which creates strongly-typed <see cref="Message"/> instances.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var type = (string) jObject.Property("MessageType");
            var target = Messages.EmptyMessageForType(type);
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }
    }
}