namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;

    public interface IMutation<T>
    {
        void Apply(T aggregate);
    }

    public interface IAggregate<AggregateState>
        where AggregateState : new()
    {
        string AggregateId { get; }
        long AggregateVersion { get; }
        long MutationVersion { get; }
        bool Exists { get; }
        bool Mutated { get; }
        DateTime LastMutation { get; }
        AggregateState State { get; }
        void Apply<Mutation>(Mutation mutation) where Mutation : IMutation<AggregateState>;
        Task Commit(CancellationToken token = default);
    }

    public interface IAggregateRepository<Aggregate>
        where Aggregate : new()
    {
        //Task Delete(string aggregateId);
        Task<IAggregate<Aggregate>> GetLatest(string aggregateId);
    }

    public class MutationEvent<Aggregate>
        where Aggregate : new()
    {

        public long EventId { get; set; }
        public IMutation<Aggregate> Mutation { get; set; }
    }

    public class MutationCommit<Aggregate>
        where Aggregate : new()
    {
        public ObjectId CommitId { get; set; }
        public List<MutationEvent<Aggregate>> MutationEvents { get; set; }
    }

    public class AggregateSettings
    {
        public int MaxMutationCommits = 100;
        public int DaysToKeyOldVersions = 100;
    }

    public class Aggregate<AggregateState> : IAggregate<AggregateState>
        where AggregateState : new()
    {
        private AggregateState _state;
        private long _committedVersion;
        private MutationCommit<AggregateState> _newCommit;

        [BsonId]
        public ObjectId Id { get; set; }

        public string AggregateId { get; set; }

        public long AggregateVersion { get; set; }

        public long MutationVersion { get; set; }

        [BsonIgnore] public bool Exists => _committedVersion > -1;
        [BsonIgnore] public bool Mutated => _newCommit != null;

        public DateTime LastMutation { get; set; }

        public DateTime? TimeToLive { get; set; }

        public AggregateState State { get => _state; set => _state = value; }

        public List<MutationCommit<AggregateState>> MutationCommits { get; set; }

        private IMongoCollection<Aggregate<AggregateState>> _collection;
        private AggregateSettings _settings;

        // public Aggregate(IMongoCollection<Aggregate<AggregateState>> collection)
        // {
        //     _collection = collection;
        //     Mutations = new List<MutationCommit<AggregateState>>();
        // }

        internal void Initialize(IMongoCollection<Aggregate<AggregateState>> collection, AggregateSettings settings)
        {
            _collection = collection;
            _settings = settings;
            _committedVersion = MutationVersion;

            foreach (var commits in MutationCommits)
                foreach (var mutationEvent in commits.MutationEvents)
                    mutationEvent.Mutation.Apply(_state);
        }

        public void Apply<Mutation>(Mutation mutation) where Mutation : IMutation<AggregateState>
        {
            mutation.Apply(_state);
            if (_newCommit == null)
            {
                _newCommit = new MutationCommit<AggregateState>();
                _newCommit.CommitId = ObjectId.GenerateNewId();
                _newCommit.MutationEvents = new List<MutationEvent<AggregateState>>();
            }

            _newCommit.MutationEvents.Add(new MutationEvent<AggregateState>()
            {
                EventId = ++MutationVersion,
                Mutation = mutation
            });
        }

        public async Task Commit(CancellationToken token = default)
        {
            if (_newCommit == null)
                return;

            var lastMutation = DateTime.UtcNow;
            if (MutationCommits.Count >= _settings.MaxMutationCommits || _committedVersion == -1)
            {
                //we need to split to a new version

                //if no commit we need to reset the state to default
                var oldState = State;
                if (_committedVersion == -1)
                    State = new AggregateState();

                Id = ObjectId.GenerateNewId();
                AggregateVersion++;
                LastMutation = lastMutation;

                _committedVersion = MutationVersion;
                MutationCommits.Clear();
                MutationCommits.Add(_newCommit);
                _newCommit = null;

                await _collection.InsertOneAsync(
                                      this,
                                      default(InsertOneOptions),
                                      token
                                  );
                State = oldState;
            }
            else
            {
                var update = Builders<Aggregate<AggregateState>>.Update;
                var updateDefintion =
                    update
                        .Set(x => x.MutationVersion, MutationVersion)
                        .Set(x => x.LastMutation, lastMutation)
                        .Push(x => x.MutationCommits, _newCommit);

                var result = await _collection.UpdateOneAsync(
                    x => x.AggregateId == AggregateId && x.AggregateVersion == AggregateVersion && x.MutationVersion == _committedVersion,
                    updateDefintion,
                    new UpdateOptions()
                    {
                        IsUpsert = false
                    },
                    token
                );

                if (result.ModifiedCount == 0)
                    throw new ConcurrencyException(AggregateId, AggregateVersion);

                LastMutation = lastMutation;
                _committedVersion = MutationVersion;
                MutationCommits.Add(_newCommit);
                _newCommit = null;
            }
        }
    }

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string aggregateId, long aggregateVersion)
            : base($"Failed to update aggregate: '{aggregateId}' with version: {aggregateVersion}")
        {

        }
    }

    public interface IAggregateCollectionFactory
    {

        IMongoCollection<Aggregate<T>> GetCollection<T>() where T : new();
    }

    public class AggregateCollectionFactory : IAggregateCollectionFactory
    {
        private readonly IMongoDatabase _database;
        private readonly ConcurrentDictionary<string, bool> _collectionNames = new ConcurrentDictionary<string, bool>();

        public AggregateCollectionFactory(IMongoDatabase database)
        {
            _database = database;

            //get list of all collection names
            var names = database.ListCollectionNames().ToList();
            foreach (var it in names)
                _collectionNames.TryAdd(it, true);
        }

        public IMongoCollection<Aggregate<T>> GetCollection<T>() where T : new()
        {
            var collectionName = typeof(T).Name;
            _collectionNames.GetOrAdd(collectionName, name =>
            {
                //create collection
                _database.CreateCollection(name);

                var collection = _database.GetCollection<Aggregate<T>>(name);

                var primaryIndex = new CreateIndexModel<Aggregate<T>>(
                    Builders<Aggregate<T>>.IndexKeys.Combine(
                            Builders<Aggregate<T>>.IndexKeys.Ascending(x => x.AggregateId),
                            Builders<Aggregate<T>>.IndexKeys.Descending(x => x.AggregateVersion)
                    ),
                    new CreateIndexOptions()
                    {
                        Unique = true,
                    }
                );

                collection.Indexes.CreateOne(primaryIndex);

                var ttlIndex = new CreateIndexModel<Aggregate<T>>(
                    Builders<Aggregate<T>>.IndexKeys.Ascending(x => x.TimeToLive),
                    new CreateIndexOptions()
                    {
                        ExpireAfter = TimeSpan.FromDays(100)
                    }
                );
                collection.Indexes.CreateOne(ttlIndex);


                return true;
            });

            return _database.GetCollection<Aggregate<T>>(collectionName);
        }
    }

    public class AggregateRepository<T> : IAggregateRepository<T>
        where T : new()
    {
        //private readonly IMongoClient _client;
        //private readonly IMongoDatabase _database;

        private readonly AggregateSettings _settings;
        private readonly IMongoCollection<Aggregate<T>> _collection;
        private readonly string _name = typeof(T).Name;

        public AggregateRepository(IAggregateCollectionFactory repositoryFactory, AggregateSettings settings = default)
        {
            //_database = database;
            _collection = repositoryFactory.GetCollection<T>();
            _settings = settings ?? new AggregateSettings();
        }

        public async Task<IAggregate<T>> GetLatest(string aggregateId)
        {
            var query = await _collection.FindAsync(x => x.AggregateId == aggregateId,
                new FindOptions<Aggregate<T>>()
                {
                    Sort = Builders<Aggregate<T>>.Sort.Descending(x => x.AggregateVersion),
                    Limit = 1
                }
            );

            var record = await query.FirstOrDefaultAsync();
            if (record == null)
            {
                record = new Aggregate<T>();
                record.AggregateId = aggregateId;
                record.AggregateVersion = -1;
                record.MutationCommits = new List<MutationCommit<T>>();
                record.MutationVersion = -1;
                record.State = new T();
            }

            record.Initialize(_collection, _settings);
            return record;
        }
    }
}