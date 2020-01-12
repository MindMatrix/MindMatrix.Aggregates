namespace MindMatrix.Aggregates
{
    public interface IAggregateEvent
    {
        AggregateEventId Id { get; }
        AggregateEventSequence Sequence { get; }
    }

    public class AggregateEvent : IAggregateEvent
    {
        public AggregateEventId Id { get; }

        public AggregateEventSequence Sequence { get; }

        public AggregateEvent(AggregateEventId id, AggregateEventSequence sequence)
        {
            Id = id;
            Sequence = sequence;
        }
    }
}