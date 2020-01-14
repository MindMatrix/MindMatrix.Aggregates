namespace MindMatrix.Aggregates
{
    using System.Threading;
    using System.Threading.Tasks;
    public interface IAggregate<T>
          where T : IAggregateRoot
    {
        T Root { get; }
        IAggregateEventList<T> Events { get; }

        Task Handle<K>(K request, CancellationToken cancellationToken);

        Task Commit(CancellationToken cancellationToken = default);
    }

    public class Aggregate<T> : IAggregate<T>
        where T : IAggregateRoot
    {
        public T Root { get; }

        public IAggregateEventList<T> Events => _events;

        private readonly IAggregateDispatcher<T> _dispatcher;
        private readonly AggregateEventList<T> _events;
        private readonly IAggregateEventStream<T> _stream;
        private AggregateVersion _committedVersion;

        public Aggregate(T root, AggregateVersion committedVersion, IAggregateDispatcher<T> dispatcher, IAggregateEventStream<T> stream)
        {
            Root = root;
            _committedVersion = committedVersion;
            _events = new AggregateEventList<T>();
            _dispatcher = dispatcher;
            _stream = stream;

            //read
        }

        public async Task Handle<K>(K request, CancellationToken cancellationToken)
        {
            await foreach (var it in _dispatcher.Dispatch(Root, request, cancellationToken))
                Events.Add(it);
        }

        public async Task Commit(CancellationToken cancellationToken = default)
        {
            _committedVersion = await _stream.Append(Root.Id, _committedVersion, _events, cancellationToken);
            _events.Clear();
            //await _stream.Append(null);
        }
    }
}