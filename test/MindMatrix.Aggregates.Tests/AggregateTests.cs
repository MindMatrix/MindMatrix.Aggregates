using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shouldly;

namespace MindMatrix.Aggregates
{

    public class Counter
    {
        public int Count { get; set; }

        public override string ToString() => $"{{ Count: {Count} }}";
    }

    public class Increment : IMutation<Counter>
    {
        public int Amount { get; set; }
        public void Apply(Counter aggregate)
        {
            aggregate.Count += Amount;
        }
    }

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


    public class AggregateEventStreamTests
    {

        public async Task CreatesNewAggregate()
        {
            await using var context = new MongoDbContext<License>();

            var aggregateId = "CreatesNewAggregate";
            var aggregate = await context.Repository.GetLatest(aggregateId);
            var createdOn = context.DateTime.UtcNow;

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });

            await aggregate.Commit();

            var collection = context.Database.GetCollection<Aggregate<License>>("License");
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
            await using var context = new MongoDbContext<License>();

            var aggregateId = "MutatesExistingAggregate";
            var aggregate = await context.Repository.GetLatest(aggregateId);
            var createdOn = context.DateTime.UtcNow;

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));

            await aggregate.Commit();

            aggregate = await context.Repository.GetLatest(aggregateId);
            aggregate.Apply(new LicenseNoop());
            aggregate.Apply(new LicenseReleased());
            aggregate.State.Released.ShouldBe(true);
            await aggregate.Commit();

            var collection = context.Database.GetCollection<Aggregate<License>>("License");
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
            var settings = new AggregateSettings() { MaxMutationCommits = 1 };
            await using var context = new MongoDbContext<License>(settings);

            var aggregateId = "ShouldSplit";
            var aggregate = await context.Repository.GetLatest(aggregateId);
            var now = context.DateTime.UtcNow;
            var createdOn = now;

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));

            await aggregate.Commit();

            aggregate = await context.Repository.GetLatest(aggregateId);
            aggregate.Apply(new LicenseNoop());
            aggregate.Apply(new LicenseReleased());
            aggregate.State.Released.ShouldBe(true);
            await aggregate.Commit();

            var collection = context.Database.GetCollection<Aggregate<License>>("License");
            var query = await collection.FindAsync(
                x => x.AggregateId == aggregateId,
                new FindOptions<Aggregate<License>>()
                {
                    Sort = Builders<Aggregate<License>>.Sort.Descending(x => x.AggregateVersion)
                }
            );
            var record = await query.ToListAsync();

            record[1].ShouldNotBeNull();
            record[1].AggregateVersion.ShouldBe(0);
            record[1].TimeToLive.ShouldBe(now);

            record[0].ShouldNotBeNull();
            record[0].TimeToLive.ShouldBeNull();
            record[0].AggregateVersion.ShouldBe(1);
            record[0].MutationVersion.ShouldBe(2);
            record[0].MutationCommits.Count.ShouldBe(1);
            record[0].MutationCommits[0].MutationEvents.Count.ShouldBe(2);
            record[0].MutationCommits[0].MutationEvents[0].EventId.ShouldBe(1);
            record[0].MutationCommits[0].MutationEvents[0].Mutation.ShouldBeOfType<LicenseNoop>();
            record[0].MutationCommits[0].MutationEvents[1].EventId.ShouldBe(2);
            record[0].MutationCommits[0].MutationEvents[1].Mutation.ShouldBeOfType<LicenseReleased>();
        }

        public async Task ShouldReplayOnLoad()
        {
            await using var context = new MongoDbContext<License>();

            var aggregateId = "ShouldReplayOnLoad";
            var aggregate = await context.Repository.GetLatest(aggregateId);
            var createdOn = context.DateTime.UtcNow;

            aggregate.Apply(new LicenseNoop());
            await aggregate.Commit();

            aggregate.Apply(new LicenseCreated() { CreatedOn = createdOn });
            await aggregate.Commit();

            aggregate = await context.Repository.GetLatest(aggregateId);
            aggregate.State.ExpiresOn.ShouldBe(createdOn.AddDays(365));
        }

        public async Task Threaded()
        {
            var settings = new AggregateSettings() { MaxMutationCommits = 10 };
            await using var context = new MongoDbContext<Counter>(settings);
            var aggregateIds = Enumerable.Range(0, 10).Select(x => Guid.NewGuid().ToString()).ToArray();
            var totals = new int[aggregateIds.Length];

            var tasks = Enumerable.Range(0, 16).Select(xx => Task.Run(async () =>
            {
                var r = new Random((xx * 1024 + 1024));
                var values = new int[aggregateIds.Length];

                for (var i = 0; i < 200; i++)
                {
                    var idx = r.Next(0, aggregateIds.Length);
                    var c = r.Next(1, 5);
                    var increments = new int[c];
                    for (var p = 0; p < increments.Length; p++)
                    {
                        increments[p] = r.Next(2, 6);
                        values[idx] += increments[p];
                    }

                    while (true)
                    {
                        var aggregate = await context.Repository.GetLatest(aggregateIds[idx]);
                        var count = aggregate.State.Count;
                        for (var p = 0; p < increments.Length; p++)
                        {
                            count += increments[p];
                            aggregate.Apply(new Increment() { Amount = increments[p] });
                            aggregate.State.Count.ShouldBe(count);
                        }
                        var result = await aggregate.Commit();
                        if (result.Status != CommitStatus.Concurrency)
                            break;
                        await Task.Yield();
                    }
                }

                for (var i = 0; i < aggregateIds.Length; i++)
                    Interlocked.Add(ref totals[i], values[i]);
            })).ToArray();

            await Task.WhenAll(tasks);

            for (var i = 0; i < aggregateIds.Length; i++)
            {
                var aggregate = await context.Repository.GetLatest(aggregateIds[i]);
                aggregate.State.Count.ShouldBe(totals[i]);
            }
        }

        //splitting 0 to 1 is copying the state of the item that caused a split
    }
}