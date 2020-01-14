namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public class AggregateStreamEventVersion : ValueObject
    {

        internal readonly long value_;

        public long Value => value_;

        public AggregateStreamEventVersion(long sequence)
        {
            value_ = sequence;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return value_;
        }

        public AggregateStreamEventVersion Next() => new AggregateStreamEventVersion(value_ + 1);

        public static implicit operator string(AggregateStreamEventVersion it) => it.value_.ToString();
        public static explicit operator AggregateStreamEventVersion(string it) => new AggregateStreamEventVersion(long.Parse(it));
    }
}