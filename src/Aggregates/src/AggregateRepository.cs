namespace MindMatrix.Aggregates
{
    using System.Threading.Tasks;

    public interface IAggregateRepository<T>
        where T : IAggregateRoot, new()
    {
        Task<IAggregate<T>> Get(AggregateId id);

        Task Commit(T root);
    }
}