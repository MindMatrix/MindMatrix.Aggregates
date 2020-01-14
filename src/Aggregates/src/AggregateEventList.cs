namespace MindMatrix.Aggregates
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IAggregateEventList<T> : IEnumerable<IAggregateMutator<T>>
        where T : IAggregateRoot
    {
        void Add(IAggregateMutator<T> mutation);
        //IEnumerable<IAggregateMutator<T>> Commit();
    }

    public class AggregateEventList<T> : IAggregateEventList<T>
           where T : IAggregateRoot
    {
        protected readonly List<IAggregateMutator<T>> _events = new List<IAggregateMutator<T>>();
        public void Add(IAggregateMutator<T> mutation)
        {
            _events.Add(mutation);
        }
        public void Clear()
        {
            _events.Clear();
        }

        public IEnumerator<IAggregateMutator<T>> GetEnumerator()
        {
            foreach (var it in _events)
                yield return it;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var it in _events)
                yield return it;
        }
    }
}