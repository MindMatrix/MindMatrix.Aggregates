// namespace MindMatrix.Aggregates
// {
//     using System.Threading.Tasks;

//     public interface IEventListFactory<Aggregate>
//     {
//         Task<EventList<Aggregate>> Create(string aggregateId, long version = -1);
//     }

//     public class EventListFactory<Aggregate> : IEventListFactory<Aggregate>
//     {
//         private readonly IMutationTypeResolver<Aggregate> _resolver;
//         private readonly IEventStore<Aggregate> _eventStore;

//         public EventListFactory(IMutationTypeResolver<Aggregate> resolver, IEventStore<Aggregate> eventStore)
//         {
//             _eventStore = eventStore;
//         }

//         public async Task<EventList<Aggregate>> Create(string aggregateId, long version = -1)
//         {
//             var events = await _eventStore.GetEvents(aggregateId, version).ToListAsync();
//             var eventList = new EventList<Aggregate>(aggregateId, version);
//             foreach (var it in events)
//                 eventList.AppendCommitted(it);

//             return eventList;
//         }
//     }
// }