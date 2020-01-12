namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public interface IAggregateEventList<T> : IEnumerable<IAggregateMutator>
        where T : IAggregateRoot, new()
    {
        void Apply(IAggregateMutator<T> mutation);
    }

    public class AggregateEventList<T>
        where T : IAggregateRoot, new()
    {
        private readonly List<IAggregateMutator> _events = new List<IAggregateMutator>();
        private readonly Aggregate<T> _aggregate;

        public AggregateEventList(Aggregate<T> root)
        {
            _aggregate = root;
        }

        public void Apply(IAggregateMutator<T> mutation)
        {
            mutation.Apply(_aggregate.Root);
            _aggregate.Root.Version.Increment();
            _events.Add(mutation);
        }
    }
}