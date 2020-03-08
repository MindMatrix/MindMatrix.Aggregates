// namespace MindMatrix.Aggregates
// {
//     using System.Threading;
//     using System.Threading.Tasks;
//     using Microsoft.EntityFrameworkCore;

//     public class SqlContext : DbContext
//     {
//         public virtual DbSet<Aggregate<T>> Aggregates { get; set; }

//         public AggregateRepository(DbContextOptions options) : base(options)
//         {

//         }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {


//         }

//         public Task<Aggregate<T>> Get(string aggregateId, CancellationToken token = default)
//         {
//             throw new System.NotImplementedException();
//         }

//         public Task<bool> SaveChanges(CancellationToken token = default)
//         {
//             return await SaveChangesAsync(default) > 0;
//         }
//     }

// }