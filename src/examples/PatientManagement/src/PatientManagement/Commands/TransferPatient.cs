// namespace PatientManagement.Commands
// {
//     using System.Collections.Generic;
//     using System.Runtime.CompilerServices;
//     using System.Threading;
//     using MindMatrix.Aggregates;

//     public class TransferPatient
//     {
//         public TransferPatient(AggregateId patientId, int wardNumber)
//         {
//             PatientId = patientId;
//             WardNumber = wardNumber;
//         }

//         public AggregateId PatientId { get; }

//         public int WardNumber { get; }
//     }

//     public class PatientTransfered : IAggregateMutator<Encounter>
//     {
//         public PatientTransfered(AggregateId patientId, int wardNumber)
//         {
//             PatientId = patientId;
//             WardNumber = wardNumber;
//         }

//         public AggregateId PatientId { get; }

//         public int WardNumber { get; }
//         public void Apply(Encounter aggregate)
//         {
//             throw new System.NotImplementedException();
//         }
//     }

//     public class TransferPatientHandler : AggregateCommand<Encounter, TransferPatient>
//     {
//         protected override async IAsyncEnumerable<IAggregateMutator<Encounter>> OnHandle(Encounter aggregate, TransferPatient request, [EnumeratorCancellation] CancellationToken cancellationToken)
//         {
//             aggregate.CheckPatientIsAdmitted();
//             yield return new PatientTransfered(aggregate.Id, request.WardNumber);
//         }
//     }
// }