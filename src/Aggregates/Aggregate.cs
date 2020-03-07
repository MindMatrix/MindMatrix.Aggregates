namespace MindMatrix.Aggregates
{
    public class Aggregate<T>
    {
        private readonly string _aggregateId;
        private readonly T _state;
        private readonly EventList _eventList;
        private IMutationSerializer<T> _serializer;
        private IMutationTypeResolver _resolver;

        public long CommittedVersion => _eventList.CommittedVersion;
        public long Version => _eventList.Version;

        public string AggregateId => _aggregateId;
        public T State => _state;

        public Aggregate(string aggregateId, T state, EventList eventList, IMutationSerializer<T> serializer, IMutationTypeResolver resolver)
        {
            _aggregateId = aggregateId;
            _state = state;
            _eventList = eventList;
            _serializer = serializer;
            _resolver = resolver;

            foreach (var committed in eventList.CommittedEvents)
            {
                var mutationType = resolver.GetByName(committed.Type);
                var mutation = serializer.Deserialize(mutationType, committed.Data);
                apply(mutation);
            }
        }

        private void apply<Mutation>(Mutation mutation)
            where Mutation : IMutation<T>
        {
            mutation.Apply(_state);
        }

        public void Apply<Mutation>(Mutation mutation)
            where Mutation : IMutation<T>
        {
            apply(mutation);
            var mutationType = _resolver.GetByType(typeof(Mutation));
            var eventData = _serializer.Serialize(mutationType, mutation);
            _eventList.Append(mutationType.Name, eventData);
        }
    }
}