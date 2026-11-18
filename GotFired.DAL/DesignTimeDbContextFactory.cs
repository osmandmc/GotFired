using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GotFired.DAL
{
    /// <summary>
    /// Lets "dotnet ef" build the context without starting the API.
    /// Connection string from GOTFIRED_CONNECTION; only needed for commands that touch a database.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GotFiredDbContext>
    {
        public GotFiredDbContext CreateDbContext(string[] args)
        {
            string connection = Environment.GetEnvironmentVariable("GOTFIRED_CONNECTION")
                                ?? "Server=localhost,1433;Database=gotfired;User ID=sa;Password=Dev_Passw0rd!;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<GotFiredDbContext>().UseSqlServer(connection).Options;
            return new GotFiredDbContext(options);
        }
    }
}
