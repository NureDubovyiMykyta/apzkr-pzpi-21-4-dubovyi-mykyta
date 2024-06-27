using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SmartOrangeryApi.Models
{
    public class EntityBase
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId Id { get; set; }

        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId CreatedById { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public bool IsDeleted { get; set; }

        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId? LastModifiedById { get; set; }

        public DateTime? LastModifiedDateUtc { get; set; }
    }
}
