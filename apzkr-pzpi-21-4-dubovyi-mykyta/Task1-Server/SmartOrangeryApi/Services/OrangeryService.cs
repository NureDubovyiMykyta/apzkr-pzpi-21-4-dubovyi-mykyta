using MongoDB.Bson;
using MongoDB.Driver;
using SmartOrangeryApi.Models;

namespace SmartOrangeryApi.Services
{
    public class OrangeryService
    {
        private readonly IMongoCollection<Orangery> _orangeries;
        private readonly IMongoCollection<Sensor> _sensors;

        public OrangeryService(MongoDbContext context)
        {
            _orangeries = context.Orangeries;
        }

        public async Task<List<Orangery>> GetAsync() =>
            await _orangeries.Find(orangery => !orangery.IsDeleted).ToListAsync();

        public async Task<Orangery> GetAsync(string id) =>
            await _orangeries.Find<Orangery>(orangery => orangery.Id == new ObjectId(id) && !orangery.IsDeleted).FirstOrDefaultAsync();

        public async Task<List<Orangery>> GetByUserIdAsync (string userId) =>
            await _orangeries.Find(orangery => orangery.UserId == new ObjectId(userId) && !orangery.IsDeleted).ToListAsync();

        public async Task<Orangery> GetBySensorIdAsync(string sensorId)
        {
            var sensor = await _sensors.Find(s => s.Id == new ObjectId(sensorId)).FirstOrDefaultAsync();
            return await _orangeries.Find(o => o.Id == sensor.OrangeryId).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Orangery orangery, ObjectId userId)
        {
            orangery.Id = ObjectId.GenerateNewId();
            orangery.UserId = userId;
            orangery.CreatedById = userId;
            orangery.CreatedDateUtc = DateTime.UtcNow;
            orangery.IsDeleted = false;

            await _orangeries.InsertOneAsync(orangery);
        }

        public async Task UpdateAsync(string id, Orangery updatedOrangery, ObjectId userId)
        {
            var existingOrangery = await _orangeries.Find(orangery => orangery.Id == new ObjectId(id) && !orangery.IsDeleted).FirstOrDefaultAsync();

            if (existingOrangery == null)
            {
                throw new KeyNotFoundException("Orangery not found");
            }

            updatedOrangery.Id = existingOrangery.Id;
            updatedOrangery.CreatedById = existingOrangery.CreatedById;
            updatedOrangery.CreatedDateUtc = existingOrangery.CreatedDateUtc;
            updatedOrangery.IsDeleted = existingOrangery.IsDeleted;
            updatedOrangery.LastModifiedById = userId;
            updatedOrangery.LastModifiedDateUtc = DateTime.UtcNow;

            await _orangeries.ReplaceOneAsync(orangery => orangery.Id == new ObjectId(id), updatedOrangery);
        }

        public async Task RemoveAsync(string id, ObjectId userId)
        {
            var update = Builders<Orangery>.Update
                .Set(orangery => orangery.IsDeleted, true)
                .Set(orangery => orangery.LastModifiedById, userId)
                .Set(orangery => orangery.LastModifiedDateUtc, DateTime.UtcNow);

            await _orangeries.UpdateOneAsync(orangery => orangery.Id == new ObjectId(id), update);
        }
    }

}
