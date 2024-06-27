using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SmartOrangeryApi
{
    public class ObjectIdJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ObjectId objectId)
            {
                writer.WriteValue(objectId.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return ObjectId.TryParse(token.ToString(), out var objectId) ? objectId : ObjectId.Empty;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectId);
        }
    }
}
