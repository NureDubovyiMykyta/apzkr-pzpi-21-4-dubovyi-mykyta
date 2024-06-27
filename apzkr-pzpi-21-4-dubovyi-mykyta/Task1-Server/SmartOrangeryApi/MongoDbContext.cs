using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartOrangeryApi.Models;

namespace SmartOrangeryApi
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Orangery> Orangeries => _database.GetCollection<Orangery>("Orangeries");
        public IMongoCollection<Device> Devices => _database.GetCollection<Device>("Devices");
        public IMongoCollection<Sensor> Sensors => _database.GetCollection<Sensor>("Sensors");
        public IMongoCollection<Log> Logs => _database.GetCollection<Log>("Logs");
    }
}