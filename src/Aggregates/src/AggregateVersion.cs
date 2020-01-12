using System;
using System.Collections.Generic;

namespace MindMatrix.Aggregates
{
    public class AggregateVersion : ValueObject
    {
        internal int _version;

        public AggregateVersion()
        {
            _version = -1;
        }

        public AggregateVersion(int version)
        {
            if (version < 0)
                throw new ArgumentOutOfRangeException(nameof(version));

            _version = version;
        }

        internal void Increment() => _version++;

        public static implicit operator int(AggregateVersion it) => it._version;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _version;
        }
    }
}