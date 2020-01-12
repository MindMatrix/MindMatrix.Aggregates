namespace MindMatrix.Aggregates
{
    public interface IAggregate<T>
          where T : IAggregateRoot, new()
    {
        T Root { get; }
        AggregateEventList<T> Events { get; }
    }

    public class Aggregate<T> : IAggregate<T>
        where T : IAggregateRoot, new()
    {
        public T Root { get; }
        public AggregateEventList<T> Events { get; }
        public Aggregate()
        {
            Root = new T();
            Events = new AggregateEventList<T>(this);
        }
    }
}