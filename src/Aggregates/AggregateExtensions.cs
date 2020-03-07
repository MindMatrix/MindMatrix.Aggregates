using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AggregateExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> iterator)
    {
        var list = new List<T>();
        await foreach (var it in iterator)
            list.Add(it);

        return list;
    }

    public static bool IsConcerteImpl(this Type type, Type impl)
    {
        if (!type.IsClass)
            return false;

        if (type.IsAbstract)
            return false;

        return IsAssignableToGenericType(type, impl);
    }

    public static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }
}