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

    public interface IAggregate<T>
        where T : IAggregateRoot, new()
    {
        T Root { get; }
        void Apply(IAggregateMutator<T> mutation);
    }

    public class Aggregate<T> : IAggregate<T>
        where T : IAggregateRoot, new()
    {
        public T Root { get; }
        private readonly AggregateEventList<T> _events;
        public Aggregate()
        {
            Root = new T();
            _events = new AggregateEventList<T>();
        }

        public void Apply(IAggregateMutator<T> mutation)
        {
            mutation.Apply(Root);
            Root.Version.Increment();
            _events.Add(mutation);
        }
    }
}