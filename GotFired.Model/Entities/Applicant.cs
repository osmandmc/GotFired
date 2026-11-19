using GotFired.Model.Entities.Enums;

namespace GotFired.Model.Entities
{
    /// <summary>The person who submitted a case. Set once, on submission.</summary>
    public class Applicant : BaseEntity
    {
        private Applicant() { } // for EF Core

        public static Applicant Create(string fullName, string email, string phoneNumber, Gender gender,
            AgeInterval? ageInterval, EducationState? educationalState, int? positionId) => new()
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Gender = gender,
            AgeInterval = ageInterval,
            EducationalState = educationalState,
            PositionId = positionId,
        };

        public AgeInterval? AgeInterval { get; private set; }
        public EducationState? EducationalState { get; private set; }
        public Gender Gender { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string FullName { get; private set; }
        public int? PositionId { get; private set; }
    }
}
