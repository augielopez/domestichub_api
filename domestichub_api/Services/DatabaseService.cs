using Npgsql;
using System;

namespace domestichub_api.Services;

class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SupabaseDb");
    }

    public async Task TestConnectionAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            Console.WriteLine("Connected to Supabase database successfully!");

            // Example query
            using var command = new NpgsqlCommand("SELECT NOW()", connection);
            var result = await command.ExecuteScalarAsync();
            Console.WriteLine($"Database time: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to the database: {ex.Message}");
        }
    }
}