namespace StudentRegistration.Models;

public class Student
{
    public int Id { get; set; }

    public string NRP { get; set; } = string.Empty;

    public string Nama { get; set; } = string.Empty;

    public int ProgramId { get; set; }

    public string Jurusan { get; set; } = string.Empty;

    public DateTime? TanggalLahir { get; set; }

    public string Alamat { get; set; } = string.Empty;

    public string NoTelepon { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}