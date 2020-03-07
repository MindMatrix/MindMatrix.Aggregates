namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public class EventList<Aggregate>
    {
        private readonly IMutationTypeResolver<Aggregate> _resolver;
        private readonly List<IEvent<Aggregate>> _events = new List<IEvent<Aggregate>>();
        private readonly List<IEvent<Aggregate>> _uncommittedEvents = new List<IEvent<Aggregate>>();
        private readonly string _aggregateId;
        private long _baseVersion;
        public long CommittedVersion => _baseVersion + _events.Count;
        public long Version => CommittedVersion + _uncommittedEvents.Count;

        public EventList(IMutationTypeResolver<Aggregate> resolver, string aggregateId, long baseVersion)
        {
            _resolver = resolver;
            _aggregateId = aggregateId;
            _baseVersion = baseVersion;
        }

        internal void AppendCommitted(IEvent<Aggregate> ev)
        {
            _events.Add(ev);
        }

        public IEvent<Aggregate> Append<Mutation>(Mutation mutation)
            where Mutation : IMutation<Aggregate>
        {
            var mutationType = _resolver.GetByType(typeof(Mutation));
            var ev = new Event<Aggregate>(Version + 1, mutationType.Name, mutation);
            _uncommittedEvents.Add(ev);
            return ev;
        }

        public IReadOnlyList<IEvent<Aggregate>> CommittedEvents => _events;
        public IReadOnlyList<IEvent<Aggregate>> UncommittedEvents => _uncommittedEvents;

        public void Commit()
        {
            foreach (var it in _uncommittedEvents)
                AppendCommitted(it);

            _uncommittedEvents.Clear();
        }
    }
}