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
                        Date DATETIME2 NOT NULL,
                        Status INT NOT NULL, -- 1=Present, 2=Absent, 3=Late, 4=Excused
                        Notes NVARCHAR(255),
                        RecordedDate DATETIME2 DEFAULT GETDATE(),
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id)
                    );
                END
                ELSE
                BEGIN
                    -- Migrate existing Attendance table Date column from DATE to DATETIME2
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Attendance') AND name = 'Date' AND system_type_id = 40)
                    BEGIN
                        -- Convert DATE to DATETIME2, preserving existing dates but adding current time
                        ALTER TABLE Attendance ALTER COLUMN Date DATETIME2 NOT NULL;
                    END
                END

                -- Grades table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Grades' AND xtype='U')
                BEGIN
                    CREATE TABLE Grades (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StudentId INT NOT NULL,
                        ProfessorId INT NOT NULL,
                        Subject NVARCHAR(100) NOT NULL,
                        Quarter NVARCHAR(20) NOT NULL, -- Prelim, Midterm, PreFinal, Final
                        ComponentType NVARCHAR(50) NOT NULL, -- QuizzesActivities, PerformanceTask, Exam
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
                
                // Run migration for existing Grades table separately
                await MigrateGradesTableAsync(connection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}");
            }
        }

        private static async Task MigrateGradesTableAsync(SqlConnection connection)
        {
            try
            {
                // Ensure we're using the correct database
                using var useDbCommand = new SqlCommand("USE StudentReportDB", connection);
                await useDbCommand.ExecuteNonQueryAsync();
                
                // Check if Grades table exists
                var checkTableQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Grades'";
                
                using var checkCommand = new SqlCommand(checkTableQuery, connection);
                var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (!tableExists) return; // Table doesn't exist, no migration needed
                
                // Check if Quarter column exists
                var checkQuarterQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Grades' AND COLUMN_NAME = 'Quarter'";
                
                using var checkQuarterCommand = new SqlCommand(checkQuarterQuery, connection);
                var quarterExists = Convert.ToInt32(await checkQuarterCommand.ExecuteScalarAsync()) > 0;
                
                // Check if ComponentType column exists
                var checkComponentQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Grades' AND COLUMN_NAME = 'ComponentType'";
                
                using var checkComponentCommand = new SqlCommand(checkComponentQuery, connection);
                var componentExists = Convert.ToInt32(await checkComponentCommand.ExecuteScalarAsync()) > 0;
                
                // Add Quarter column if it doesn't exist
                if (!quarterExists)
                {
                    var addQuarterQuery = "ALTER TABLE StudentReportDB.dbo.Grades ADD Quarter NVARCHAR(20) NULL";
                    using var addQuarterCommand = new SqlCommand(addQuarterQuery, connection);
                    await addQuarterCommand.ExecuteNonQueryAsync();
                }
                
                // Add ComponentType column if it doesn't exist
                if (!componentExists)
                {
                    var addComponentQuery = "ALTER TABLE StudentReportDB.dbo.Grades ADD ComponentType NVARCHAR(50) NULL";
                    using var addComponentCommand = new SqlCommand(addComponentQuery, connection);
                    await addComponentCommand.ExecuteNonQueryAsync();
                }
                
                // Migrate existing data (only if AssignmentType column exists)
                var checkAssignmentTypeQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Grades' AND COLUMN_NAME = 'AssignmentType'";
                
                using var checkAssignmentTypeCommand = new SqlCommand(checkAssignmentTypeQuery, connection);
                var assignmentTypeExists = Convert.ToInt32(await checkAssignmentTypeCommand.ExecuteScalarAsync()) > 0;
                
                if (assignmentTypeExists)
                {
                    var updateQuery = @"
                        UPDATE StudentReportDB.dbo.Grades 
                        SET Quarter = ISNULL(Quarter, 'Prelim'),
                            ComponentType = ISNULL(ComponentType, 
                                CASE 
                                    WHEN AssignmentType IN ('Quiz', 'Quizzes', 'Activity', 'Activities') THEN 'QuizzesActivities'
                                    WHEN AssignmentType IN ('Project', 'Performance', 'Performance Task') THEN 'PerformanceTask'
                                    WHEN AssignmentType IN ('Exam', 'Test', 'Final Exam') THEN 'Exam'
                                    ELSE 'QuizzesActivities'
                                END)
                        WHERE Quarter IS NULL OR ComponentType IS NULL";
                    
                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    await updateCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    // If AssignmentType doesn't exist, just set defaults
                    var updateQuery = @"
                        UPDATE StudentReportDB.dbo.Grades 
                        SET Quarter = ISNULL(Quarter, 'Prelim'),
                            ComponentType = ISNULL(ComponentType, 'QuizzesActivities')
                        WHERE Quarter IS NULL OR ComponentType IS NULL";
                    
                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    await updateCommand.ExecuteNonQueryAsync();
                }
                
                // Make columns NOT NULL after ensuring all rows have values
                var makeNotNullQuery = @"
                    USE StudentReportDB;
                    IF NOT EXISTS (SELECT * FROM Grades WHERE Quarter IS NULL)
                    BEGIN
                        ALTER TABLE Grades ALTER COLUMN Quarter NVARCHAR(20) NOT NULL;
                    END
                    
                    IF NOT EXISTS (SELECT * FROM Grades WHERE ComponentType IS NULL)
                    BEGIN
                        ALTER TABLE Grades ALTER COLUMN ComponentType NVARCHAR(50) NOT NULL;
                    END";
                
                using var makeNotNullCommand = new SqlCommand(makeNotNullQuery, connection);
                await makeNotNullCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Log migration error but don't fail initialization
                System.Diagnostics.Debug.WriteLine($"Grades table migration warning: {ex.Message}");
            }
        }
    }
}
