namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
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
        Task<CommitStatusResult> Commit(CancellationToken token = default);
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

    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }

    public class CommitStatusResult
    {
        public CommitStatus Status { get; set; }
        public ObjectId LastCommit { get; set; }
        public ObjectId NewCommit { get; set; }

        public override string ToString() => $"[{Status}, P: {LastCommit}, N: {NewCommit}]";
    }

    public enum CommitStatus
    {
        Updated,
        Split,
        Noop,
        New,
        Concurrency

    }

    public class Aggregate<AggregateState> : IAggregate<AggregateState>
        where AggregateState : new()
    {
        private AggregateState _state;//, _originalState;
        private long _committedVersion;
        private MutationCommit<AggregateState> _newCommit;

        [BsonId]
        public ObjectId Id { get; set; }


        public string AggregateId { get; set; }

        public long AggregateVersion { get; set; }

        public long MutationVersion { get; set; }

        [BsonIgnore] public bool Exists => _committedVersion > -1;
        [BsonIgnore] public bool Mutated => _newCommit != null;

        public ObjectId LastCommit { get; set; }

        [BsonIgnore]
        public DateTime LastMutation => LastCommit.CreationTime;

        public DateTime? TimeToLive { get; set; }

        public AggregateState State { get => _state; set => _state = value; }

        public List<MutationCommit<AggregateState>> MutationCommits { get; set; }

        private IMongoCollection<Aggregate<AggregateState>> _collection;
        private AggregateSettings _settings;
        private IDateTime _dateTime;

        public override string ToString() => $"{{ Id: {AggregateId}, V: {AggregateVersion}, E: {Exists}, C: {LastCommit}, M: {MutationVersion}, S: {State}: H: {this.GetHashCode():X8} }}";

        // public Aggregate(IMongoCollection<Aggregate<AggregateState>> collection)
        // {
        //     _collection = collection;
        //     Mutations = new List<MutationCommit<AggregateState>>();
        // }

        internal void Initialize(IMongoCollection<Aggregate<AggregateState>> collection, AggregateSettings settings, IDateTime dateTime)
        {
            _collection = collection;
            _settings = settings;
            _dateTime = dateTime;
            _committedVersion = MutationVersion;
            //_originalState = BsonSerializer.Deserialize<AggregateState>(_state.ToBsonDocument());

            //we have to skip the first commit since the state already contains it
            var skip = AggregateVersion > 0 ? 1 : 0;
            foreach (var commits in MutationCommits.Skip(skip))
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

        public async Task<CommitStatusResult> Commit(CancellationToken token = default)
        {
            if (_newCommit == null)
                return new CommitStatusResult() { Status = CommitStatus.Noop, LastCommit = LastCommit, NewCommit = LastCommit };

            var lastCommit = LastCommit;
            if (MutationCommits.Count >= _settings.MaxMutationCommits || _committedVersion == -1)
            {
                //we need to split to a new version

                //if no commit we need to reset the state to default
                var oldState = State;
                if (_committedVersion == -1)
                {
                    State = new AggregateState();
                    //Console.WriteLine($"New: {AggregateId}");
                }
                else
                {
                    //Console.WriteLine($"Split: {AggregateId}");
                }


                Id = ObjectId.GenerateNewId();
                AggregateVersion++;
                LastCommit = _newCommit.CommitId;

                _committedVersion = MutationVersion;
                MutationCommits.Clear();
                MutationCommits.Add(_newCommit);
                _newCommit = null;
                try
                {
                    await _collection.InsertOneAsync(
                                          this,
                                          default(InsertOneOptions),
                                          token
                                      );
                }
                catch (MongoWriteException writeEx)
                {
                    if (writeEx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    {
                        return new CommitStatusResult() { Status = CommitStatus.Concurrency, LastCommit = lastCommit, NewCommit = LastCommit };
                        //throw new ConcurrencyException(AggregateId, AggregateVersion);
                    }

                    throw;
                }
                finally
                {
                    State = oldState;
                }

                if (AggregateVersion > 0)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var update = Builders<Aggregate<AggregateState>>.Update;
                        var updateDefintion =
                            update
                                .Set(x => x.TimeToLive, _dateTime.UtcNow);
                        //.Set(x => x.LastCommit, LastCommit);

                        var result = await _collection.UpdateOneAsync(
                            x => x.AggregateId == AggregateId && x.AggregateVersion == AggregateVersion - 1,
                            updateDefintion,
                            new UpdateOptions()
                            {
                                IsUpsert = false
                            },
                            token
                        );

                        if (result.ModifiedCount == 1)
                            break;

                        await Task.Delay(i * 50);
                    }

                    return new CommitStatusResult() { Status = CommitStatus.Split, LastCommit = lastCommit, NewCommit = LastCommit };
                }

                return new CommitStatusResult() { Status = CommitStatus.New, LastCommit = lastCommit, NewCommit = LastCommit };
            }
            else
            {
                var update = Builders<Aggregate<AggregateState>>.Update;
                var updateDefintion =
                    update
                        .Set(x => x.MutationVersion, MutationVersion)
                        .Set(x => x.LastCommit, _newCommit.CommitId)
                        .Push(x => x.MutationCommits, _newCommit);

                var result = await _collection.UpdateOneAsync(
                    x => x.AggregateId == AggregateId && x.AggregateVersion == AggregateVersion &&
                            x.MutationVersion == _committedVersion && x.LastCommit == LastCommit,
                    updateDefintion,
                    new UpdateOptions()
                    {
                        IsUpsert = false
                    },
                    token
                );
                //throw new ConcurrencyException(AggregateId, AggregateVersion);

                LastCommit = _newCommit.CommitId;
                _committedVersion = MutationVersion;
                MutationCommits.Add(_newCommit);
                _newCommit = null;

                if (result.ModifiedCount != 1)
                    return new CommitStatusResult() { Status = CommitStatus.Concurrency, LastCommit = lastCommit, NewCommit = LastCommit };

                return new CommitStatusResult() { Status = CommitStatus.Updated, LastCommit = lastCommit, NewCommit = LastCommit };
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
        private readonly IDateTime _dateTime;
        private readonly string _name = typeof(T).Name;

        public AggregateRepository(IAggregateCollectionFactory repositoryFactory, IDateTime dateTime, AggregateSettings settings = default)
        {
            //_database = database;
            _collection = repositoryFactory.GetCollection<T>();
            _dateTime = dateTime;
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

            record.Initialize(_collection, _settings, _dateTime);
            return record;
        }
    }
}