namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAggregateRepository<T>
        where T : IAggregateRoot
    {
        Task<IAggregate<T>> Get(AggregateId id);

        //Task Commit(T root);
    }

    public class AggregateRepository<T> : IAggregateRepository<T>
       where T : IAggregateRoot, new()
    {
        private Dictionary<AggregateId, Aggregate<T>> _aggregates = new Dictionary<AggregateId, Aggregate<T>>();
        protected readonly IAggregateEventStream<T> _stream;
        protected readonly IAggregateDispatcher<T> _dispatcher;

        public AggregateRepository(IAggregateEventStream<T> stream, IAggregateDispatcher<T> dispatcher)
        {
            _stream = stream;
            _dispatcher = dispatcher;
        }

        // public Task Commit(T root)
        // {
        //     throw new System.NotImplementedException();
        // }

        public async Task<IAggregate<T>> Get(AggregateId id)
        {
            if (!_aggregates.TryGetValue(id, out var aggregate))
            {
                var root = new T();
                var version = new AggregateVersion(-1);
                await foreach (var it in _stream.Open(id))
                {
                    it.Apply(root);
                    version.Set(it.Version);
                }

                aggregate = new Aggregate<T>(root, version, _dispatcher, _stream);
                _aggregates.Add(id, aggregate);
            }

            return aggregate;
        }
    }
}