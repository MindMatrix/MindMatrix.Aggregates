namespace MindMatrix.Aggregates
{
    using System.Threading.Tasks;

    public interface IAggregateRepository<Aggregate>
        where Aggregate : new()
    {
        //Task Delete(string aggregateId);
        Task<IAggregate<Aggregate>> GetLatest(string aggregateId);
    }
}