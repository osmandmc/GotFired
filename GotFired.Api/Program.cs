using System.Text;
using GotFired.Api.Auth;
using GotFired.Api.Infrastructure;
using GotFired.Business;
using GotFired.DAL;
using GotFired.Model.Identity;
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
builder.Services.AddDbContext<GotFiredDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("GotFired")
            ?? throw new InvalidOperationException("ConnectionStrings:GotFired is not configured."),
        // EF6 always loaded multiple collection includes in one query; keep that explicitly.
        sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

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
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthBusiness>();
builder.Services.AddScoped<DismissalCaseBusiness>();
builder.Services.AddScoped<ParameterBusiness>();
builder.Services.AddSingleton<SmtpManager>(); // stateless; also used by the singleton exception handler
builder.Services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));

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

app.Run();

public partial class Program;
