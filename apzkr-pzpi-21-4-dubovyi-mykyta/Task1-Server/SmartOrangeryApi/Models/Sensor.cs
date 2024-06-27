using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SmartOrangeryApi.Models.Enums;

namespace SmartOrangeryApi.Models
{
    public class Sensor : EntityBase
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId OrangeryId { get; set; }
        public SensorType Type { get; set; }
        public double LastValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
