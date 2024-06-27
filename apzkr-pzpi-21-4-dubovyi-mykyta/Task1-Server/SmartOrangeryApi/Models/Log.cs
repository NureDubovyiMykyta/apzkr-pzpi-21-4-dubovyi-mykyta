using MongoDB.Bson;
using Newtonsoft.Json;

namespace SmartOrangeryApi.Models
{
    public class Log : EntityBase
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId SensorId { get; set; }
        public double Value { get; set; }
        public DateTime Time { get; set; }
    }
}
