namespace MindMatrix.Aggregates
{
    public interface IAggregateStreamEvent<T> : IAggregateMutator<T>
        where T : IAggregateRoot
    {
        AggregateStreamEventId Id { get; }
        AggregateStreamEventVersion Version { get; }
    }

    public abstract class AggregateStreamEvent<T> : IAggregateStreamEvent<T>
        where T : IAggregateRoot
    {
        public AggregateStreamEventId Id { get; }

        public AggregateStreamEventVersion Version { get; }

        private readonly IAggregateMutator<T> _mutation;

        public AggregateStreamEvent(AggregateStreamEventId id, AggregateStreamEventVersion version, IAggregateMutator<T> mutation)
        {
            Id = id;
            Version = version;
            _mutation = mutation;
        }

        public void Apply(T aggregate)
        {
            _mutation.Apply(aggregate);
        }
    }
}