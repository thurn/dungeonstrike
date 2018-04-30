using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DungeonStrike.Source.Messaging
{
    public abstract class UnionJsonConverter<T> : CustomCreationConverter<T>
    {
        /// <summary>
        /// Should not be invoked during the parsing process due to our implementation of <see cref="ReadJson"/>.
        /// </summary>
        public override T Create(Type objectType)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// JSON parsing implementation which creates strongly-typed T instances.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var type = (string) jObject.Property(GetTypeIdentifier());
            var target = GetEmptyObjectForType(type);
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }

        public abstract string GetTypeIdentifier();

        public abstract object GetEmptyObjectForType(string type);
    }
}