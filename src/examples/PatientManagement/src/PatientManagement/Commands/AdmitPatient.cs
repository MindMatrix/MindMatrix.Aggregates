// namespace PatientManagement.Commands
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Runtime.CompilerServices;
//     using System.Threading;
//     using MindMatrix.Aggregates;

//     public class AdmitPatient
//     {
//         public AdmitPatient(AggregateId patientId, string patientName, int ageInYears, DateTime timeOfAdmission, int wardNumber)
//         {
//             PatientId = patientId;
//             PatientName = patientName;
//             AgeInYears = ageInYears;
//             TimeOfAdmission = timeOfAdmission;
//             WardNumber = wardNumber;
//         }

//         public AggregateId PatientId { get; }

//         public string PatientName { get; }

//         public int AgeInYears { get; }

//         public DateTime TimeOfAdmission { get; }

//         public int WardNumber { get; }
//     }

//     public class PatientAdmitted : IAggregateMutator<Encounter>
//     {
//         public AggregateId PatientId { get; }

//         public string PatientName { get; }

//         public int AgeInYears { get; }

//         public int WardNumber { get; }

//         public PatientAdmitted(AggregateId patientId, string patientName, int ageInYears, int wardNumber)
//         {
//             PatientId = patientId;
//             PatientName = patientName;
//             AgeInYears = ageInYears;
//             WardNumber = wardNumber;
//         }

//         public void Apply(Encounter aggregate)
//         {
//             aggregate.CurrentlyAdmitted = true;
//             //aggregate.Id = request.PatientId;
//             aggregate.PatientName = PatientName;
//             aggregate.AgeInYears = AgeInYears;
//             aggregate.Ward = WardNumber;
//         }
//     }

//     public class AdmitPatientHandler : AggregateCommand<Encounter, AdmitPatient>
//     {
//         protected override async IAsyncEnumerable<IAggregateMutator<Encounter>> OnHandle(Encounter aggregate, AdmitPatient request, [EnumeratorCancellation] CancellationToken cancellationToken)
//         {
//             yield return new AggregateCreated<Encounter>(aggregate.Id);
//             yield return new PatientAdmitted(aggregate.Id, request.PatientName, request.AgeInYears, request.WardNumber);
//         }
//     }
// }