namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public class AggregateId : ValueObject
    {

        private readonly Guid _value;

        public AggregateId(Guid guid)
        {
            _value = guid;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _value;
        }

        public static AggregateId GenerateId() => new AggregateId(Guid.NewGuid());

        public static implicit operator string(AggregateId it) => it._value.ToString();
        public static explicit operator AggregateId(string it) => new AggregateId(Guid.Parse(it));
    }
}