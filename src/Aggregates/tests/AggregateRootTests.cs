namespace MindMatrix.Aggregates.Tests
{
    using System;
    using Xunit;
    using Shouldly;
    using MindMatrix.Aggregates;
    using System.Threading.Tasks;
    using System.Threading;
    using MediatR;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

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

        // public class CreateUser : IAggregateCommand<User, AggregateCreated>
        // {
        //     public Task<AggregateCreated> Handle(Aggregate<User> aggregate, CancellationToken cancellationToken = default)
        //     {
        //         throw new NotImplementedException();
        //     }
        // }

        // public class CreateUser : IRequest<Aggregate<User>>
        // {
        //     public string Name { get; }

        // }

        // public class CreateUserHandler : IRequestHandler<CreateUser, Aggregate<User>>
        // {
        //     public Task<Aggregate<User>> Handle(CreateUser request, CancellationToken cancellationToken)
        //     {
        //         return Task.FromResult<Aggregate<User>>(null);
        //     }
        // }

        public class UpdateUser //: IRequest
        {
            public string Name { get; set; }
        }

        public class UpdatedUser : IAggregateMutator<User>
        {
            public AggregateId Id { get; }

            public UpdateUser UpdateUser { get; }

            public UpdatedUser(AggregateId id, UpdateUser update)
            {
                Id = id;
                UpdateUser = update;
            }

            public void Apply(User aggregate)
            {
                aggregate.Name = UpdateUser.Name;
            }
        }

        public class UpdateUserHandler : AggregateCommand<User, UpdateUser>
        {
            protected override async IAsyncEnumerable<IAggregateMutator<User>> OnHandle(User aggregate, UpdateUser command, [EnumeratorCancellation]CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                yield return new UpdatedUser(aggregate.Id, command);
            }
        }


        public class User : AggregateRoot
        {
            public string Name { get; set; }

            public User(AggregateId id)
            {
                Id = id;
            }
        }


        [Fact]
        public async void ShouldCreateAggregate()
        {
            var id = AggregateId.GenerateId();
            var aggregate = new User(id);

            //aggregate.Events.Apply(new AggregateCreated() { Id = id });

            //aggregate.Root.Id.ShouldBe(id);


            var handler = new UpdateUserHandler();
            var events = await handler.Handle(aggregate, new UpdateUser() { Name = "john" }).ToListAsync();
            events.Count.ShouldBe(1);
            events[0].ShouldBeOfType<UpdatedUser>().Id.ShouldBe(id);
            aggregate.Version.ShouldBe(new AggregateVersion(1));


            //repo commit

            //var x=  new UpdateUserHandler()
        }
    }
}