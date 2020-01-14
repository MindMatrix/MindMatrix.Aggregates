namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAggregateEventStream<T>
        where T : IAggregateRoot
    {
        IAsyncEnumerable<IAggregateStreamEvent<T>> Open(AggregateId id);
        Task<AggregateVersion> Append(AggregateId id, AggregateVersion expectedVersion, IEnumerable<IAggregateMutator<T>> events, CancellationToken cancellationToken = default);
    }
}