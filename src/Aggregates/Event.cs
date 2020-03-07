namespace MindMatrix.Aggregates
{
    public class Event
    {
        public long Id { get; }
        public string Mapper { get; }
        public string Data { get; }

        public Event(long id, string mapper, string data)
        {
            Id = id;
            Mapper = mapper;
            Data = data;
        }
    }
}