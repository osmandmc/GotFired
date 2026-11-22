using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GotFired.Api.Tests.Infrastructure;

/// <summary>Runs sqlcmd-style scripts (batches separated by GO) on one connection, like sqlcmd -b -I.</summary>
public static class SqlScript
{
    public static async Task RunFileAsync(string connectionString, string relativePath)
    {
        string path = Path.Combine(AppContext.BaseDirectory, relativePath);
        await RunAsync(connectionString, await File.ReadAllTextAsync(path));
    }

    public static async Task RunAsync(string connectionString, string script)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        foreach (string batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync();
        }
    }

    public static async Task<List<string>> QueryLinesAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var lines = new List<string>();
        while (await reader.ReadAsync()) lines.Add(reader.IsDBNull(0) ? null : Convert.ToString(reader.GetValue(0)));
        return lines;
    }

    public static async Task<T> ScalarAsync<T>(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return (T)await command.ExecuteScalarAsync();
    }
}
