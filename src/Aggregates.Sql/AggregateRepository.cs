namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class SqlAggregate<T> : Aggregate<T>
        where T : new()
    {
        public SqlAggregate(string aggregateId) : base(aggregateId)
        {
        }

        public SqlAggregate(string aggregateId, T state, long version) : base(aggregateId, state, version)
        {
        }

        internal void Commit() => commit();
    }

    public class SqlEvent
    {
        public long Id { get; set; }
        public string AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }

    public class AggregateRepository<T> : DbContext, IAggregateRepository<T>
        where T : new()
    {
        public virtual DbSet<SqlAggregate<T>> Aggregates { get; set; }
        public virtual DbSet<SqlEvent> Events { get; set; }

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
                    .IsRequired()
                    .IsConcurrencyToken();


                entity
                    .Property(x => x.State)
                    .HasField("_state")
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                    .IsRequired()
                    .HasConversion(
                        to => JsonConvert.SerializeObject(to),
                        to => JsonConvert.DeserializeObject<T>(to)
                    );
            });

            modelBuilder.Entity<SqlEvent>(entity =>
            {
                entity
                    .Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                entity
                    .Property(e => e.AggregateId)
                    .IsRequired()
                    .HasMaxLength(64);

                entity
                    .Property(e => e.AggregateVersion)
                    .IsRequired();

                entity
                    .Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(100);



            });
        }

        public Aggregate<T> Create()
        {
            var agg = new SqlAggregate<T>(System.Guid.NewGuid().ToString());
            Aggregates.Add(agg);
            return agg;
        }

        public async Task<Aggregate<T>> Get(string aggregateId, CancellationToken token = default)
        {
            var aggregate = await Aggregates.Where(x => x.AggregateId == aggregateId).FirstOrDefaultAsync();
            if (aggregate == null)
            {
                aggregate = new SqlAggregate<T>(aggregateId);
                Aggregates.Add(aggregate);
            }

            return aggregate;
        }

        public async Task<bool> SaveChanges(CancellationToken token = default)
        {
            var aggregates = new List<SqlAggregate<T>>();
            foreach (var it in ChangeTracker.Entries())
                if (it.Entity is SqlAggregate<T> aggregate && aggregate.HasChanges)
                    aggregates.Add(aggregate);

            foreach (var it in aggregates)
            {
                var eventId = it.CommittedVersion;
                foreach (var ev in it.UncommittedEvents)
                {
                    Events.Add(new SqlEvent()
                    {
                        AggregateVersion = eventId++,
                        Type = "",
                        Data = ""
                    });
                }
            }

            return await SaveChangesAsync(default) > 0;
        }
    }

}