namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAggregate<AggregateState>
        where AggregateState : new()
    {
        string AggregateId { get; }
        long AggregateVersion { get; }
        long MutationVersion { get; }
        bool Exists { get; }
        bool Mutated { get; }
        DateTime LastMutation { get; }
        AggregateState State { get; }
        void Apply<Mutation>(Mutation mutation) where Mutation : IMutation<AggregateState>;
        //Task<CommitStatusResult> Commit(CancellationToken token = default);
        Task Commit(bool snapshot = false, CancellationToken token = default);
    }
}