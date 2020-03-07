namespace MindMatrix.Aggregates
{
    public class Event
    {
        public long Id { get; }
        public string Type { get; }
        public string Data { get; }

        public Event(long id, string mapper, string data)
        {
            Id = id;
            Type = mapper;
            Data = data;
        }
    }
}