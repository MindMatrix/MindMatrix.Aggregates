namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;

    public partial class AggregateId : ValueObject
    {
        private string value_;

        public string Value => value_;

        public AggregateId(Guid guid)
        {
            value_ = guid.ToString();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return value_;
        }

        public static AggregateId GenerateId() => new AggregateId(Guid.NewGuid());

        public static implicit operator string(AggregateId it) => it.value_;
        public static explicit operator AggregateId(string it) => new AggregateId(Guid.Parse(it));

        //public override string ToString() => $"AggregateId: {_value}";
    }
}