using MongoDB.Bson;
using Newtonsoft.Json;

namespace SmartOrangeryApi.Models
{
    public class Orangery : EntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId UserId { get; set; }
        public double OptimalTemperature { get; set; }
        public double OptimalHumidity { get; set; }
        public double OptimalLight { get; set; }
        public double OptimalCO2 { get; set; }
    }
}
