namespace MindMatrix.Aggregates
{
    public class Aggregate<AggregateState> : IAggregate<AggregateState>
    {
        private readonly string _aggregateId;
        private readonly AggregateState _state;
        private readonly EventList<AggregateState> _eventList;
        public long CommittedVersion => _eventList.CommittedVersion;
        public long Version => _eventList.Version;
        public string AggregateId => _aggregateId;
        public AggregateState State => _state;

        public Aggregate(string aggregateId, AggregateState state, EventList<AggregateState> eventList)
        {
            _aggregateId = aggregateId;
            _state = state;
            _eventList = eventList;

            foreach (var committed in eventList.CommittedEvents)
                apply(committed.Mutation);
        }

        private void apply<Mutation>(Mutation mutation)
            where Mutation : IMutation<AggregateState>
        {
            mutation.Apply(_state);
        }

        public void Apply<Mutation>(Mutation mutation)
            where Mutation : IMutation<AggregateState>
        {
            apply(mutation);
            _eventList.Append(mutation);
        }
    }
}