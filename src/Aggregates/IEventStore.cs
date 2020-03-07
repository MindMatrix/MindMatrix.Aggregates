namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        IAsyncEnumerable<Event> GetEvents(string aggregateId, long fromVersion = -1);
        Task<bool> AppendEvents(string aggregateId, long expectedVersion, IEnumerable<Event> events);
    }
}