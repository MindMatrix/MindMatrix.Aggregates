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

        public async Task CanCreateAggregate()
        {
            var aggregateId = System.Guid.NewGuid().ToString();
            var aggregates = new InMemoryAggregateRepository<License>();
            var aggregate = await aggregates.Get(aggregateId);

            var createdOn = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            var expirseOn = createdOn.AddDays(365);

            aggregate.Apply(new LicenseCreated() { Date = createdOn });

            aggregate.Version.ShouldBe(0);
            aggregate.CommittedVersion.ShouldBe(-1);
            aggregate.State.ExpiresOn.ShouldBe(expirseOn);
        }

        // public async Task AppliesStateOnLoad()
        // {
        //     var aggregateId = System.Guid.NewGuid().ToString();
        //     var state = new License();
        //     var createdOn = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
        //     var expirseOn = createdOn.AddDays(365);
        //     var licenseEvent = new LicenseCreated() { Date = createdOn };
        //     var eventStore = new InMemoryAggregateRepository<License>();

        //     var aggregate = eventStore.Get(aggregateId);

        //     aggregate.Version.ShouldBe(0);
        //     aggregate.CommittedVersion.ShouldBe(0);
        //     aggregate.State.ExpiresOn.ShouldBe(expirseOn);
        // }
    }
}