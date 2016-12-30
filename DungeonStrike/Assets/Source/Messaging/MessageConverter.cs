using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DungeonStrike.Assets.Source.Messaging
{
    public sealed class MessageConverter : CustomCreationConverter<Message>
    {
        public override Message Create(Type objectType)
        {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var type = (string) jObject.Property("MessageType");
            Message target;
            switch (type)
            {
                case "LoadScene":
                    target = new LoadSceneMessage();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }
    }
}