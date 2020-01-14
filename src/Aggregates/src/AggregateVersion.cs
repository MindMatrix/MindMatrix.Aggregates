using System;
using System.Collections.Generic;

namespace MindMatrix.Aggregates
{
    public partial class AggregateVersion : ValueObject
    {
        private long value_;
        public long Value => value_;
        public AggregateVersion(long version)
        {
            if (version < -1)
                throw new ArgumentOutOfRangeException(nameof(version));

            value_ = version;
        }

        internal void Increment() => value_++;

        internal void Set(AggregateStreamEventVersion version) => value_ = version.value_;

        public static implicit operator long(AggregateVersion it) => it.value_;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return value_;
        }
    }
}