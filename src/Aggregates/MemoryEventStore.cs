namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MemoryEventStore : IEventStore
    {
        private readonly Dictionary<string, List<Event>> _aggregates = new Dictionary<string, List<Event>>();

        public async Task<bool> AppendEvents(string aggregateId, long expectedVersion, IEnumerable<Event> events)
        {
            var eventList = await GetOrCreateEvents(aggregateId);
            lock (eventList)
            {
                if (eventList.Count - 1 != expectedVersion)
                    return false;
                eventList.AddRange(events);
                return true;
            }
        }

        public async IAsyncEnumerable<Event> GetEvents(string aggregateId, long fromVersion = -1)
        {
            var eventList = await GetOrCreateEvents(aggregateId);
            lock (eventList)
            {
                foreach (var it in eventList.Skip((int)fromVersion + 1))
                    yield return it;
            }
        }

        private Task<List<Event>> GetOrCreateEvents(string aggregateId)
        {
            lock (_aggregates)
            {
                if (!_aggregates.TryGetValue(aggregateId, out var eventList))
                {
                    eventList = new List<Event>();
                    _aggregates.Add(aggregateId, eventList);
                }
                return Task.FromResult(eventList);
            }
        }
    }
}