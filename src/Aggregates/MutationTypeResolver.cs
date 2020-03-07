namespace MindMatrix.Aggregates
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;

    public class MutationType
    {
        public string Name { get; }
        public Type Type { get; }

        public MutationType(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    public interface IMutationTypeResolver
    {
        MutationType GetByName(string mutationType);
        MutationType GetByType(Type type);
    }

    public class MutationTypeResolver : IMutationTypeResolver
    {
        private readonly Dictionary<string, MutationType> _named = new Dictionary<string, MutationType>();
        private readonly Dictionary<Type, MutationType> _types = new Dictionary<Type, MutationType>();

        public MutationTypeResolver(Assembly assembly)
        {
            var types = from x in assembly.GetExportedTypes()
                        where x.IsConcerteImpl(typeof(IMutation<>))
                        select x;

            foreach (var it in types)
            {
                var mutationType = new MutationType(it.Name, it);
                _named.Add(mutationType.Name, mutationType);
                _types.Add(mutationType.Type, mutationType);
            }
        }

        public MutationType GetByName(string mutationType) => _named[mutationType];

        public MutationType GetByType(Type type) => _types[type];
    }
}