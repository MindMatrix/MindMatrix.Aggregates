using System;

namespace MindMatrix.Aggregates
{
    public interface IAggregateRoot
    {
        AggregateId Id { get; }
        AggregateVersion Version { get; }
        //IAggregateEventList GetEvents();
    }

    public abstract class AggregateRoot : IAggregateRoot
    {
        public AggregateId Id { get; internal set; } = new AggregateId(Guid.Empty);
        public AggregateVersion Version { get; internal set; } = new AggregateVersion();


        // public void Apply(IAggregateMutator<T> mutation)
        // {
        //     mutation.Apply(Root);
        //     _events.Add(mutation);
        //     Version.Increment();
        // }


    }


}