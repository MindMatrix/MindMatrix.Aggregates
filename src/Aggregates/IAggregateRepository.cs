namespace MindMatrix.Aggregates
{
    using System.Threading.Tasks;

    public interface IAggregateRepository<Aggregate>
        where Aggregate : new()
    {
        Task<Aggregate<Aggregate>> Get(string aggregateId);
        //void Save(Aggregate aggregate);
        //void Delete(string aggregateId);

        Task<bool> SaveChanges();

    }

}