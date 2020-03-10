namespace MindMatrix.Aggregates
{
    using System;

    public class StaticDateTime : IDateTime
    {
        public DateTime UtcNow => new DateTime(2010, 3, 29, 1, 2, 3, DateTimeKind.Utc);
    }

}