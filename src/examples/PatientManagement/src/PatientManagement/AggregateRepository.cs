namespace PatientManagement
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using MindMatrix.Aggregates;
    using StructureMap;

    public class Aggregate<T> : IAggregate<T>
          where T : IAggregateRoot, new()
    {
        public T Root { get; }
        public IAggregateEventList<T> Events { get; }

        private readonly IContainer _container;

        public Aggregate(IContainer container)
        {
            _container = container;
            Root = new T();
            Events = new AggregateEventList<T>(this);
        }

        public async Task Handle<K>(K request, CancellationToken cancellationToken = default)
        {
            var dispatcher = _container.GetInstance<IAggregateCommand<T, K>>();
            await foreach (var it in dispatcher.Handle(Root, request))
                Events.Add(it);
        }
    }

    public class AggregateEventList<T> : IAggregateEventList<T>
        where T : IAggregateRoot, new()
    {
        private readonly List<IAggregateMutator<T>> _events = new List<IAggregateMutator<T>>();
        private readonly Aggregate<T> _aggregate;

        public AggregateEventList(Aggregate<T> root)
        {
            _aggregate = root;
        }

        public void Add(IAggregateMutator<T> mutation)
        {
            //var ev = new AggregateEvent<T>(_aggregate.Root, mutation);
            //_aggregate.Root.Apply(ev);
            _events.Add(mutation);
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }
    }

    public class AggregateRepository<T> : IAggregateRepository<T>
        where T : IAggregateRoot, new()
    {
        private readonly IEventStoreConnection _eventStore;

        public AggregateRepository(IEventStoreConnection eventStore)
        {
            _eventStore = eventStore;
        }


        public Task Commit(T root)
        {
            //root.
            throw new System.NotImplementedException();
        }

        Task<IAggregate<T>> IAggregateRepository<T>.Get(AggregateId id)
        {
            throw new NotImplementedException();
        }
    }
}