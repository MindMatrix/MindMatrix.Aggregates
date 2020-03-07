namespace MindMatrix.Aggregates
{
    public interface IEvent<T>
    {
        long Id { get; }
        string Type { get; }

        IMutation<T> Mutation { get; }
    }

    public class Event<T> : IEvent<T>
    {
        public long Id { get; }
        public string Type { get; }
        public IMutation<T> Mutation { get; }

        public Event(long id, string mapper, IMutation<T> mutation)
        {
            Id = id;
            Type = mapper;
            Mutation = mutation;
        }
    }
}