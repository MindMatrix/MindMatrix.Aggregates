namespace MindMatrix.Aggregates.Tests
{
    using System;
    using Xunit;
    using Shouldly;
    using MindMatrix.Aggregates;

    public class AggregateRootTests
    {
        public class AggregateCreated : IAggregateMutator<User>
        {
            public AggregateId Id { get; set; }
            public void Apply(User user)
            {
                user.Id = Id;
            }

        }

        public class User : AggregateRoot
        {

        }


        [Fact]
        public void ShouldCreateAggregate()
        {
            var id = AggregateId.GenerateId();
            var aggregate = new Aggregate<User>();
            aggregate.Apply(new AggregateCreated() { Id = id });

            aggregate.Root.Id.ShouldBe(id);
        }
    }
}