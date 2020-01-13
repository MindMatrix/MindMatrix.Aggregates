namespace MindMatrix.Aggregates
{
    public interface IAggregateEvent
    {
        void Apply();
    }

    public class AggregateEvent<T> : IAggregateEvent
        where T : IAggregateRoot
    {
        private readonly T _aggregate;
        private readonly IAggregateMutator<T> _mutation;

        public AggregateEvent(T aggregate, IAggregateMutator<T> mutation)
        {
            _aggregate = aggregate;
            _mutation = mutation;
        }

        public void Apply()
        {
            _mutation.Apply(_aggregate);
            _aggregate.Version.Increment();
        }
    }
}