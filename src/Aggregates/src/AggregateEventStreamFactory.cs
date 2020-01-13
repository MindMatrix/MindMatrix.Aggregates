namespace MindMatrix.Aggregates
{
    public interface IAggregateEventStreamFactory<T>
        where T : IAggregateRoot
    {
        IAggregateEventStream<T> Open(AggregateId id);

    }

    public interface IAggregateEventStream<T>
        where T : IAggregateRoot
    {
        //Task 
    }
}