namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Shouldly;

    public class SqlAggregateRepositoryTests
    {
        public class License
        {
            public string TitleId { get; set; }
            public DateTime ExpiresOn { get; set; }
        }

        public async Task Test()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var dbOptionsBuilder = new DbContextOptionsBuilder<AggregateRepository<License>>();
            dbOptionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            using (var repository = new AggregateRepository<License>(dbOptionsBuilder.Options))
            {
                var aggregate = await repository.Get(aggregateId);
                aggregate.AggregateId.ShouldBe(aggregateId);
                aggregate.Version.ShouldBe(-1);
            }
        }
    }
}