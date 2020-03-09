namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using Shouldly;

    public class MongoAggregateTests
    {

        public class License
        {
            public string TitleId { get; internal set; }
            public DateTime ExpiresOn { get; internal set; }
        }

        public class LicenseCreated : IMutation<License>
        {
            public string TitleId { get; set; }
            public DateTime CreatedOn { get; set; }
            public void Apply(License aggregate)
            {
                aggregate.ExpiresOn = CreatedOn.AddDays(365);
            }
        }

        public async Task CanCreateAggregate()
        {
            var resolver = new MutationTypeResolver<License>();
            await using var mongo = new MongoDbContext<License>(resolver);
            var client = mongo.Client;
            var database = mongo.Database;
            var repository = new MongoAggregateRepository<License>(client, database, resolver);

            var now = DateTime.UtcNow;
            var aggregate = repository.Create();
            aggregate.Apply(new LicenseCreated() { CreatedOn = now });


            aggregate.State.ExpiresOn.ShouldBe(now.AddDays(365));
            aggregate.Version.ShouldBe(0);
            aggregate.Exists.ShouldBe(false);
            aggregate.CommittedVersion.ShouldBe(-1);

            var result = await repository.SaveChanges();
            result.ShouldBe(true);

            aggregate.Version.ShouldBe(0);
            aggregate.CommittedVersion.ShouldBe(0);
            aggregate.Exists.ShouldBe(true);
        }

    }
}