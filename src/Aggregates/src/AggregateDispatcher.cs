namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Threading;

    public interface IAggregateDispatcher<T>
        where T : IAggregateRoot
    {
        IAsyncEnumerable<IAggregateMutator<T>> Dispatch<K>(T aggregate, K request, CancellationToken cancellationToken = default);
    }
}