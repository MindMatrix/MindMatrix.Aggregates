// namespace MindMatrix.Aggregates
// {
//     using System.Collections.Generic;

//     public class EventList<Aggregate>
//     {
//         //private readonly IMutationTypeResolver<Aggregate> _resolver;
//         private readonly List<IMutation<Aggregate>> _events = new List<IMutation<Aggregate>>();
//         private readonly List<IMutation<Aggregate>> _uncommittedEvents = new List<IMutation<Aggregate>>();
//         private readonly string _aggregateId;
//         private long _baseVersion;
//         public long CommittedVersion => _baseVersion + _events.Count;
//         public long Version => CommittedVersion + _uncommittedEvents.Count;

//         public EventList(string aggregateId, long baseVersion)
//         {
//             //resolver = resolver;
//             _aggregateId = aggregateId;
//             _baseVersion = baseVersion;
//         }

//         internal void AppendCommitted(IMutation<Aggregate> ev)
//         {
//             _events.Add(ev);
//         }

//         public void Append<Mutation>(Mutation mutation)
//             where Mutation : IMutation<Aggregate>
//         {
//             //var mutationType = _resolver.GetByType(typeof(Mutation));
//             //var ev = new Event<Aggregate>(Version + 1, mutationType.Name, mutation);
//             _uncommittedEvents.Add(mutation);
//             //return ev;
//         }

//         public IReadOnlyList<IMutation<Aggregate>> CommittedEvents => _events;
//         public IReadOnlyList<IMutation<Aggregate>> UncommittedEvents => _uncommittedEvents;

//         public void Commit()
//         {
//             foreach (var it in _uncommittedEvents)
//                 AppendCommitted(it);

//             _uncommittedEvents.Clear();
//         }
//     }
// }