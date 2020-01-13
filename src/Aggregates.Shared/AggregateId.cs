// namespace MindMatrix.Aggregates
// {
//     using System;
//     using System.Collections.Generic;

//     public partial class RpcAggregateId : ValueObject
//     {
//         private string value_;

//         public RpcAggregateId(Guid guid)
//         {
//             value_ = guid.ToString();
//         }

//         protected override IEnumerable<object> GetAtomicValues()
//         {
//             yield return value_;
//         }

//         public static RpcAggregateId GenerateId() => new RpcAggregateId(Guid.NewGuid());

//         public static implicit operator string(RpcAggregateId it) => it.value_;
//         public static explicit operator RpcAggregateId(Aggregateid it) => new RpcAggregateId(it.value_);

//         //public override string ToString() => $"RpcAggregateId: {_value}";
//     }
// }