using System.Data.SqlClient;
using System.Configuration;

namespace StudentReportInitial.Data
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StudentReportDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task InitializeDatabaseAsync()
        {
            // Create default admin password hash
            string adminPwdHash, adminPwdSalt;
            PasswordHasher.CreatePasswordHash("admin123", out adminPwdHash, out adminPwdSalt);

            var createDbScript = @"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentReportDB')
                BEGIN
                    CREATE DATABASE StudentReportDB;
                END
            ";

            var createTablesScript = @"
                USE StudentReportDB;

                -- Users table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                BEGIN
                    CREATE TABLE Users (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Username NVARCHAR(50) UNIQUE NOT NULL,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        PasswordSalt NVARCHAR(255) NOT NULL,
                        FirstName NVARCHAR(100) NOT NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        Email NVARCHAR(100) UNIQUE NOT NULL,
                        Phone NVARCHAR(20),
                        Role INT NOT NULL, -- 1=Admin, 2=Professor, 3=Guardian, 4=Student
                        CreatedDate DATETIME2 DEFAULT GETDATE(),
                        IsActive BIT DEFAULT 1
                    );
                END

                -- Students table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Students' AND xtype='U')
                BEGIN
                    CREATE TABLE Students (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StudentId NVARCHAR(20) UNIQUE NOT NULL,
                        FirstName NVARCHAR(100) NOT NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        DateOfBirth DATE NOT NULL,
                        Gender NVARCHAR(10) NOT NULL,
                        Address NVARCHAR(255),
                        Phone NVARCHAR(20),
                        Email NVARCHAR(100),
                        GradeLevel NVARCHAR(20) NOT NULL,
                        Section NVARCHAR(20) NOT NULL,
                        GuardianId INT NOT NULL,
                        EnrollmentDate DATE DEFAULT GETDATE(),
                        IsActive BIT DEFAULT 1,
                        Username NVARCHAR(50) UNIQUE NOT NULL,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        PasswordSalt NVARCHAR(255) NOT NULL,
                        FOREIGN KEY (GuardianId) REFERENCES Users(Id)
                    );
                END

                -- Subjects table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Subjects' AND xtype='U')
                BEGIN
                    CREATE TABLE Subjects (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Code NVARCHAR(20) UNIQUE NOT NULL,
                        Description NVARCHAR(255),
                        ProfessorId INT NULL,  -- Made nullable to support unclaimed subjects
                        GradeLevel NVARCHAR(20) NOT NULL,
                        Section NVARCHAR(20) NOT NULL,
                        IsActive BIT DEFAULT 1,
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id)
                    );
                END

                -- StudentSubjects junction table for many-to-many relationship
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StudentSubjects' AND xtype='U')
                BEGIN
                    CREATE TABLE StudentSubjects (
                        StudentId INT NOT NULL,
                        SubjectId INT NOT NULL,
                        EnrollmentDate DATETIME2 DEFAULT GETDATE(),
                        PRIMARY KEY (StudentId, SubjectId),
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (SubjectId) REFERENCES Subjects(Id)
                    );
                END

                -- Attendance table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Attendance' AND xtype='U')
                BEGIN
                    CREATE TABLE Attendance (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StudentId INT NOT NULL,
                        ProfessorId INT NOT NULL,
                        Subject NVARCHAR(100) NOT NULL,
                        Date DATE NOT NULL,
                        Status INT NOT NULL, -- 1=Present, 2=Absent, 3=Late, 4=Excused
                        Notes NVARCHAR(255),
                        RecordedDate DATETIME2 DEFAULT GETDATE(),
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id)
                    );
                END

                -- Grades table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Grades' AND xtype='U')
                BEGIN
                    CREATE TABLE Grades (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StudentId INT NOT NULL,
                        ProfessorId INT NOT NULL,
                        Subject NVARCHAR(100) NOT NULL,
                        AssignmentType NVARCHAR(50) NOT NULL,
                        AssignmentName NVARCHAR(100) NOT NULL,
                        Score DECIMAL(5,2) NOT NULL,
                        MaxScore DECIMAL(5,2) NOT NULL,
                        Percentage DECIMAL(5,2) NOT NULL,
                        Comments NVARCHAR(255),
                        DateRecorded DATETIME2 DEFAULT GETDATE(),
                        DueDate DATE NOT NULL,
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id)
                    );
                END

                -- Insert default admin user with pre-hashed password
                IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Role, CreatedDate, IsActive)
                    VALUES ('admin', @adminPwdHash, @adminPwdSalt, 'System', 'Administrator', 'admin@school.com', 1, GETDATE(), 1);
                END
            ";

            try
            {
                using var connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                await connection.OpenAsync();
                
                using var command = new SqlCommand(createDbScript, connection);
                await command.ExecuteNonQueryAsync();
                
                command.CommandText = createTablesScript;
                command.Parameters.AddWithValue("@adminPwdHash", adminPwdHash);
                command.Parameters.AddWithValue("@adminPwdSalt", adminPwdSalt);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}");
            }
        }
    }
}
