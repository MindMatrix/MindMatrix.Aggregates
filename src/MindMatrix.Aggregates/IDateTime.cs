namespace MindMatrix.Aggregates
{
    using System;

    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}