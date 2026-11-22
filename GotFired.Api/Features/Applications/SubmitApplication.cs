using System.ComponentModel.DataAnnotations;
using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Applications
{
    /// <summary>
    /// POST /applications: the public form (form-encoded, as apply.html posts it). 202 Accepted: the case is queued
    /// for staff; the applicant hears back by email when it is answered. A repeated Guid (double click) is accepted
    /// without storing a second case.
    /// </summary>
    public static class SubmitApplication
    {
        /// <summary>
        /// The posted form. Value-type fields are nullable so an empty field reaches validation (a field message)
        /// instead of failing model binding (a bare 400); [Required] marks what the database requires.
        /// </summary>
        public sealed class Command
        {
            public string Guid { get; set; }
            [Required(ErrorMessage = "E-posta adresi zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
            public string ApplicantEmail { get; set; }
            public string ApplicantFullName { get; set; }
            public string ApplicantPhoneNumber { get; set; }
            [Required(ErrorMessage = "Cinsiyet seçin.")]
            public Gender? ApplicantGender { get; set; }
            public AgeInterval? ApplicantAgeInterval { get; set; }
            public EducationState? ApplicantEducationalState { get; set; }
            public int? ApplicantPositionId { get; set; }
            [Required(ErrorMessage = "Şehir seçin.")]
            public int? CityID { get; set; }
            public DateTime? EmploymentTerminationDate { get; set; }
            public int? DeclaredTerminationReasonID { get; set; }
            public string OtherDeclaredTerminationReason { get; set; }
            public int? SGKTerminationReasonID { get; set; }
            [Required(ErrorMessage = "Çalışma süresini seçin.")]
            public EmploymentDurationSince? EmploymentDurationSince { get; set; }
            [Required(ErrorMessage = "Çalışan sayısını seçin.")]
            public EmployeeCount? EmployeeCount { get; set; }
            public string CompanySectorName { get; set; }
            public string CompanyDescription { get; set; }
            public bool? HasSignedAnyDocument { get; set; }
            public string SignedDocuments { get; set; }
            [Required(ErrorMessage = "İşten çıkarılma durumunu seçin.")]
            public DismissalState? DismissalState { get; set; }
            public bool? WantShareHisExperience { get; set; }
            public string DetailedExperience { get; set; }
            public string AdditionalInfo { get; set; }
            public int[] SupportedByIds { get; set; }
        }

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, bool>
        {
            /// <returns>false when this submission was already stored (double click).</returns>
            public async Task<bool> Handle(Command c, CancellationToken ct)
            {
                if (!string.IsNullOrEmpty(c.Guid) && await db.DismissalCase.AnyAsync(d => d.Guid == c.Guid, ct)) return false;

                var applicant = Applicant.Create(c.ApplicantFullName, c.ApplicantEmail, c.ApplicantPhoneNumber,
                    c.ApplicantGender.Value, c.ApplicantAgeInterval, c.ApplicantEducationalState, c.ApplicantPositionId);
                var details = new CaseDetails
                {
                    CityId = c.CityID.Value,
                    EmploymentTerminationDate = c.EmploymentTerminationDate,
                    DeclaredTerminationReasonId = c.DeclaredTerminationReasonID,
                    OtherDeclaredTerminationReason = c.OtherDeclaredTerminationReason,
                    SgkTerminationReasonId = c.SGKTerminationReasonID,
                    EmploymentDurationSince = c.EmploymentDurationSince.Value,
                    EmployeeCount = c.EmployeeCount.Value,
                    CompanySectorName = c.CompanySectorName,
                    CompanyDescription = c.CompanyDescription,
                    HasSignedAnyDocument = c.HasSignedAnyDocument ?? false,
                    SignedDocuments = c.SignedDocuments,
                    DismissalState = c.DismissalState.Value,
                    WantShareHisExperience = c.WantShareHisExperience ?? false,
                    DetailedExperience = c.DetailedExperience,
                    AdditionalInfo = c.AdditionalInfo,
                };
                string guid = string.IsNullOrEmpty(c.Guid) ? System.Guid.NewGuid().ToString() : c.Guid;

                db.DismissalCase.Add(DismissalCase.Submit(guid, applicant, details, c.SupportedByIds));
                await db.SaveChangesAsync(ct);
                return true;
            }
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapPost("/applications", async ([FromForm] Command command, Handler handler, CancellationToken ct) =>
            {
                if (Problems.ValidationOrNull(command) is { } invalid) return invalid;
                await handler.Handle(command, ct);
                return Results.Accepted();
            })
            .AllowAnonymous()
            .DisableAntiforgery();   // public, cookie-less form: there is no session for a CSRF attack to ride on
    }
}
