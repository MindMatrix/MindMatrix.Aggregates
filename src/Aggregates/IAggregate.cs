namespace MindMatrix.Aggregates
{
    public interface IAggregate<T>
    {
        string AggregateId { get; }
        T State { get; }
        long Version { get; }
        long CommittedVersion { get; }

        void Apply<Mutation>(Mutation mutation)
                    where Mutation : IMutation<T>;
    }
}