namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class MongoAggregateRepository<T> : IAggregateRepository<T>
        where T : new()
    {
        private class MongoAggregate
        {
            public ObjectId Id { get; set; }
            public long Version { get; set; }
            public T State { get; set; }

        }

        private class MongoAggregateWrapper : Aggregate<T>
        {
            public MongoAggregateWrapper(string aggregateId) : base(aggregateId)
            {
            }

            public MongoAggregateWrapper(string aggregateId, T state, long version) : base(aggregateId, state, version)
            {
            }

            internal void Commit() => commit();
        }

        private readonly List<MongoAggregateWrapper> _loadedAggregates = new List<MongoAggregateWrapper>();

        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<MongoAggregate> _aggregateCollection;
        private readonly Dictionary<string, IMongoCollection<BsonDocument>> _eventCollections = new Dictionary<string, IMongoCollection<BsonDocument>>();
        private readonly Dictionary<string, Aggregate<T>> _aggregates = new Dictionary<string, Aggregate<T>>();

        private readonly IMutationTypeResolver<T> _resolver;



        public MongoAggregateRepository(IMongoClient client, IMongoDatabase database, IMutationTypeResolver<T> resolver)
        {
            _client = client;
            _database = database;
            _resolver = resolver;
            _aggregateCollection = _database.GetCollection<MongoAggregate>("aggregates");
            foreach (var it in resolver.AllTypes)
                _eventCollections.Add(it.Name, _database.GetCollection<BsonDocument>($"event_{it.Name}"));
        }

        public Aggregate<T> Create()
        {
            var aggregate = new MongoAggregateWrapper(ObjectId.GenerateNewId().ToString());
            _loadedAggregates.Add(aggregate);
            return aggregate;
        }

        public async Task<Aggregate<T>> Get(string aggregateId, CancellationToken token = default)
        {
            var query = await _aggregateCollection.FindAsync(x => x.Id == ObjectId.Parse(aggregateId));
            var result = await query.FirstOrDefaultAsync();
            if (result == null)
                throw new Exception("Not found!"); //TODO: need to add proper exception

            var wrapped = new MongoAggregateWrapper(aggregateId, result.State, result.Version);
            _loadedAggregates.Add(wrapped);
            return wrapped;
        }

        public async Task<bool> SaveChanges(CancellationToken token = default)
        {
            using var session = await _client.StartSessionAsync(new ClientSessionOptions()
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new TransactionOptions(
                    ReadConcern.Majority,
                    ReadPreference.Nearest,
                    WriteConcern.WMajority,
                    TimeSpan.FromSeconds(10)
                )
            }, token);


            session.StartTransaction();
            try
            {
                foreach (var it in _loadedAggregates)
                {
                    if (it.HasChanges)
                    {
                        var aggregate = new MongoAggregate();
                        aggregate.Id = ObjectId.Parse(it.AggregateId);
                        aggregate.Version = it.Version;
                        aggregate.State = it.State;

                        if (!it.Exists)
                            await _aggregateCollection.InsertOneAsync(session, aggregate, default(InsertOneOptions), token);
                        else
                        {
                            var result = await _aggregateCollection.ReplaceOneAsync(session, Builders<MongoAggregate>.Filter.Eq(x => x.Version, it.CommittedVersion), aggregate, default(ReplaceOptions), token);
                            if (result.ModifiedCount != 1)
                                throw new Exception("Concurrency mismatch"); //TODO: concurrency exception
                        }


                        var baseVersion = it.CommittedVersion;
                        foreach (var ev in it.UncommittedEvents)
                        {
                            var mt = _resolver.GetByType(ev.GetType());
                            var eventCollection = _eventCollections[mt.Name];

                            var bdoc = new BsonDocument();
                            bdoc.Add("_id", ObjectId.GenerateNewId());
                            bdoc.Add("EventId", ++baseVersion);

                            var mutationData = ev.ToBsonDocument();
                            mutationData.Remove("_t");
                            bdoc.Add("Data", mutationData);

                            await eventCollection.InsertOneAsync(session, bdoc, default(InsertOneOptions), token);
                        }

                        //do something
                    }
                }
                await session.CommitTransactionAsync();
                foreach (var it in _loadedAggregates)
                    it.Commit();
            }
            catch //(Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }

            return true;
        }
    }
}