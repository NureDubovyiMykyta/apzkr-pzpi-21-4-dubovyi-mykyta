using MongoDB.Bson;
using MongoDB.Driver;
using SmartOrangeryApi.Models;

namespace SmartOrangeryApi.Services
{
    public class SensorService
    {
        private readonly IMongoCollection<Sensor> _sensors;
        private readonly OrangeryService _orangeryService;
        private readonly DeviceService _deviceService;
        private readonly RegulationService _regulationService;

        public SensorService(MongoDbContext context, OrangeryService orangeryService, DeviceService deviceService, RegulationService regulationService)
        {
            _sensors = context.Sensors;
            _orangeryService = orangeryService;
            _deviceService = deviceService;
            _regulationService = regulationService;
        }

        public async Task<List<Sensor>> GetAsync() =>
            await _sensors.Find(sensor => !sensor.IsDeleted).ToListAsync();

        public async Task<Sensor> GetAsync(string id) =>
            await _sensors.Find<Sensor>(sensor => sensor.Id == new ObjectId(id) && !sensor.IsDeleted).FirstOrDefaultAsync();

        public async Task<List<Sensor>> GetByOrangeryIdAsync(string orangeryId) =>
            await _sensors.Find(sensor => sensor.OrangeryId == new ObjectId(orangeryId) && !sensor.IsDeleted).ToListAsync();

        public async Task<Sensor> CreateAsync(Sensor sensor, ObjectId userId)
        {
            sensor.Id = ObjectId.GenerateNewId();
            sensor.CreatedById = userId;
            sensor.CreatedDateUtc = DateTime.UtcNow;
            sensor.IsDeleted = false;

            await _sensors.InsertOneAsync(sensor);
            return sensor;
        }

        public async Task UpdateAsync(string id, Sensor updatedSensor, ObjectId userId)
        {
            var existingSensor = await _sensors.Find(sensor => sensor.Id == new ObjectId(id) && !sensor.IsDeleted).FirstOrDefaultAsync();

            if (existingSensor == null)
            {
                throw new KeyNotFoundException("Sensor not found");
            }

            updatedSensor.Id = existingSensor.Id;
            updatedSensor.CreatedById = existingSensor.CreatedById;
            updatedSensor.CreatedDateUtc = existingSensor.CreatedDateUtc;
            updatedSensor.IsDeleted = existingSensor.IsDeleted;
            updatedSensor.LastModifiedById = userId;
            updatedSensor.LastModifiedDateUtc = DateTime.UtcNow;

            await _sensors.ReplaceOneAsync(sensor => sensor.Id == new ObjectId(id), updatedSensor);
        }

        public async Task UpdateAsync(string id, Sensor updatedSensor)
        {
            var existingSensor = await _sensors.Find(sensor => sensor.Id == new ObjectId(id) && !sensor.IsDeleted).FirstOrDefaultAsync();

            if (existingSensor == null)
            {
                throw new KeyNotFoundException("Sensor not found");
            }

            updatedSensor.Id = existingSensor.Id;
            updatedSensor.CreatedDateUtc = existingSensor.CreatedDateUtc;
            updatedSensor.IsDeleted = existingSensor.IsDeleted;
            updatedSensor.LastUpdated = DateTime.UtcNow;

            await _sensors.ReplaceOneAsync(sensor => sensor.Id == new ObjectId(id), updatedSensor);
        }

        public async Task RemoveAsync(string id, ObjectId userId)
        {
            var update = Builders<Sensor>.Update
                .Set(sensor => sensor.IsDeleted, true)
                .Set(sensor => sensor.LastModifiedById, userId)
                .Set(sensor => sensor.LastModifiedDateUtc, DateTime.UtcNow);

            await _sensors.UpdateOneAsync(sensor => sensor.Id == new ObjectId(id), update);
        }
    }

}
