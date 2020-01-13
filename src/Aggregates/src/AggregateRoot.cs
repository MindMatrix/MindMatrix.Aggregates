using System;

namespace MindMatrix.Aggregates
{
    public interface IAggregateRoot
    {
        AggregateId Id { get; }
        AggregateVersion Version { get; }
        //IAggregateEventList GetEvents();
        void Apply(IAggregateEvent ev);
    }

    public abstract class AggregateRoot : IAggregateRoot
    {
        public AggregateId Id { get; internal set; } = new AggregateId(Guid.Empty);
        public AggregateVersion Version { get; internal set; } = new AggregateVersion(-1);

        public void Apply(IAggregateEvent ev)
        {
            ev.Apply();
            Version.Increment();
        }
    }


}