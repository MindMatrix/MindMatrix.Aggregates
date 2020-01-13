// using System;
// using System.Collections.Generic;

// namespace MindMatrix.Aggregates
// {
//     public partial class AggregateVersion : ValueObject
//     {
//         private int value_;
//         public AggregateVersion(int version)
//         {
//             if (version < -1)
//                 throw new ArgumentOutOfRangeException(nameof(version));

//             value_ = version;
//         }

//         internal void Increment() => value_++;

//         public static implicit operator int(AggregateVersion it) => it.value_;

//         protected override IEnumerable<object> GetAtomicValues()
//         {
//             yield return value_;
//         }
//     }
// }