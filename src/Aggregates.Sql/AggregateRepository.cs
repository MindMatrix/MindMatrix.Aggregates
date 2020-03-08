namespace MindMatrix.Aggregates
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class AggregateRepository<T> : DbContext, IAggregateRepository<T>
        where T : new()
    {
        public virtual DbSet<Aggregate<T>> Aggregates { get; set; }

        public AggregateRepository(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aggregate<T>>(entity =>
            {
                entity.HasKey(x => x.AggregateId);

                entity
                    .Property(x => x.AggregateId)
                    .HasMaxLength(64)
                    .IsRequired();

                entity
                    .Property(x => x.CommittedVersion)
                    .HasField("_baseVersion")
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                    .IsConcurrencyToken();

                entity
                    .Property(x => x.State)
                    .HasField("_state")
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                    .HasConversion(
                        to => JsonConvert.SerializeObject(to),
                        to => JsonConvert.DeserializeObject<T>(to)
                    );
            });
        }

        public Task<Aggregate<T>> Get(string aggregateId, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> SaveChanges(CancellationToken token = default)
        {
            return await SaveChangesAsync(default) > 0;
        }
    }

}