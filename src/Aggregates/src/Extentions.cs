namespace MindMatrix.Aggregates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class Extentions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> iterator)
        {
            var list = new List<T>();
            await foreach (var it in iterator)
                list.Add(it);

            return list;
        }
    }
}