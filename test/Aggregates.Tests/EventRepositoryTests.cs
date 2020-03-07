namespace MindMatrix.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;

    public class MemoryEventRepositoryTests
    {
        public async Task CanWriteAndRead()
        {
            var eventStore = new MemoryEventStore();
            var aggregateId = Guid.NewGuid().ToString();
            var events = new[] {
                new Event( 0, "Created", $"Guid: {aggregateId}"),
                new Event( 1, "Deleted", $"Guid: {aggregateId}")
            };

            var result = await eventStore.AppendEvents(aggregateId, -1, events);
            var eventResults = await eventStore.GetEvents(aggregateId).ToListAsync();

            result.ShouldBe(true);
            eventResults.Count.ShouldBe(2);
        }

        public async Task ShouldGetAfterVersion()
        {
            var eventStore = new MemoryEventStore();
            var aggregateId = Guid.NewGuid().ToString();
            var events = new[] {
                new Event( 0, "Created", $"Guid: {aggregateId}"),
                new Event( 1, "Updated", $"Guid: {aggregateId}"),
                new Event( 2, "Deleted", $"Guid: {aggregateId}"),
            };

            var result = await eventStore.AppendEvents(aggregateId, -1, events);
            var eventResults = await eventStore.GetEvents(aggregateId, 0).ToListAsync();

            result.ShouldBe(true);
            eventResults.Count.ShouldBe(2);
        }

        public async Task ShouldReturnFalseOnBadVersion()
        {
            var eventStore = new MemoryEventStore();
            var aggregateId = Guid.NewGuid().ToString();
            var events = new[] {
                new Event( 0, "Created", $"Guid: {aggregateId}"),
                new Event( 1, "Deleted", $"Guid: {aggregateId}")
            };

            await eventStore.AppendEvents(aggregateId, -1, events);
            (await eventStore.AppendEvents(aggregateId, 0, new[] { new Event(0, "Created", $"Guid: {aggregateId}") })).ShouldBe(false);
            (await eventStore.AppendEvents(aggregateId, 2, new[] { new Event(1, "Created", $"Guid: {aggregateId}") })).ShouldBe(false);
            (await eventStore.AppendEvents(aggregateId, 3, new[] { new Event(3, "Created", $"Guid: {aggregateId}") })).ShouldBe(false);
            (await eventStore.AppendEvents(aggregateId, 1, new[] { new Event(2, "Created", $"Guid: {aggregateId}") })).ShouldBe(true);

            (await eventStore.GetEvents(aggregateId).ToListAsync()).Count.ShouldBe(3);
        }
    }
}