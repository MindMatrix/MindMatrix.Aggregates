namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;

    public class EventList
    {
        private readonly List<Event> _events = new List<Event>();
        private readonly List<Event> _uncommittedEvents = new List<Event>();
        private readonly string _aggregateId;
        private long _baseVersion;
        public long CommittedVersion => _baseVersion + _events.Count;
        public long Version => CommittedVersion + _uncommittedEvents.Count;

        public EventList(string aggregateId, long baseVersion)
        {
            _aggregateId = aggregateId;
            _baseVersion = baseVersion;
        }

        internal void AppendCommitted(Event ev)
        {
            _events.Add(ev);
        }

        public Event Append(string type, string data)
        {
            var ev = new Event(Version + 1, type, data);
            _uncommittedEvents.Add(ev);
            return ev;
        }


        public IReadOnlyList<Event> CommittedEvents => _events;
        public IReadOnlyList<Event> UncommittedEvents => _uncommittedEvents;

        public void Commit()
        {
            foreach (var it in _uncommittedEvents)
                AppendCommitted(it);

            _uncommittedEvents.Clear();
        }
    }
}