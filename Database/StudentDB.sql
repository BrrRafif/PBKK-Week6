CREATE DATABASE StudentRegistrationDB;
GO

USE StudentRegistrationDB;
GO

SELECT DB_NAME() AS CurrentDatabase;
GO

-- Tabel Programs (jurusan)
CREATE TABLE Programs
(
    ProgramId INT IDENTITY(1,1)
        CONSTRAINT PK_Programs PRIMARY KEY,

    ProgramCode VARCHAR(20) NOT NULL
        CONSTRAINT UQ_Programs_ProgramCode UNIQUE,

    ProgramName VARCHAR(100) NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Programs_IsActive DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Programs_CreatedAt DEFAULT SYSDATETIME()
);
GO

INSERT INTO Programs (ProgramCode, ProgramName)
VALUES
('IF', 'Teknik Informatika'),    
('EE', 'Teknik Elektro'),        
('BE', 'Teknik Biomedik'),        
('CE', 'Teknik Komputer'),        
('IT', 'Teknologi Informasi'),    
('SI', 'Sistem Informasi');       
GO

SELECT * FROM Programs;
GO

-- Tabel Students
CREATE TABLE Students
(
    Id INT IDENTITY(1,1)
        CONSTRAINT PK_Students PRIMARY KEY,

    NRP VARCHAR(10) NOT NULL
        CONSTRAINT UQ_Students_NRP UNIQUE
        CONSTRAINT CK_Students_NRP
            CHECK (NRP LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),

    Nama VARCHAR(100) NOT NULL,

    ProgramId INT NOT NULL,

    TanggalLahir DATE NULL,

    Alamat VARCHAR(250) NULL,

    NoTelepon VARCHAR(20) NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME(),

    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Students_Programs
        FOREIGN KEY (ProgramId)
        REFERENCES Programs(ProgramId)
);
GO

INSERT INTO Students (NRP, Nama, ProgramId, TanggalLahir, Alamat, NoTelepon)
VALUES
('5022201029', 'Kamarudin',         2, '2003-03-12', 'Jl. Merdeka No. 123, Surabaya',   '081234567890'),
('5023231029', 'Siti Aminah',       3, '2004-07-21', 'Jl. Diponegoro No. 45, Surabaya', '082233445566'),
('5024211029', 'Andi Wijaya',       4, '2003-11-05', 'Jl. Ahmad Yani No. 10, Surabaya', '081355667788'),
('5025251029', 'Rina Sari',         1, '2004-01-18', 'Jl. Darmo No. 20, Surabaya',      '089900112233'),
('5026251029', 'Bambang Pamungkas', 6, '2003-09-27', 'Jl. Raya ITS, Surabaya',          '081288776655'),
('5027251029', 'Budi Sudarsono',    5, '2004-04-09', 'Jl. Keputih No. 7, Surabaya',     '085766554433');
GO

SELECT * FROM Students;
GO

SELECT
    s.Id,
    s.NRP,
    s.Nama,
    p.ProgramCode,
    p.ProgramName AS Jurusan,
    s.TanggalLahir,
    s.NoTelepon
FROM Students s
INNER JOIN Programs p
    ON s.ProgramId = p.ProgramId;
GO