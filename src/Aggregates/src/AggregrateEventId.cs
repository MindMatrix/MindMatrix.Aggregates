namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public class AggregateEventId : ValueObject
    {

        private readonly Guid _value;

        public AggregateEventId(Guid guid)
        {
            _value = guid;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _value;
        }

        public static implicit operator string(AggregateEventId it) => it._value.ToString();
        public static explicit operator AggregateEventId(string it) => new AggregateEventId(Guid.Parse(it));
    }
}