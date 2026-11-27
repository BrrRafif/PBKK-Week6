using Microsoft.Data.SqlClient;
using StudentRegistration.Models;
using StudentRegistration.ViewModels;
using StudentRegistration.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace StudentRegistration.ViewModels;

public class StudentViewModel : INotifyPropertyChanged
{
    private static readonly string[] DotColors =
        { "#D4A72C", "#C08B2E", "#B8763A", "#A9894B", "#8C7A3E", "#6E5A28" };

    private readonly StudentRepository _studentRepository = new();
    private readonly ProgramRepository _programRepository = new();

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Programs> Programs { get; } = new();
    public ObservableCollection<ProgramSummary> ProgramSummaries { get; } = new();

    private Student? _selectedStudent;
    public Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;
            OnPropertyChanged();
            if (value != null)
                LoadToForm(value);
        }
    }

    private Student _formStudent = new();
    public Student FormStudent
    {
        get => _formStudent;
        set
        {
            _formStudent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(FormModeText));
        }
    }

    public bool IsEditMode => FormStudent.Id != 0;
    public string SaveButtonText => IsEditMode ? "Update" : "Simpan";
    public string FormModeText => IsEditMode ? $"Mode edit: NRP {FormStudent.NRP}" : "Mode tambah data baru";

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    private string _successMessage = "";
    public string SuccessMessage
    {
        get => _successMessage;
        set { _successMessage = value; OnPropertyChanged(); }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    private int _totalStudents;
    public int TotalStudents
    {
        get => _totalStudents;
        private set { _totalStudents = value; OnPropertyChanged(); }
    }

    public int TotalPrograms => Programs.Count;
    public int ShownStudents => Students.Count;

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ResetSearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand EditRowCommand { get; }
    public ICommand DeleteRowCommand { get; }

    public StudentViewModel()
    {
        SaveCommand = new RelayCommand(Save);
        ResetCommand = new RelayCommand(Reset);
        SearchCommand = new RelayCommand(Search);
        ResetSearchCommand = new RelayCommand(ResetSearch);
        RefreshCommand = new RelayCommand(Refresh);
        EditRowCommand = new RelayCommand(p =>
        {
            if (p is Student s) LoadToForm(s);
        });
        DeleteRowCommand = new RelayCommand(p =>
        {
            if (p is Student s) DeleteStudent(s);
        });

        Reset();
        LoadAll();
    }
    private void LoadAll()
    {
        try
        {
            LoadPrograms();
            LoadStudents();
            LoadStatistics();
        }
        catch (SqlException ex)
        {
            ErrorMessage = "⚠ Gagal terhubung ke SQL Server. Periksa instance .\\SQLEXPRESS " +
                           "dan database StudentRegistrationDB.\n" + ex.Message;
        }
    }

    private void LoadPrograms()
    {
        Programs.Clear();
        foreach (var program in _programRepository.GetActive())
            Programs.Add(program);

        OnPropertyChanged(nameof(TotalPrograms));
        OnPropertyChanged(nameof(FormStudent));
    }

    private void LoadStudents()
    {
        var result = string.IsNullOrWhiteSpace(SearchText)
            ? _studentRepository.GetAll()
            : _studentRepository.Search(SearchText.Trim());

        Students.Clear();
        foreach (var student in result)
            Students.Add(student);

        OnPropertyChanged(nameof(ShownStudents));
    }

    private void LoadStatistics()
    {
        TotalStudents = _studentRepository.CountAll();

        ProgramSummaries.Clear();
        var summaries = _programRepository.GetSummary();
        for (int i = 0; i < summaries.Count; i++)
        {
            summaries[i].Color = DotColors[i % DotColors.Length];
            ProgramSummaries.Add(summaries[i]);
        }
    }


    private void Save()
    {
        ErrorMessage = "";
        SuccessMessage = "";

        try
        {
            string? error = Validate(FormStudent);
            if (error != null)
            {
                ErrorMessage = error;
                return;
            }

            bool isNew = FormStudent.Id == 0;

            if (isNew)
                _studentRepository.Insert(FormStudent);
            else
                _studentRepository.Update(FormStudent);

            Reset();
            LoadStudents();
            LoadStatistics();

            SuccessMessage = isNew
                ? "✔ Data mahasiswa berhasil disimpan."
                : "✔ Data mahasiswa berhasil diperbarui.";
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            ErrorMessage = $"⚠ NRP {FormStudent.NRP} sudah terdaftar. NRP tidak boleh sama.";
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "⚠ Data ditolak database: jurusan tidak ditemukan atau format NRP salah.";
        }
        catch (SqlException ex)
        {
            ErrorMessage = "⚠ Database error: " + ex.Message;
        }
    }

    private string? Validate(Student s)
    {
        s.NRP = (s.NRP ?? "").Trim();
        s.Nama = (s.Nama ?? "").Trim();
        s.Alamat = (s.Alamat ?? "").Trim();
        s.NoTelepon = (s.NoTelepon ?? "").Trim();

        var kosong = new List<string>();
        if (s.NRP == "") kosong.Add("NRP");
        if (s.Nama == "") kosong.Add("Nama");
        if (s.ProgramId == 0) kosong.Add("Jurusan");

        if (kosong.Count > 0)
            return "⚠ Data belum lengkap. Isi terlebih dahulu: " + string.Join(", ", kosong) + ".";

        if (!Regex.IsMatch(s.NRP, @"^\d{10}$"))
            return "⚠ NRP harus tepat 10 digit angka, contoh: 5025241029.";

        if (s.TanggalLahir.HasValue && s.TanggalLahir.Value.Date > DateTime.Today)
            return "⚠ Tanggal lahir tidak boleh melebihi hari ini.";

        if (s.NoTelepon != "" && !Regex.IsMatch(s.NoTelepon, @"^\d{10,15}$"))
            return "⚠ No. telepon hanya angka, 10–15 digit.";

        if (_studentRepository.NrpExists(s.NRP, s.Id))
            return $"⚠ NRP {s.NRP} sudah terdaftar. NRP tidak boleh sama.";

        return null;
    }

    private void DeleteStudent(Student s)
    {
        var confirm = MessageBox.Show(
            $"Hapus data {s.Nama} (NRP {s.NRP})?",
            "Konfirmasi Hapus",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            _studentRepository.Delete(s.Id);
            Reset();
            LoadStudents();
            LoadStatistics();
            SuccessMessage = "✔ Data mahasiswa berhasil dihapus.";
        }
        catch (SqlException ex)
        {
            ErrorMessage = "⚠ Database error: " + ex.Message;
        }
    }

    // search

    private void Search()
    {
        try
        {
            LoadStudents();
        }
        catch (SqlException ex)
        {
            ErrorMessage = "⚠ Database error: " + ex.Message;
        }
    }

    private void ResetSearch()
    {
        SearchText = "";
        Search();
    }

    // refresh
    private void Refresh()
    {
        ErrorMessage = "";
        SuccessMessage = "";
        LoadAll();
        if (ErrorMessage == "")
            SuccessMessage = "✔ Data berhasil dimuat ulang dari database.";
    }

    // form

    private void Reset()
    {
        SelectedStudent = null;
        FormStudent = new Student();
        ErrorMessage = "";
        SuccessMessage = "";
    }

    private void LoadToForm(Student s)
    {
        FormStudent = new Student
        {
            Id = s.Id,
            NRP = s.NRP,
            Nama = s.Nama,
            ProgramId = s.ProgramId,
            Jurusan = s.Jurusan,
            TanggalLahir = s.TanggalLahir,
            Alamat = s.Alamat,
            NoTelepon = s.NoTelepon,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
        ErrorMessage = "";
        SuccessMessage = "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}