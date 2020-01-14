namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public class AggregateStreamEventId : ValueObject
    {

        private readonly Guid _value;

        public AggregateStreamEventId(Guid guid)
        {
            _value = guid;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _value;
        }

        public static AggregateStreamEventId GenerateId() => new AggregateStreamEventId(Guid.NewGuid());

        public static implicit operator string(AggregateStreamEventId it) => it._value.ToString();
        public static explicit operator AggregateStreamEventId(string it) => new AggregateStreamEventId(Guid.Parse(it));
    }
}