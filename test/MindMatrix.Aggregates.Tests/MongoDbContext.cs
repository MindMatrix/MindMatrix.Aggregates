using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MindMatrix.Aggregates
{
    public class MongoDbContext<T> : IAsyncDisposable
        where T : new()
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        public IMongoDatabase Database => _database;
        private readonly IAggregateCollectionFactory _factory;
        private readonly IAggregateRepository<T> _repository;
        public IAggregateRepository<T> Repository => _repository;

        private readonly string _databaseId = "v" + Guid.NewGuid().ToString().Replace("-", "");
        private readonly AggregateSettings _settings;
        private readonly IDateTime _dateTime;
        public IDateTime DateTime => _dateTime;
        private readonly bool _destroy;
        public MongoDbContext(AggregateSettings settings = default, bool destroy = true, IDateTime dateTime = default, string databaseId = default)
        {
            _databaseId = databaseId ?? "v" + Guid.NewGuid().ToString().Replace("-", "");
            _dateTime = dateTime ?? new StaticDateTime();
            _destroy = destroy;
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            _client = new MongoClient(connectionSettings);
            _database = _client.GetDatabase(_databaseId);
            _factory = new AggregateCollectionFactory(_database);
            _settings = settings;
            _repository = new AggregateRepository<T>(_factory, _dateTime, _settings);
        }

        public async ValueTask DisposeAsync()
        {
            if (_destroy)
                await _client.DropDatabaseAsync(_databaseId);
        }
    }

}