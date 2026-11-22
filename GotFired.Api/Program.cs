using System.Text;
using System.Text.Json.Serialization;
using GotFired.Api.Features;
using GotFired.Api.Features.ApplicantAccess;
using GotFired.Api.Features.Auth;
using GotFired.Api.Infrastructure;
using GotFired.Api.Infrastructure.Email;
using GotFired.DAL;
using GotFired.DAL.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// --- Data -------------------------------------------------------------------------------------
int compatibilityLevel = config.GetValue("Database:CompatibilityLevel", 160);
builder.Services.AddDbContext<GotFiredDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("GotFired")
            ?? throw new InvalidOperationException("ConnectionStrings:GotFired is not configured."),
        sql =>
        {
            // EF6 always loaded multiple collection includes in one query; keep that explicitly.
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            // Generate SQL for the database's compatibility level. A database restored from the old SQL Server 2008 R2
            // keeps level 100 until raised (db/README.md); at 100, e.g. OPENJSON (EF's default for list.Contains) does
            // not exist. Checked against the real level at startup below.
            sql.UseCompatibilityLevel(compatibilityLevel);
        }));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // ASP.NET Identity 2 defaults, which existing passwords were created under.
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.AllowedUserNameCharacters = null; // v2 allowed any characters when configured this way
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GotFiredDbContext>();

// Identity v2 password hashes are verified transparently and rehashed to v3 on next login.
// Set Identity:PasswordHashCompatibilityV2=true while a rollback to the old .NET Framework app must stay possible,
// because the old app cannot verify v3 hashes.
builder.Services.Configure<PasswordHasherOptions>(o =>
{
    if (config.GetValue<bool>("Identity:PasswordHashCompatibilityV2"))
        o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2;
});

// --- Features (vertical slices: one command or query handler per use case) ----------------------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHandlers();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>(); // stateless; also used by the singleton exception handler
builder.Services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<ApplicantLinkOptions>(config.GetSection(ApplicantLinkOptions.SectionName));

// --- Auth (replaces the OWIN OAuth authorization server) -------------------------------------
builder.Services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException("Jwt:Key must be configured with at least 32 bytes.");

builder.Services.AddSingleton<TokenService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = TokenService.NameClaimType,
            RoleClaimType = TokenService.RoleClaimType,
        };
    });
builder.Services.AddEndpointPolicies();

// --- HTTP ---------------------------------------------------------------------------------------
builder.Services.ConfigureHttpJsonOptions(o =>
{
    // PascalCase property names: the existing pages read e.g. data.FullName. Problem Details keep their standard
    // lower-case names (type, title, status, detail, errors).
    o.SerializerOptions.PropertyNamingPolicy = null;
    o.SerializerOptions.DictionaryKeyPolicy = null;
    // The AngularJS pages send ids taken from the URL as strings ("42").
    o.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
});

// Order matters: rule violations are answered (422) before anything is treated as an error.
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
builder.Services.AddExceptionHandler<EmailExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // The container sits behind a reverse proxy on a private network.
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.Use((context, next) =>
{
    // Defence in depth for the applicant page (its link carries a secret token in the URL fragment).
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    return next(context);
});
app.UseExceptionHandler();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapFeatures();

if (config.GetValue<bool>("Seed:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<GotFiredDbContext>();
    await db.Database.MigrateAsync();
    await DevDataSeeder.SeedAsync(db,
        sp.GetRequiredService<UserManager<ApplicationUser>>(),
        sp.GetRequiredService<RoleManager<IdentityRole>>(),
        config["Seed:AdminUserName"] ?? "admin",
        config["Seed:AdminPassword"] ?? throw new InvalidOperationException("Seed:AdminPassword is required when Seed:Enabled is true."));
}

// Fail fast if EF would generate SQL the database cannot run (e.g. a restored backup still at level 100).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GotFiredDbContext>();
    int actual = await db.Database
        .SqlQueryRaw<int>("SELECT CAST(compatibility_level AS int) AS [Value] FROM sys.databases WHERE name = DB_NAME()")
        .SingleAsync();
    if (actual < compatibilityLevel)
        throw new InvalidOperationException(
            $"Database compatibility level is {actual} but Database:CompatibilityLevel is {compatibilityLevel}. " +
            $"Raise it (ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = {compatibilityLevel}) or set Database__CompatibilityLevel={actual}. See db/README.md.");
}

app.Run();

public partial class Program;
