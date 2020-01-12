namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public class AggregateEventSequence : ValueObject
    {

        private readonly long _value;

        public AggregateEventSequence(long sequence)
        {
            _value = sequence;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _value;
        }

        public AggregateEventSequence Next() => new AggregateEventSequence(_value + 1);

        public static implicit operator string(AggregateEventSequence it) => it._value.ToString();
        public static explicit operator AggregateEventSequence(string it) => new AggregateEventSequence(long.Parse(it));
    }
}