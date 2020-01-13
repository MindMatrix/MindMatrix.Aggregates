// namespace PatientManagement.Commands
// {
//     using System.Collections.Generic;
//     using System.Runtime.CompilerServices;
//     using System.Threading;
//     using MindMatrix.Aggregates;

//     public class DischargePatient
//     {
//         public DischargePatient(AggregateId patientId)
//         {
//             PatientId = patientId;
//         }

//         public AggregateId PatientId { get; }
//     }

//     public class DischargedPatient : IAggregateMutator<Encounter>
//     {
//         public DischargedPatient(AggregateId patientId)
//         {
//             PatientId = patientId;
//         }

//         public AggregateId PatientId { get; }

//         public void Apply(Encounter aggregate)
//         {
//             aggregate.CurrentlyAdmitted = false;
//         }
//     }

//     public class DischargePatientHandler : AggregateCommand<Encounter, DischargePatient>
//     {
//         protected override async IAsyncEnumerable<IAggregateMutator<Encounter>> OnHandle(Encounter aggregate, DischargePatient request, [EnumeratorCancellation] CancellationToken cancellationToken)
//         {
//             aggregate.CheckPatientIsAdmitted();
//             yield return new DischargedPatient(aggregate.Id);
//         }
//     }
// }