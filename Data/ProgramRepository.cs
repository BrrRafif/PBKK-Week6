using Microsoft.Data.SqlClient;
using StudentRegistration.Models;

namespace StudentRegistration.Repositories;

public class ProgramRepository
{
    private readonly string _connectionString =
        @"Server=.\SQLEXPRESS;
          Database=StudentRegistrationDB;
          Trusted_Connection=True;
          TrustServerCertificate=True;";

    public List<Programs> GetActive()
    {
        var programs = new List<Programs>();

        const string sql = """
            SELECT ProgramId, ProgramCode, ProgramName, IsActive
            FROM Programs
            WHERE IsActive = 1
            ORDER BY ProgramName;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            programs.Add(new Programs
            {
                ProgramId = reader.GetInt32(reader.GetOrdinal("ProgramId")),
                ProgramCode = reader.GetString(reader.GetOrdinal("ProgramCode")),
                ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }

        return programs;
    }

    public List<ProgramSummary> GetSummary()
    {
        var result = new List<ProgramSummary>();

        const string sql = """
            SELECT
                p.ProgramId,
                p.ProgramName,
                COUNT(s.Id) AS JumlahMahasiswa
            FROM Programs p
            LEFT JOIN Students s
                ON p.ProgramId = s.ProgramId
            WHERE p.IsActive = 1
            GROUP BY p.ProgramId, p.ProgramName
            ORDER BY p.ProgramId;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new ProgramSummary
            {
                ProgramId = reader.GetInt32(reader.GetOrdinal("ProgramId")),
                ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
                Total = reader.GetInt32(reader.GetOrdinal("JumlahMahasiswa"))
            });
        }

        return result;
    }
}