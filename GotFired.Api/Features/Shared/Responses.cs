using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GotFired.Api.Features.Shared
{
    /// <summary>Id/name pair for drop-downs and tags.</summary>
    public sealed record Lookup(int ID, string Name);

    /// <summary>A page of a collection: { data, paging } (the shape the SPA's lists and pager use).</summary>
    public sealed record Page<T>(IReadOnlyList<T> data, Paging paging);

    public sealed record Paging(int TotalItems, int ItemsPerPage, int CurrentPage);

    public sealed record CommentResponse(int ID, string Text, string UserName, DateTime CreatedDate);

    public static class Problems
    {
        public static IResult NotFound(string detail = null) =>
            Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Kayıt bulunamadı.", detail: detail);

        /// <summary>400 with the DataAnnotations errors of <paramref name="model"/>, or null when it is valid.</summary>
        public static IResult ValidationOrNull(object model)
        {
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true)) return null;
            var errors = results
                .SelectMany(r => r.MemberNames.DefaultIfEmpty(""), (r, member) => (member, r.ErrorMessage))
                .GroupBy(e => e.member)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }
    }

    /// <summary>Enum values with their Turkish [Display] names, for the form's drop-downs.</summary>
    public static class EnumLookups
    {
        public static IReadOnlyList<Lookup> Of<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(v => new Lookup(Convert.ToInt32(v), typeof(TEnum).GetMember(v.ToString())[0].GetCustomAttribute<DisplayAttribute>()?.GetName() ?? v.ToString()))
                .ToList();
    }

    public static class PagingDefaults
    {
        public const int PageSize = 20;

        public static (int Page, int PageSize) Normalize(int? page, int? pageSize) =>
            (Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? PageSize, 1, 100));
    }
}
