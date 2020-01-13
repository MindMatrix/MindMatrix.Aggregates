namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public interface IAggregateCommand<T, K>
        where T : IAggregateRoot
    {
        IAsyncEnumerable<IAggregateMutator<T>> Handle(T aggregate, K request, CancellationToken cancellationToken = default);
    }

    public abstract class AggregateCommand<T, K> : IAggregateCommand<T, K>
         where T : IAggregateRoot
    {
        public async IAsyncEnumerable<IAggregateMutator<T>> Handle(T aggregate, K request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var it in OnHandle(aggregate, request, cancellationToken))
            {
                aggregate.Apply(new AggregateEvent<T>(aggregate, it));
                it.Apply(aggregate);
                yield return it;
            }
        }

        protected abstract IAsyncEnumerable<IAggregateMutator<T>> OnHandle(T aggregate, K request, CancellationToken cancellationToken);
    }


}