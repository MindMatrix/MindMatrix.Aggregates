// namespace PatientManagement
// {
//     using MindMatrix.Aggregates;
//     public class Encounter : AggregateRoot
//     {
//         public string PatientName { get; internal set; }
//         public int AgeInYears { get; internal set; }
//         public int Ward { get; internal set; }
//         public bool CurrentlyAdmitted { get; internal set; }

//         // public Encounter(AggregateId patientId, string patientName, int ageInYears, int wardNumber)
//         // {
//         //     Id = patientId;

//         //     //Raise(new PatientAdmitted(patientId, patientName, ageInYears, wardNumber));
//         // }

//         public void CheckPatientIsAdmitted()
//         {
//             if (!CurrentlyAdmitted)
//             {
//                 throw new DomainException("Patient needs to be admitted first.");
//             }
//         }
//     }
// }