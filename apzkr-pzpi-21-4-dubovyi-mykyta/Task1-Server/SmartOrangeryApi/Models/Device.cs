using MongoDB.Bson;
using Newtonsoft.Json;
using SmartOrangeryApi.Models.Enums;

namespace SmartOrangeryApi.Models
{
    public class Device : EntityBase
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId OrangeryId { get; set; }
        public DeviceType Type { get; set; }
        public string Status { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
