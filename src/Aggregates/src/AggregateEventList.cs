namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public interface IAggregateEventList : IEnumerable<IAggregateMutator>
    {
        //void Add(IAggregateMutator mutation);
    }

    public class AggregateEventList<T>
        where T : IAggregateRoot
    {
        private readonly List<IAggregateMutator> _events = new List<IAggregateMutator>();

        public void Add(IAggregateMutator<T> mutation)
        {
            _events.Add(mutation);
        }
    }
}