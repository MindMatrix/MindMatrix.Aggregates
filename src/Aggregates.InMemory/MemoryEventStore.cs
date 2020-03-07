namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class InMemoryAggregateRepository<T> : IAggregateRepository<T>
        where T : new()
    {
        private readonly Dictionary<string, Aggregate<T>> _aggregates = new Dictionary<string, Aggregate<T>>();

        private Aggregate<T> GetOrCreateAggregate(string aggregateId)
        {
            lock (_aggregates)
            {
                if (!_aggregates.TryGetValue(aggregateId, out var aggregate))
                {
                    aggregate = new Aggregate<T>(aggregateId);
                    _aggregates.Add(aggregateId, aggregate);
                }
                return aggregate;
            }
        }

        public Task<Aggregate<T>> Get(string aggregateId) => Task.FromResult(GetOrCreateAggregate(aggregateId));

        public Task<bool> SaveChanges()
        {
            foreach (var it in _aggregates.Values)
                it.Commit();
            return Task.FromResult(true);
        }
    }

    // public class MemoryEventStore<Aggregate> : IEventStore<Aggregate>
    // {
    //     private readonly Dictionary<string, List<Event<Aggregate>>> _aggregates = new Dictionary<string, List<Event<Aggregate>>>();

    //     public async Task<bool> AppendEvents(string aggregateId, long expectedVersion, IEnumerable<IMutation<Aggregate>> events)
    //     {
    //         var eventList = await GetOrCreateEvents(aggregateId);
    //         lock (eventList)
    //         {
    //             if (eventList.Count - 1 != expectedVersion)
    //                 return false;
    //             eventList.AddRange(events);
    //             return true;
    //         }
    //     }

    //     public async IAsyncEnumerable<IEvent<Aggregate>> GetEvents(string aggregateId, long fromVersion = -1)
    //     {
    //         var eventList = await GetOrCreateEvents(aggregateId);
    //         lock (eventList)
    //         {
    //             foreach (var it in eventList.Skip((int)fromVersion + 1))
    //                 yield return it;
    //         }
    //     }

    //     private Task<List<Event>> GetOrCreateEvents(string aggregateId)
    //     {
    //         lock (_aggregates)
    //         {
    //             if (!_aggregates.TryGetValue(aggregateId, out var eventList))
    //             {
    //                 eventList = new List<Event>();
    //                 _aggregates.Add(aggregateId, eventList);
    //             }
    //             return Task.FromResult(eventList);
    //         }
    //     }
    // }
}