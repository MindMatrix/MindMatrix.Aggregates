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


    public class AggregateRepository<T> : IAggregateRepository<T>
        where T : IAggregateRoot
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