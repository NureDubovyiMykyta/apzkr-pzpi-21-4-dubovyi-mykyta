using MongoDB.Bson;
using MongoDB.Driver;
using SmartOrangeryApi.Models;
using SmartOrangeryApi.Models.Enums;

namespace SmartOrangeryApi.Services
{
    public class DeviceService
    {
        private readonly IMongoCollection<Device> _devices;

        public DeviceService(MongoDbContext context)
        {
            _devices = context.Devices;
        }

        public async Task<List<Device>> GetAsync() =>
            await _devices.Find(device => !device.IsDeleted).ToListAsync();

        public async Task<Device> GetAsync(string id) =>
            await _devices.Find<Device>(device => device.Id == new ObjectId(id) && !device.IsDeleted).FirstOrDefaultAsync();

        public async Task<List<Device>> GetByOrangeryIdAsync(string orangeryId) =>
            await _devices.Find(device => device.OrangeryId == new ObjectId(orangeryId) && !device.IsDeleted).ToListAsync();

        public async Task<Device> GetByTypeAndOrangeryIdAsync(DeviceType deviceType, string orangeryId)
        {
            return await _devices.Find(device => device.Type == deviceType && device.OrangeryId == new ObjectId(orangeryId) && !device.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Device device, ObjectId userId)
        {
            device.Id = ObjectId.GenerateNewId();
            device.CreatedById = userId;
            device.CreatedDateUtc = DateTime.UtcNow;
            device.IsDeleted = false;

            await _devices.InsertOneAsync(device);
        }

        public async Task UpdateAsync(string id, Device updatedDevice)
        {
            var existingDevice = await _devices.Find(device => device.Id == new ObjectId(id) && !device.IsDeleted).FirstOrDefaultAsync();

            if (existingDevice == null)
            {
                throw new KeyNotFoundException("Device not found");
            }

            updatedDevice.Id = existingDevice.Id;
            updatedDevice.CreatedById = existingDevice.CreatedById;
            updatedDevice.CreatedDateUtc = existingDevice.CreatedDateUtc;
            updatedDevice.IsDeleted = existingDevice.IsDeleted;
            updatedDevice.LastModifiedDateUtc = DateTime.UtcNow;

            await _devices.ReplaceOneAsync(device => device.Id == new ObjectId(id), updatedDevice);
        }

        public async Task RemoveAsync(string id, ObjectId userId)
        {
            var update = Builders<Device>.Update
                .Set(device => device.IsDeleted, true)
                .Set(device => device.LastModifiedById, userId)
                .Set(device => device.LastModifiedDateUtc, DateTime.UtcNow);

            await _devices.UpdateOneAsync(device => device.Id == new ObjectId(id), update);
        }
    }

}
