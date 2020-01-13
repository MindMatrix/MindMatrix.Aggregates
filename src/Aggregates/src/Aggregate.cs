namespace MindMatrix.Aggregates
{
    using System.Threading;
    using System.Threading.Tasks;
    public interface IAggregate<T>
          where T : IAggregateRoot, new()
    {
        T Root { get; }
        IAggregateEventList<T> Events { get; }

        Task Handle<K>(K request, CancellationToken cancellationToken);
    }
}