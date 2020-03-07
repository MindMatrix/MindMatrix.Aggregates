namespace MindMatrix.Aggregates
{
    using System.Threading.Tasks;

    public interface IEventListFactory
    {
        Task<EventList> Create(string aggregateId, long version = -1);
    }

    public class EventListFactory : IEventListFactory
    {
        private readonly IEventStore _eventStore;

        public EventListFactory(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<EventList> Create(string aggregateId, long version = -1)
        {
            var events = await _eventStore.GetEvents(aggregateId, version).ToListAsync();
            var eventList = new EventList(aggregateId, version);
            foreach (var it in events)
                eventList.AppendCommitted(it);

            return eventList;
        }
    }
}