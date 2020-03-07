// namespace MindMatrix.Aggregates
// {
//     using System.Linq;
//     using System.Threading.Tasks;
//     using Shouldly;

//     public class EventListTests
//     {
//         public async Task AppendsUncommitted()
//         {
//             var aggregateId = System.Guid.NewGuid().ToString();
//             var eventStore = new MemoryEventStore();
//             var eventListFactory = new EventListFactory(eventStore);

//             var eventList = await eventListFactory.Create(aggregateId);

//             eventList.Version.ShouldBe(-1);
//             eventList.CommittedVersion.ShouldBe(-1);

//             var ev = eventList.Append("created", "data");
//             ev.Id.ShouldBe(0);
//             eventList.CommittedVersion.ShouldBe(-1);
//             eventList.Version.ShouldBe(0);
//         }

//         public async Task ReadsCommitted()
//         {
//             var aggregateId = System.Guid.NewGuid().ToString();
//             var eventStore = new MemoryEventStore();
//             var events = new[] {
//                 new Event( 0, "Created", $"Guid: {aggregateId}"),
//                 new Event( 1, "Updated", $"Guid: {aggregateId}"),
//                 new Event( 2, "Deleted", $"Guid: {aggregateId}")
//             };

//             await eventStore.AppendEvents(aggregateId, -1, events);

//             var eventListFactory = new EventListFactory(eventStore);

//             var eventList = await eventListFactory.Create(aggregateId, 0);

//             eventList.Version.ShouldBe(2);
//             eventList.CommittedVersion.ShouldBe(2);

//             var ev = eventList.Append("update", "data");
//             ev.Id.ShouldBe(3);
//             eventList.CommittedVersion.ShouldBe(2);
//             eventList.Version.ShouldBe(3);

//             eventList.Commit();

//             eventList.CommittedVersion.ShouldBe(3);
//             eventList.Version.ShouldBe(3);

//         }
//     }
// }