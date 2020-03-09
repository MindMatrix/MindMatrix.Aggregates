using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shouldly;

namespace MindMatrix.Aggregates.Tests
{
    public class AggregateEventStreamTests
    {

        public class License
        {
            public string TitleId { get; set; }
            public DateTime ExpiresOn { get; set; }
        }

        public class LicenseCreated : IMutation<License>
        {
            public DateTime CreatedOn { get; set; }
            public void Apply(ref License aggregate)
            {
                aggregate.ExpiresOn = CreatedOn.AddDays(365);
            }
        }

        public async Task Test()
        {
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            var client = new MongoClient(connectionSettings);
            var database = client.GetDatabase("aggregates");
            var factory = new AggregateCollectionFactory(database);
            var repository = new AggregateRepository<License>(factory);

            var aggregateId = Guid.NewGuid().ToString();
            var aggregate = await repository.GetLatest(aggregateId);
            var createdOn = new DateTime(2020, 3, 9, 20, 19, 42, DateTimeKind.Utc);

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });

            await aggregate.Commit();

            var collection = database.GetCollection<Aggregate<License>>("License");
            var query = await collection.FindAsync(x => x.AggregateId == aggregateId && x.AggregateVersion == 0);
            var record = await query.FirstOrDefaultAsync();

            record.ShouldNotBeNull();
            record.MutationVersion.ShouldBe(0);
            record.MutationCommits.Count.ShouldBe(1);
            record.MutationCommits[0].Mutations.Count.ShouldBe(1);
            record.MutationCommits[0].Mutations[0].EventId.ShouldBe(0);
            record.MutationCommits[0].Mutations[0].Mutation.ShouldBeOfType<LicenseCreated>().CreatedOn.ShouldBe(createdOn);
        }

    }
}