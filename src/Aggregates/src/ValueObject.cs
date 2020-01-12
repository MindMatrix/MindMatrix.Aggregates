namespace MindMatrix.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ValueObject
    {
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            ValueObject other = (ValueObject)obj;
            IEnumerator<object> thisValues = GetAtomicValues().GetEnumerator();
            IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();
            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (ReferenceEquals(thisValues.Current, null) ^
                    ReferenceEquals(otherValues.Current, null))
                {
                    return false;
                }

                if (thisValues.Current != null &&
                    !thisValues.Current.Equals(otherValues.Current))
                {
                    return false;
                }
            }
            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        private const uint magic = 16777619u;
        private static uint MixHash(uint hash, uint data)
        {
            unchecked
            {
                uint lo16 = data & 0xFFFFu;
                uint hi16 = data >> 16;
                hash += lo16;
                hash ^= hash << 16;
                hash ^= hi16 << 11;
                hash += hash >> 11;
                return hash;
            }
        }

        private static uint PostHash(uint hash)
        {
            unchecked
            {
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 2;
                hash += hash >> 15;
                hash ^= hash << 10;
                return hash;
            }
        }

        public override int GetHashCode()
        {
            var hash = magic;
            foreach (var it in GetAtomicValues())
                hash = MixHash(hash, it != null ? (uint)it.GetHashCode() : 0);

            return (int)PostHash(hash);
        }
    }
}