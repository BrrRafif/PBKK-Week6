using System.Data;
using Microsoft.Data.SqlClient;
using StudentRegistration.Models;

namespace StudentRegistration.Repositories;

public class StudentRepository
{
    private readonly string _connectionString =
        @"Server=.\SQLEXPRESS;
              Database=StudentRegistrationDB;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

    private const string SelectSql = """
        SELECT
            s.Id,
            s.NRP,
            s.Nama,
            s.ProgramId,
            p.ProgramName AS Jurusan,
            s.TanggalLahir,
            s.Alamat,
            s.NoTelepon,
            s.CreatedAt,
            s.UpdatedAt
        FROM Students s
        INNER JOIN Programs p
            ON s.ProgramId = p.ProgramId
        """;

    // READ 
    public List<Student> GetAll()
    {
        var students = new List<Student>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(SelectSql + " ORDER BY s.Id DESC;", connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
            students.Add(Map(reader));

        return students;
    }

    // SEARCH 
    public List<Student> Search(string keyword)
    {
        var students = new List<Student>();

        const string sql = SelectSql + """

            WHERE s.NRP LIKE @Keyword
               OR s.Nama LIKE @Keyword
               OR p.ProgramName LIKE @Keyword
               OR p.ProgramCode LIKE @Keyword
            ORDER BY s.Id DESC;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Keyword", SqlDbType.VarChar, 120).Value = "%" + keyword + "%";

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
            students.Add(Map(reader));

        return students;
    }

    // Total mahasiswa
    public int CountAll()
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT COUNT(*) FROM Students;", connection);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // CREATE
    public void Insert(Student student)
    {
        const string sql = """
            INSERT INTO Students
                (NRP, Nama, ProgramId, TanggalLahir, Alamat, NoTelepon)
            VALUES
                (@NRP, @Nama, @ProgramId, @TanggalLahir, @Alamat, @NoTelepon);
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, student);

        connection.Open();
        command.ExecuteNonQuery();
    }

    // UPDATE
    public void Update(Student student)
    {
        const string sql = """
            UPDATE Students SET
                NRP          = @NRP,
                Nama         = @Nama,
                ProgramId    = @ProgramId,
                TanggalLahir = @TanggalLahir,
                Alamat       = @Alamat,
                NoTelepon    = @NoTelepon,
                UpdatedAt    = SYSDATETIME()
            WHERE Id = @Id;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, student);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = student.Id;

        connection.Open();
        command.ExecuteNonQuery();
    }

    // DELETE
    public void Delete(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM Students WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool NrpExists(string nrp, int excludeId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Students WHERE NRP = @NRP AND Id <> @Id;", connection);
        command.Parameters.Add("@NRP", SqlDbType.VarChar, 10).Value = nrp;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = excludeId;

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void AddParameters(SqlCommand command, Student s)
    {
        command.Parameters.Add("@NRP", SqlDbType.VarChar, 10).Value = s.NRP;
        command.Parameters.Add("@Nama", SqlDbType.VarChar, 100).Value = s.Nama;
        command.Parameters.Add("@ProgramId", SqlDbType.Int).Value = s.ProgramId;
        command.Parameters.Add("@TanggalLahir", SqlDbType.Date).Value =
            s.TanggalLahir.HasValue ? (object)s.TanggalLahir.Value.Date : DBNull.Value;
        command.Parameters.Add("@Alamat", SqlDbType.VarChar, 250).Value =
            string.IsNullOrWhiteSpace(s.Alamat) ? DBNull.Value : (object)s.Alamat;
        command.Parameters.Add("@NoTelepon", SqlDbType.VarChar, 20).Value =
            string.IsNullOrWhiteSpace(s.NoTelepon) ? DBNull.Value : (object)s.NoTelepon;
    }

    private static Student Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("Id")),
        NRP = r.GetString(r.GetOrdinal("NRP")),
        Nama = r.GetString(r.GetOrdinal("Nama")),
        ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
        Jurusan = r.GetString(r.GetOrdinal("Jurusan")),
        TanggalLahir = r.IsDBNull(r.GetOrdinal("TanggalLahir"))
                         ? null : r.GetDateTime(r.GetOrdinal("TanggalLahir")),
        Alamat = r.IsDBNull(r.GetOrdinal("Alamat"))
                         ? string.Empty : r.GetString(r.GetOrdinal("Alamat")),
        NoTelepon = r.IsDBNull(r.GetOrdinal("NoTelepon"))
                         ? string.Empty : r.GetString(r.GetOrdinal("NoTelepon")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
        UpdatedAt = r.IsDBNull(r.GetOrdinal("UpdatedAt"))
                         ? null : r.GetDateTime(r.GetOrdinal("UpdatedAt"))
    };
}