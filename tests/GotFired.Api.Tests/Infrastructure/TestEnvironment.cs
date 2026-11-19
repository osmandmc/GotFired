using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using GotFired.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(GotFired.Api.Tests.Infrastructure.TestEnvironment))]

namespace GotFired.Api.Tests.Infrastructure;

/// <summary>
/// One environment for the whole test run: SQL Server 2022 and Mailpit in containers, and the API in-process.
/// The "legacy" database is built exactly like production on cutover day (db/README.md): the EF6-era schema with
/// Turkish_CI_AS collation, compatibility level raised to 160, db/upgrade-legacy-database.sql, db/migrations.sql.
/// Tests share it, so each test creates its own data (unique names) and must not depend on test order.
/// </summary>
public sealed class TestEnvironment : IAsyncLifetime
{
    public const string AdminUser = "legacy-admin";      // from db/tests/legacy-ef6-schema.sql, v2 password hash
    public const string PlainUser = "legacy-user";       // role "user"
    public const string LegacyPassword = "legacy-pass";

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly IContainer _mailpit = new ContainerBuilder("axllent/mailpit:v1.27")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8025).ForPath("/api/v1/messages")))
        .Build();

    private WebApplicationFactory<Program> _factory;

    public string LegacyConnectionString { get; private set; }
    public string FreshConnectionString { get; private set; }
    public Mailpit Mail { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_sql.StartAsync(), _mailpit.StartAsync());
        string server = _sql.GetConnectionString();

        await SqlScript.RunAsync(server, """
            CREATE DATABASE legacy COLLATE Turkish_CI_AS;
            ALTER DATABASE legacy SET COMPATIBILITY_LEVEL = 100;
            CREATE DATABASE fresh;
            """);
        LegacyConnectionString = WithDatabase(server, "legacy");
        FreshConnectionString = WithDatabase(server, "fresh");

        await SqlScript.RunFileAsync(LegacyConnectionString, "db/tests/legacy-ef6-schema.sql");
        await SqlScript.RunAsync(LegacyConnectionString, "ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 160;");
        await SqlScript.RunFileAsync(LegacyConnectionString, "db/upgrade-legacy-database.sql");
        await SqlScript.RunFileAsync(LegacyConnectionString, "db/migrations.sql");

        await using (var fresh = new GotFiredDbContext(new DbContextOptionsBuilder<GotFiredDbContext>().UseSqlServer(FreshConnectionString).Options, TimeProvider.System))
        {
            await fresh.Database.MigrateAsync();
        }

        Mail = new Mailpit(new Uri($"http://{_mailpit.Hostname}:{_mailpit.GetMappedPublicPort(8025)}"));

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:GotFired", LegacyConnectionString);
            builder.UseSetting("Jwt:Key", "integration-tests-signing-key-0123456789-abcdef");
            builder.UseSetting("Smtp:Host", _mailpit.Hostname);
            builder.UseSetting("Smtp:Port", _mailpit.GetMappedPublicPort(1025).ToString());
            builder.UseSetting("Smtp:UseStartTls", "false");
            builder.UseSetting("Smtp:UserName", "");
            builder.UseSetting("Smtp:From", "noreply@tests.local");
            builder.UseSetting("Smtp:ExceptionRecipient", "errors@tests.local");
            builder.UseSetting("Smtp:PublicBaseUrl", "https://gotfired.test");
            builder.UseSetting("Logging:LogLevel:Default", "Warning");
            // Catch DI lifetime mistakes (e.g. a singleton holding a scoped service) like Development does.
            builder.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; });
        });
    }

    private readonly List<WebApplicationFactory<Program>> _variants = [];

    /// <summary>A client that does not follow redirects and keeps no cookies, like the SPA's XHR calls.</summary>
    public ApiClient CreateClient() => CreateClient(_factory);

    /// <summary>A client for an API instance with some services replaced (e.g. a fake clock). Same database.</summary>
    public ApiClient CreateClient(Action<IServiceCollection> replaceServices)
    {
        var variant = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(replaceServices));
        lock (_variants) _variants.Add(variant);
        return CreateClient(variant);
    }

    private ApiClient CreateClient(WebApplicationFactory<Program> factory) =>
        new(factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false }), Mail);

    public async ValueTask DisposeAsync()
    {
        foreach (var variant in _variants) await variant.DisposeAsync();
        if (_factory != null) await _factory.DisposeAsync();
        await Task.WhenAll(_sql.DisposeAsync().AsTask(), _mailpit.DisposeAsync().AsTask());
    }

    private static string WithDatabase(string connectionString, string database) =>
        new SqlConnectionStringBuilder(connectionString) { InitialCatalog = database }.ConnectionString;
}
