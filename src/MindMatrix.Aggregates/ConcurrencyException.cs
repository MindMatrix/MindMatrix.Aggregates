namespace MindMatrix.Aggregates
{
    using System;

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string aggregateId, long aggregateVersion)
            : base($"Failed to update aggregate: '{aggregateId}' with version: {aggregateVersion}")
        {

        }
    }
}