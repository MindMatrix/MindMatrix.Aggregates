using System.Collections.Generic;
using System.Threading.Tasks;
using MindMatrix.Aggregates;
using StructureMap;

public static class AggregateExtensions
{
    public static void UseMemoryEventStore(this ConfigurationExpression cfg)
    {
        cfg.For<IEventStore>().Use<MemoryEventStore>();
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> iterator)
    {
        var list = new List<T>();
        await foreach (var it in iterator)
            list.Add(it);

        return list;
    }
}