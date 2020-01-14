namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Net.Client;

    public class GrpcAggregateEventStream<T> : IAggregateEventStream<T>
        where T : IAggregateRoot
    {
        private readonly AggregateService.AggregateServiceClient _client;
        public GrpcAggregateEventStream(GrpcChannel channel)
        {
            _client = new AggregateService.AggregateServiceClient(channel);
        }

        public async Task<AggregateVersion> Append(AggregateId id, AggregateVersion expectedVersion, IEnumerable<IAggregateMutator<T>> events, CancellationToken cancellationToken)
        {
            var request = new AppendRequest()
            {
                Id = id.Value,
                ExpectedVersion = expectedVersion.Value
            };

            request.Events.AddRange(events.Select(x => new AppendEvent()
            {
                Id = AggregateStreamEventId.GenerateId().ToString(),
                Data = null,
                Metadata = null
            }));

            var result = await _client.AppendAsync(request, cancellationToken: cancellationToken);
            return new AggregateVersion(result.CommitedVersion);
        }

        public IAsyncEnumerable<IAggregateStreamEvent<T>> Open(AggregateId id)
        {
            throw new NotImplementedException();
        }
    }
}