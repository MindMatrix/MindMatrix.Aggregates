namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Shouldly;

    public class AggregateTests
    {

        public class License
        {
            public DateTime ExpiresOn { get; internal set; }
        }

        public class LicenseCreated : IMutation<License>
        {
            public DateTime Date { get; set; }

            public void Apply(License aggregate)
            {
                aggregate.ExpiresOn = Date.AddDays(365);
            }
        }

        public void CanCreateAggregate()
        {
            var aggregateId = System.Guid.NewGuid().ToString();
            var state = new License();
            var events = new EventList(aggregateId, -1);
            var serializer = new NewtonsoftMutationSerializer<License>();
            var resolver = new MutationTypeResolver(typeof(License).Assembly);
            var aggregate = new Aggregate<License>(aggregateId, state, events, serializer, resolver);

            var createdOn = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            var expirseOn = createdOn.AddDays(365);

            aggregate.Apply(new LicenseCreated() { Date = createdOn });

            aggregate.Version.ShouldBe(0);
            aggregate.CommittedVersion.ShouldBe(-1);
            aggregate.State.ExpiresOn.ShouldBe(expirseOn);
        }

        public async Task AppliesStateOnLoad()
        {
            var aggregateId = System.Guid.NewGuid().ToString();
            var state = new License();
            var serializer = new NewtonsoftMutationSerializer<License>();
            var resolver = new MutationTypeResolver(typeof(License).Assembly);
            var mutationType = resolver.GetByType(typeof(LicenseCreated));

            var createdOn = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            var expirseOn = createdOn.AddDays(365);
            var licenseEvent = new LicenseCreated() { Date = createdOn };
            var eventStore = new MemoryEventStore();
            await eventStore.AppendEvents(aggregateId, -1, new[] { new Event(0, mutationType.Name, serializer.Serialize(mutationType, licenseEvent)) });
            var eventListFactory = new EventListFactory(eventStore);
            var events = await eventListFactory.Create(aggregateId);

            var aggregate = new Aggregate<License>(aggregateId, state, events, serializer, resolver);

            aggregate.Version.ShouldBe(0);
            aggregate.CommittedVersion.ShouldBe(0);
            aggregate.State.ExpiresOn.ShouldBe(expirseOn);
        }
    }
}