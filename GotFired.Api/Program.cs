using System.Text;
using GotFired.Api.Auth;
using GotFired.Api.Infrastructure;
using GotFired.Business;
using GotFired.Business.Email;
using GotFired.Business.Services;
using GotFired.DAL;
using GotFired.DAL.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

// --- Application services ---------------------------------------------------------------------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IAnswerTemplateService, AnswerTemplateService>();
builder.Services.AddScoped<IUserService, UserService>();
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
builder.Services.AddAuthorization();

// --- MVC ---------------------------------------------------------------------------------------
builder.Services
    // Web API 2 returned Ok("text") as a JSON string; ASP.NET Core would write it as raw text/plain.
    .AddControllers(o => o.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter>())
    // Keep Web API 2's Newtonsoft behaviour: PascalCase property names, ISO dates, lenient number parsing.
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.ContractResolver = new DefaultContractResolver();
        o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    // Web API 2 did not reject invalid models automatically; controllers check ModelState themselves.
    o.SuppressModelStateInvalidFilter = true;
});

// Order matters: rule violations are answered with 412 before anything is treated as an error.
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
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
    // Applicant links carry a secret token in the URL: never leak it to other sites via the Referer header
    // (userView.html loads third-party scripts).
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    return next(context);
});
app.UseExceptionHandler();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapTokenEndpoint();
app.MapControllers();

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
