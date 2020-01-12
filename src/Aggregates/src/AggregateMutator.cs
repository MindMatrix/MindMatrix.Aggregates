namespace MindMatrix.Aggregates
{
    public interface IAggregateMutator
    {
        // object GetEvent();
        // void Apply();
    }
    public interface IAggregateMutator<T> : IAggregateMutator
        where T : IAggregateRoot
    {
        void Apply(T aggregate);
    }

    // public class AggregateMutator<T> : IAggregateMutator
    // {
    //     private readonly T Aggregate;
    //     private readonly IAggregateMutator<T> Mutation;

    //     public object GetEvent() => Mutation;

    //     public void Apply() => Mutation.Apply(Aggregate);
    // }
}