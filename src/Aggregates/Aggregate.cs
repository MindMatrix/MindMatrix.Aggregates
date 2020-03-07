namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public class Aggregate<AggregateState> //: IAggregate<AggregateState>
        where AggregateState : new()
    {
        private readonly string _aggregateId;
        private readonly AggregateState _state;
        public string AggregateId => _aggregateId;
        public AggregateState State => _state;

        private readonly List<IMutation<AggregateState>> _events = new List<IMutation<AggregateState>>();
        private readonly List<IMutation<AggregateState>> _uncommittedEvents = new List<IMutation<AggregateState>>();
        private long _baseVersion;
        public long CommittedVersion => _baseVersion + _events.Count;
        public long Version => CommittedVersion + _uncommittedEvents.Count;

        public bool Exists => CommittedVersion > -1;
        public bool HasChanges => CommittedVersion != Version;

        public Aggregate(string aggregateId) : this(aggregateId, new AggregateState(), -1)
        {

        }

        public Aggregate(string aggregateId, AggregateState state, long version)
        {
            _aggregateId = aggregateId;
            _baseVersion = version;
            _state = state;
        }

        public void Apply<Mutation>(Mutation mutation)
            where Mutation : IMutation<AggregateState>
        {
            mutation.Apply(_state);
            _uncommittedEvents.Add(mutation);
        }

        public IReadOnlyList<IMutation<AggregateState>> CommittedEvents => _events;
        public IReadOnlyList<IMutation<AggregateState>> UncommittedEvents => _uncommittedEvents;

        public void Commit()
        {
            _events.AddRange(_uncommittedEvents);
            _uncommittedEvents.Clear();
        }
    }
}