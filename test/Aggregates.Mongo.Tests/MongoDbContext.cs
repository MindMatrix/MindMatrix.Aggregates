namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Driver;

    public class MongoDbContext<T> : IAsyncDisposable
    {
        private readonly string _databaseId = "test_" + Guid.NewGuid().ToString().Replace("-", "");
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        public IMongoDatabase Database => _database;
        public IMongoClient Client => _client;

        public MongoDbContext(MutationTypeResolver<T> resolver)
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://127.0.0.1:27017?retryWrites=false");
            _client = new MongoClient(settings);
            _database = _client.GetDatabase(_databaseId);
            _database.CreateCollection("aggregates");

            foreach (var it in resolver.AllTypes)
                _database.CreateCollection($"event_{it.Name}");

            //var convetion = new ConventionProfile();
            var conventionPack = new ConventionPack();
            conventionPack.Add(new IgnoreIfDefaultConvention(true));
            ConventionRegistry.Register("Defaults", conventionPack, t => true);
        }

        private class C : ConventionBase, IConvention
        {
        }

        public async ValueTask DisposeAsync()
        {
            //await _client.DropDatabaseAsync(_databaseId);
        }
    }
}