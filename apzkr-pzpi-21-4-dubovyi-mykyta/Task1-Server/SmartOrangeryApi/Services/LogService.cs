using MongoDB.Bson;
using MongoDB.Driver;
using SmartOrangeryApi.Models;

namespace SmartOrangeryApi.Services
{
    public class LogService
    {
        private readonly IMongoCollection<Log> _logs;

        public LogService(MongoDbContext context)
        {
            _logs = context.Logs;
        }

        public async Task<List<Log>> GetAsync() =>
            await _logs.Find(log => !log.IsDeleted).ToListAsync();

        public async Task<Log> GetAsync(string id) =>
            await _logs.Find<Log>(log => log.Id == new ObjectId(id) && !log.IsDeleted).FirstOrDefaultAsync();

        public async Task<List<Log>> GetBySensorIdAsync(string sensorId) =>
            await _logs.Find(log => log.SensorId == new ObjectId(sensorId) && !log.IsDeleted).ToListAsync();

        public async Task CreateAsync(Log log)
        {
            log.Id = ObjectId.GenerateNewId();
            log.CreatedDateUtc = DateTime.UtcNow;
            log.IsDeleted = false;

            await _logs.InsertOneAsync(log);
        }

        public async Task UpdateAsync(string id, Log updatedLog, ObjectId userId)
        {
            var existingLog = await _logs.Find(log => log.Id == new ObjectId(id) && !log.IsDeleted).FirstOrDefaultAsync();

            if (existingLog == null)
            {
                throw new KeyNotFoundException("Log not found");
            }

            updatedLog.Id = existingLog.Id;
            updatedLog.CreatedById = existingLog.CreatedById;
            updatedLog.CreatedDateUtc = existingLog.CreatedDateUtc;
            updatedLog.IsDeleted = existingLog.IsDeleted;
            updatedLog.LastModifiedById = userId;
            updatedLog.LastModifiedDateUtc = DateTime.UtcNow;

            await _logs.ReplaceOneAsync(log => log.Id == new ObjectId(id), updatedLog);
        }

        public async Task RemoveAsync(string id, ObjectId userId)
        {
            var update = Builders<Log>.Update
                .Set(log => log.IsDeleted, true)
                .Set(log => log.LastModifiedById, userId)
                .Set(log => log.LastModifiedDateUtc, DateTime.UtcNow);

            await _logs.UpdateOneAsync(log => log.Id == new ObjectId(id), update);
        }
    }

}
