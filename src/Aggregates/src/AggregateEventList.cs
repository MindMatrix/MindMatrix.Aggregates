namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public interface IAggregateEventList<T>
        where T : IAggregateRoot, new()
    {
        void Add(IAggregateMutator<T> mutation);
        void Commit();
    }
}