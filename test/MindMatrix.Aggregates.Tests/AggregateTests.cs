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
            public string TitleId { get; internal set; }
            public DateTime ExpiresOn { get; internal set; }
            public bool Released { get; internal set; }
        }

        public class LicenseCreated : IMutation<License>
        {
            public DateTime CreatedOn { get; set; }
            public void Apply(License aggregate)
            {
                aggregate.ExpiresOn = CreatedOn.AddDays(365);
                aggregate.Released = false;
            }
        }

        public class LicenseReleased : IMutation<License>
        {
            public void Apply(License aggregate)
            {
                aggregate.Released = true;
            }
        }

        public class LicenseNoop : IMutation<License>
        {
            public void Apply(License aggregate)
            {
            }
        }

        public async Task CreatesNewAggregate()
        {
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            var client = new MongoClient(connectionSettings);
            var database = client.GetDatabase("aggregates");
            var factory = new AggregateCollectionFactory(database);
            var repository = new AggregateRepository<License>(factory);

            var aggregateId = "CreatesNewAggregate";
            var aggregate = await repository.GetLatest(aggregateId);
            var createdOn = new DateTime(2020, 3, 9, 20, 19, 42, DateTimeKind.Utc);

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });

            await aggregate.Commit();

            var collection = database.GetCollection<Aggregate<License>>("License");
            var query = await collection.FindAsync(x => x.AggregateId == aggregateId && x.AggregateVersion == 0);
            var record = await query.FirstOrDefaultAsync();

            record.ShouldNotBeNull();
            record.AggregateVersion.ShouldBe(0);
            record.State.ExpiresOn.ShouldBe(new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            record.MutationVersion.ShouldBe(0);
            record.MutationCommits.Count.ShouldBe(1);
            record.MutationCommits[0].MutationEvents.Count.ShouldBe(1);
            record.MutationCommits[0].MutationEvents[0].EventId.ShouldBe(0);
            record.MutationCommits[0].MutationEvents[0].Mutation.ShouldBeOfType<LicenseCreated>().CreatedOn.ShouldBe(createdOn);
        }

        public async Task MutatesExistingAggregate()
        {
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            var client = new MongoClient(connectionSettings);
            var database = client.GetDatabase("aggregates");
            var factory = new AggregateCollectionFactory(database);
            var repository = new AggregateRepository<License>(factory);

            var aggregateId = "MutatesExistingAggregate";
            var aggregate = await repository.GetLatest(aggregateId);
            var createdOn = new DateTime(2020, 3, 9, 20, 19, 42, DateTimeKind.Utc);

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));

            await aggregate.Commit();

            aggregate = await repository.GetLatest(aggregateId);
            aggregate.Apply(new LicenseNoop());
            aggregate.Apply(new LicenseReleased());
            aggregate.State.Released.ShouldBe(true);
            await aggregate.Commit();

            var collection = database.GetCollection<Aggregate<License>>("License");
            var query = await collection.FindAsync(x => x.AggregateId == aggregateId && x.AggregateVersion == 0);
            var record = await query.FirstOrDefaultAsync();

            record.ShouldNotBeNull();
            record.AggregateVersion.ShouldBe(0);
            record.MutationVersion.ShouldBe(2);
            record.MutationCommits.Count.ShouldBe(2);
            record.MutationCommits[0].MutationEvents.Count.ShouldBe(1);
            record.MutationCommits[0].MutationEvents[0].EventId.ShouldBe(0);
            record.MutationCommits[0].MutationEvents[0].Mutation.ShouldBeOfType<LicenseCreated>().CreatedOn.ShouldBe(createdOn);
            record.MutationCommits[1].MutationEvents.Count.ShouldBe(2);
            record.MutationCommits[1].MutationEvents[0].EventId.ShouldBe(1);
            record.MutationCommits[1].MutationEvents[0].Mutation.ShouldBeOfType<LicenseNoop>();
            record.MutationCommits[1].MutationEvents[1].EventId.ShouldBe(2);
            record.MutationCommits[1].MutationEvents[1].Mutation.ShouldBeOfType<LicenseReleased>();
        }

        public async Task ShouldSplit()
        {
            var aggregateSettings = new AggregateSettings() { MaxMutationCommits = 1 };
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            var client = new MongoClient(connectionSettings);
            var database = client.GetDatabase("aggregates");
            var factory = new AggregateCollectionFactory(database);
            var repository = new AggregateRepository<License>(factory, aggregateSettings);

            var aggregateId = "ShouldSplit";
            var aggregate = await repository.GetLatest(aggregateId);
            var createdOn = new DateTime(2020, 3, 9, 20, 19, 42, DateTimeKind.Utc);

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));

            await aggregate.Commit();

            aggregate = await repository.GetLatest(aggregateId);
            aggregate.Apply(new LicenseNoop());
            aggregate.Apply(new LicenseReleased());
            aggregate.State.Released.ShouldBe(true);
            await aggregate.Commit();

            var collection = database.GetCollection<Aggregate<License>>("License");
            var query = await collection.FindAsync(
                x => x.AggregateId == aggregateId,
                new FindOptions<Aggregate<License>>()
                {
                    Sort = Builders<Aggregate<License>>.Sort.Descending(x => x.AggregateVersion),
                    Limit = 1
                }

            );
            var record = await query.FirstOrDefaultAsync();

            record.ShouldNotBeNull();
            record.AggregateVersion.ShouldBe(1);
            record.MutationVersion.ShouldBe(2);
            record.MutationCommits.Count.ShouldBe(1);
            record.MutationCommits[0].MutationEvents.Count.ShouldBe(2);
            record.MutationCommits[0].MutationEvents[0].EventId.ShouldBe(1);
            record.MutationCommits[0].MutationEvents[0].Mutation.ShouldBeOfType<LicenseNoop>();
            record.MutationCommits[0].MutationEvents[1].EventId.ShouldBe(2);
            record.MutationCommits[0].MutationEvents[1].Mutation.ShouldBeOfType<LicenseReleased>();
        }

        public async Task ShouldReplayOnLoad()
        {
            var connectionSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            var client = new MongoClient(connectionSettings);
            var database = client.GetDatabase("aggregates");
            var factory = new AggregateCollectionFactory(database);
            var repository = new AggregateRepository<License>(factory);

            var aggregateId = "ShouldReplayOnLoad";
            var aggregate = await repository.GetLatest(aggregateId);
            var createdOn = new DateTime(2020, 3, 9, 20, 19, 42, DateTimeKind.Utc);

            aggregate.Apply(new LicenseNoop());
            await aggregate.Commit();

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            await aggregate.Commit();

            aggregate = await repository.GetLatest(aggregateId);
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));
        }
    }
}