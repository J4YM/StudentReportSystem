using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;

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

                -- Branches table (must be created first as other tables reference it)
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Branches' AND xtype='U')
                BEGIN
                    CREATE TABLE Branches (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Code NVARCHAR(20) UNIQUE NOT NULL,
                        Address NVARCHAR(255),
                        IsActive BIT DEFAULT 1
                    );
                    
                    -- Insert all STI branches by region
                    INSERT INTO Branches (Name, Code, Address, IsActive) VALUES
                    -- Metro Manila
                    ('STI College Ortigas-Cainta', 'ORTIGAS', 'Ortigas Avenue Extension, Cainta, Rizal', 1),
                    ('STI College Alabang', 'ALABANG', 'Alabang, Muntinlupa City', 1),
                    ('STI College Caloocan', 'CALOOCAN', 'Caloocan City', 1),
                    ('STI College Global City', 'GLOBAL', 'Taguig City', 1),
                    ('STI College Las Piñas', 'LASPINAS', 'Las Piñas City', 1),
                    ('STI College Makati', 'MAKATI', 'Makati City', 1),
                    ('STI College Marikina', 'MARIKINA', 'Marikina City', 1),
                    ('STI College Muñoz', 'MUNOZ', 'Muñoz, Quezon City', 1),
                    ('STI College Novaliches', 'NOVALICHES', 'Novaliches, Quezon City', 1),
                    ('STI College Parañaque', 'PARANAQUE', 'Parañaque City', 1),
                    ('STI College Pasay', 'PASAY', 'Pasay City', 1),
                    ('STI College Pasig', 'PASIG', 'Pasig City', 1),
                    ('STI College Quezon Avenue', 'QUEZONAVE', 'Quezon Avenue, Quezon City', 1),
                    ('STI College Recto', 'RECTO', 'Recto Avenue, Manila', 1),
                    ('STI College Shaw', 'SHAW', 'Shaw Boulevard, Mandaluyong', 1),
                    ('STI College Taft', 'TAFT', 'Taft Avenue, Manila', 1),
                    ('STI College Valenzuela', 'VALENZUELA', 'Valenzuela City', 1),
                    -- Luzon (North)
                    ('STI College Baliuag', 'BALIUAG', 'Baliuag, Bulacan', 1),
                    ('STI College Calamba', 'CALAMBA', 'Calamba, Laguna', 1),
                    ('STI College Cubao', 'CUBAO', 'Cubao, Quezon City', 1),
                    ('STI College Fairview', 'FAIRVIEW', 'Fairview, Quezon City', 1),
                    ('STI College Las Piñas - Zapote', 'ZAPOTE', 'Zapote, Las Piñas', 1),
                    ('STI College Malolos', 'MALOLOS', 'Malolos, Bulacan', 1),
                    ('STI College Meycauayan', 'MEYCAUAYAN', 'Meycauayan, Bulacan', 1),
                    ('STI College San Fernando', 'SANFERNANDO', 'San Fernando, Pampanga', 1),
                    ('STI College Sta. Maria', 'STAMARIA', 'Sta. Maria, Bulacan', 1),
                    -- Luzon (South)
                    ('STI College Bacoor', 'BACOOR', 'Bacoor, Cavite', 1),
                    ('STI College Batangas', 'BATANGAS', 'Batangas City', 1),
                    ('STI College Dasmariñas', 'DASMARINAS', 'Dasmariñas, Cavite', 1),
                    ('STI College Lucena', 'LUCENA', 'Lucena City', 1),
                    ('STI College San Pablo', 'SANPABLO', 'San Pablo, Laguna', 1),
                    ('STI College Sta. Rosa', 'STAROSA', 'Sta. Rosa, Laguna', 1),
                    -- Visayas
                    ('STI College Bacolod', 'BACOLOD', 'Bacolod City', 1),
                    ('STI College Cebu', 'CEBU', 'Cebu City', 1),
                    ('STI College Iloilo', 'ILOILO', 'Iloilo City', 1),
                    ('STI College Tacloban', 'TACLOBAN', 'Tacloban City', 1),
                    -- Mindanao
                    ('STI College Butuan', 'BUTUAN', 'Butuan City', 1),
                    ('STI College Cagayan de Oro', 'CDO', 'Cagayan de Oro City', 1),
                    ('STI College Davao', 'DAVAO', 'Davao City', 1),
                    ('STI College General Santos', 'GENSAN', 'General Santos City', 1),
                    ('STI College Iligan', 'ILIGAN', 'Iligan City', 1),
                    ('STI College Koronadal', 'KORONADAL', 'Koronadal City', 1),
                    ('STI College Marbel', 'MARBEL', 'Koronadal, South Cotabato', 1);
                END

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
                        Role INT NOT NULL, -- 0=SuperAdmin, 1=Admin, 2=Professor, 3=Guardian, 4=Student
                        BranchId INT NULL,
                        CreatedDate DATETIME2 DEFAULT GETDATE(),
                        IsActive BIT DEFAULT 1,
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id)
                    );
                END

                -- AdminRequests table to capture branch admin escalation
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AdminRequests' AND xtype='U')
                BEGIN
                    CREATE TABLE AdminRequests (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        RequestedByUserId INT NOT NULL,
                        BranchId INT NULL,
                        RequestedRole INT NOT NULL,
                        FullName NVARCHAR(200) NOT NULL,
                        Email NVARCHAR(150) NOT NULL,
                        Reason NVARCHAR(500),
                        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                        CreatedDate DATETIME2 DEFAULT GETDATE(),
                        ReviewedByUserId INT NULL,
                        ReviewedDate DATETIME2 NULL,
                        Notes NVARCHAR(500) NULL,
                        FOREIGN KEY (RequestedByUserId) REFERENCES Users(Id),
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id)
                    );
                END

                -- Security audit log for privileged actions
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SecurityAuditLogs' AND xtype='U')
                BEGIN
                    CREATE TABLE SecurityAuditLogs (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        Action NVARCHAR(150) NOT NULL,
                        Details NVARCHAR(500),
                        VerificationMethod NVARCHAR(100) NOT NULL,
                        LoggedDate DATETIME2 DEFAULT GETDATE(),
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
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
                        GuardianId INT NULL, -- Made nullable, primary guardian stored here for backward compatibility
                        BranchId INT NOT NULL,
                        EnrollmentDate DATE DEFAULT GETDATE(),
                        IsActive BIT DEFAULT 1,
                        Username NVARCHAR(50) UNIQUE NOT NULL,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        PasswordSalt NVARCHAR(255) NOT NULL,
                        FOREIGN KEY (GuardianId) REFERENCES Users(Id),
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id)
                    );
                END

                -- Subjects table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Subjects' AND xtype='U')
                BEGIN
                    CREATE TABLE Subjects (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Code NVARCHAR(20) NOT NULL,
                        Description NVARCHAR(255),
                        ProfessorId INT NULL,  -- Made nullable to support unclaimed subjects
                        GradeLevel NVARCHAR(20) NOT NULL,
                        Section NVARCHAR(20) NOT NULL,
                        BranchId INT NOT NULL,
                        IsActive BIT DEFAULT 1,
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id),
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id),
                        UNIQUE(Code, BranchId)
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
                        BranchId INT NOT NULL,
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id),
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id)
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
                        BranchId INT NOT NULL,
                        FOREIGN KEY (StudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ProfessorId) REFERENCES Users(Id),
                        FOREIGN KEY (BranchId) REFERENCES Branches(Id)
                    );
                END

                -- Insert default admin user with pre-hashed password (Super Admin)
                IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Role, BranchId, CreatedDate, IsActive)
                    VALUES ('admin', @adminPwdHash, @adminPwdSalt, 'System', 'Administrator', 'admin@school.com', 0, NULL, GETDATE(), 1);
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
                
                // Run migration for branch system
                await MigrateBranchSystemAsync(connection);
                
                // Run migration for GuardianStudents junction table
                await MigrateGuardianStudentsTableAsync(connection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}");
            }
        }

        private static async Task MigrateBranchSystemAsync(SqlConnection connection)
        {
            try
            {
                using var useDbCommand = new SqlCommand("USE StudentReportDB", connection);
                await useDbCommand.ExecuteNonQueryAsync();
                
                System.Diagnostics.Debug.WriteLine("Starting branch system migration...");

                // Ensure Branches table exists (should already exist from initial creation, but check for migration)
                var checkBranchesTable = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Branches'";
                
                using var checkBranchesCommand = new SqlCommand(checkBranchesTable, connection);
                var branchesTableExists = Convert.ToInt32(await checkBranchesCommand.ExecuteScalarAsync()) > 0;
                
                if (!branchesTableExists)
                {
                    var createBranchesTable = @"
                        CREATE TABLE Branches (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL,
                            Code NVARCHAR(20) UNIQUE NOT NULL,
                            Address NVARCHAR(255),
                            IsActive BIT DEFAULT 1
                        );";
                    
                    using var createBranchesCommand = new SqlCommand(createBranchesTable, connection);
                    await createBranchesCommand.ExecuteNonQueryAsync();
                }
                
                // Populate all STI branches (add missing ones, don't duplicate)
                var allBranches = new List<(string Name, string Code, string Address)>
                {
                    // Metro Manila
                    ("STI College Ortigas-Cainta", "ORTIGAS", "Ortigas Avenue Extension, Cainta, Rizal"),
                    ("STI College Alabang", "ALABANG", "Alabang, Muntinlupa City"),
                    ("STI College Caloocan", "CALOOCAN", "Caloocan City"),
                    ("STI College Global City", "GLOBAL", "Taguig City"),
                    ("STI College Las Piñas", "LASPINAS", "Las Piñas City"),
                    ("STI College Makati", "MAKATI", "Makati City"),
                    ("STI College Marikina", "MARIKINA", "Marikina City"),
                    ("STI College Muñoz", "MUNOZ", "Muñoz, Quezon City"),
                    ("STI College Novaliches", "NOVALICHES", "Novaliches, Quezon City"),
                    ("STI College Parañaque", "PARANAQUE", "Parañaque City"),
                    ("STI College Pasay", "PASAY", "Pasay City"),
                    ("STI College Pasig", "PASIG", "Pasig City"),
                    ("STI College Quezon Avenue", "QUEZONAVE", "Quezon Avenue, Quezon City"),
                    ("STI College Recto", "RECTO", "Recto Avenue, Manila"),
                    ("STI College Shaw", "SHAW", "Shaw Boulevard, Mandaluyong"),
                    ("STI College Taft", "TAFT", "Taft Avenue, Manila"),
                    ("STI College Valenzuela", "VALENZUELA", "Valenzuela City"),
                    // Luzon (North)
                    ("STI College Baliuag", "BALIUAG", "Baliuag, Bulacan"),
                    ("STI College Calamba", "CALAMBA", "Calamba, Laguna"),
                    ("STI College Cubao", "CUBAO", "Cubao, Quezon City"),
                    ("STI College Fairview", "FAIRVIEW", "Fairview, Quezon City"),
                    ("STI College Las Piñas - Zapote", "ZAPOTE", "Zapote, Las Piñas"),
                    ("STI College Malolos", "MALOLOS", "Malolos, Bulacan"),
                    ("STI College Meycauayan", "MEYCAUAYAN", "Meycauayan, Bulacan"),
                    ("STI College San Fernando", "SANFERNANDO", "San Fernando, Pampanga"),
                    ("STI College Sta. Maria", "STAMARIA", "Sta. Maria, Bulacan"),
                    // Luzon (South)
                    ("STI College Bacoor", "BACOOR", "Bacoor, Cavite"),
                    ("STI College Batangas", "BATANGAS", "Batangas City"),
                    ("STI College Dasmariñas", "DASMARINAS", "Dasmariñas, Cavite"),
                    ("STI College Lucena", "LUCENA", "Lucena City"),
                    ("STI College San Pablo", "SANPABLO", "San Pablo, Laguna"),
                    ("STI College Sta. Rosa", "STAROSA", "Sta. Rosa, Laguna"),
                    // Visayas
                    ("STI College Bacolod", "BACOLOD", "Bacolod City"),
                    ("STI College Cebu", "CEBU", "Cebu City"),
                    ("STI College Iloilo", "ILOILO", "Iloilo City"),
                    ("STI College Tacloban", "TACLOBAN", "Tacloban City"),
                    // Mindanao
                    ("STI College Butuan", "BUTUAN", "Butuan City"),
                    ("STI College Cagayan de Oro", "CDO", "Cagayan de Oro City"),
                    ("STI College Davao", "DAVAO", "Davao City"),
                    ("STI College General Santos", "GENSAN", "General Santos City"),
                    ("STI College Iligan", "ILIGAN", "Iligan City"),
                    ("STI College Koronadal", "KORONADAL", "Koronadal City"),
                    ("STI College Marbel", "MARBEL", "Koronadal, South Cotabato")
                };

                // Insert branches that don't exist (check by Code to avoid duplicates)
                foreach (var branch in allBranches)
                {
                    var checkBranchQuery = "SELECT COUNT(*) FROM Branches WHERE Code = @code";
                    using var checkCommand = new SqlCommand(checkBranchQuery, connection);
                    checkCommand.Parameters.AddWithValue("@code", branch.Code);
                    var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                    
                    if (!exists)
                    {
                        var insertQuery = "INSERT INTO Branches (Name, Code, Address, IsActive) VALUES (@name, @code, @address, 1)";
                        using var insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@name", branch.Name);
                        insertCommand.Parameters.AddWithValue("@code", branch.Code);
                        insertCommand.Parameters.AddWithValue("@address", branch.Address);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }

                // Get default branch ID (Baliuag)
                var getDefaultBranchQuery = "SELECT TOP 1 Id FROM Branches WHERE Code = 'BALIUAG'";
                int defaultBranchId = 1;
                using var getBranchCommand = new SqlCommand(getDefaultBranchQuery, connection);
                var branchResult = await getBranchCommand.ExecuteScalarAsync();
                if (branchResult != null)
                {
                    defaultBranchId = Convert.ToInt32(branchResult);
                }

                // Add BranchId column to Users table if it doesn't exist
                var checkUsersBranchId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'BranchId'";
                
                using var checkUsersCommand = new SqlCommand(checkUsersBranchId, connection);
                var usersBranchIdExists = Convert.ToInt32(await checkUsersCommand.ExecuteScalarAsync()) > 0;
                
                if (!usersBranchIdExists)
                {
                    var addUsersBranchId = "ALTER TABLE Users ADD BranchId INT NULL";
                    using var addUsersCommand = new SqlCommand(addUsersBranchId, connection);
                    await addUsersCommand.ExecuteNonQueryAsync();
                    
                    // Set default branch for existing admins (non-super admin)
                    var updateUsersBranch = @"
                        UPDATE Users 
                        SET BranchId = @defaultBranchId 
                        WHERE Role = 1 AND BranchId IS NULL";
                    using var updateUsersCommand = new SqlCommand(updateUsersBranch, connection);
                    updateUsersCommand.Parameters.AddWithValue("@defaultBranchId", defaultBranchId);
                    await updateUsersCommand.ExecuteNonQueryAsync();
                }

                // Add BranchId column to Students table if it doesn't exist
                var checkStudentsBranchId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Students' AND COLUMN_NAME = 'BranchId'";
                
                using var checkStudentsCommand = new SqlCommand(checkStudentsBranchId, connection);
                var studentsBranchIdExists = Convert.ToInt32(await checkStudentsCommand.ExecuteScalarAsync()) > 0;
                
                if (!studentsBranchIdExists)
                {
                    var addStudentsBranchId = "ALTER TABLE Students ADD BranchId INT NULL";
                    using var addStudentsCommand = new SqlCommand(addStudentsBranchId, connection);
                    await addStudentsCommand.ExecuteNonQueryAsync();
                    
                    // Set default branch for existing students
                    var updateStudentsBranch = @"
                        UPDATE Students 
                        SET BranchId = @defaultBranchId 
                        WHERE BranchId IS NULL";
                    using var updateStudentsCommand = new SqlCommand(updateStudentsBranch, connection);
                    updateStudentsCommand.Parameters.AddWithValue("@defaultBranchId", defaultBranchId);
                    await updateStudentsCommand.ExecuteNonQueryAsync();
                    
                    // Make BranchId NOT NULL after populating it
                    var makeStudentsBranchIdNotNull = "ALTER TABLE Students ALTER COLUMN BranchId INT NOT NULL";
                    using var makeNotNullCommand = new SqlCommand(makeStudentsBranchIdNotNull, connection);
                    await makeNotNullCommand.ExecuteNonQueryAsync();
                }

                // Add BranchId column to Subjects table if it doesn't exist
                var checkSubjectsBranchId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Subjects' AND COLUMN_NAME = 'BranchId'";
                
                using var checkSubjectsCommand = new SqlCommand(checkSubjectsBranchId, connection);
                var subjectsBranchIdExists = Convert.ToInt32(await checkSubjectsCommand.ExecuteScalarAsync()) > 0;
                
                if (!subjectsBranchIdExists)
                {
                    var addSubjectsBranchId = "ALTER TABLE Subjects ADD BranchId INT NULL";
                    using var addSubjectsCommand = new SqlCommand(addSubjectsBranchId, connection);
                    await addSubjectsCommand.ExecuteNonQueryAsync();
                    
                    // Set default branch for existing subjects
                    var updateSubjectsBranch = @"
                        UPDATE Subjects 
                        SET BranchId = @defaultBranchId 
                        WHERE BranchId IS NULL";
                    using var updateSubjectsCommand = new SqlCommand(updateSubjectsBranch, connection);
                    updateSubjectsCommand.Parameters.AddWithValue("@defaultBranchId", defaultBranchId);
                    await updateSubjectsCommand.ExecuteNonQueryAsync();
                    
                    // Make BranchId NOT NULL after populating it
                    var makeSubjectsBranchIdNotNull = "ALTER TABLE Subjects ALTER COLUMN BranchId INT NOT NULL";
                    using var makeNotNullCommand = new SqlCommand(makeSubjectsBranchIdNotNull, connection);
                    await makeNotNullCommand.ExecuteNonQueryAsync();
                }

                // Add BranchId to Grades table (via StudentId join)
                var checkGradesBranchId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Grades' AND COLUMN_NAME = 'BranchId'";
                
                using var checkGradesCommand = new SqlCommand(checkGradesBranchId, connection);
                var gradesBranchIdExists = Convert.ToInt32(await checkGradesCommand.ExecuteScalarAsync()) > 0;
                
                if (!gradesBranchIdExists)
                {
                    var addGradesBranchId = "ALTER TABLE Grades ADD BranchId INT NULL";
                    using var addGradesCommand = new SqlCommand(addGradesBranchId, connection);
                    await addGradesCommand.ExecuteNonQueryAsync();
                    
                    // Set branch for existing grades based on student's branch
                    var updateGradesBranch = @"
                        UPDATE g
                        SET g.BranchId = s.BranchId
                        FROM Grades g
                        INNER JOIN Students s ON g.StudentId = s.Id
                        WHERE g.BranchId IS NULL AND s.BranchId IS NOT NULL";
                    using var updateGradesCommand = new SqlCommand(updateGradesBranch, connection);
                    await updateGradesCommand.ExecuteNonQueryAsync();
                    
                    // Set default branch for any remaining NULLs (in case student doesn't have branch)
                    var updateRemainingNulls = @"
                        UPDATE Grades 
                        SET BranchId = @defaultBranchId 
                        WHERE BranchId IS NULL";
                    using var updateRemainingCommand = new SqlCommand(updateRemainingNulls, connection);
                    updateRemainingCommand.Parameters.AddWithValue("@defaultBranchId", defaultBranchId);
                    await updateRemainingCommand.ExecuteNonQueryAsync();
                    
                    // Make BranchId NOT NULL after populating it
                    var makeGradesBranchIdNotNull = "ALTER TABLE Grades ALTER COLUMN BranchId INT NOT NULL";
                    using var makeNotNullCommand = new SqlCommand(makeGradesBranchIdNotNull, connection);
                    await makeNotNullCommand.ExecuteNonQueryAsync();
                    
                    // Add foreign key constraint if it doesn't exist
                    var checkFkExists = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                        WHERE CONSTRAINT_NAME = 'FK_Grades_Branches' AND TABLE_NAME = 'Grades'";
                    using var checkFkCommand = new SqlCommand(checkFkExists, connection);
                    var fkExists = Convert.ToInt32(await checkFkCommand.ExecuteScalarAsync()) > 0;
                    
                    if (!fkExists)
                    {
                        var addGradesFk = "ALTER TABLE Grades ADD CONSTRAINT FK_Grades_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id)";
                        using var addFkCommand = new SqlCommand(addGradesFk, connection);
                        await addFkCommand.ExecuteNonQueryAsync();
                    }
                }

                // Add BranchId to Attendance table (via StudentId join)
                var checkAttendanceBranchId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'BranchId'";
                
                using var checkAttendanceCommand = new SqlCommand(checkAttendanceBranchId, connection);
                var attendanceBranchIdExists = Convert.ToInt32(await checkAttendanceCommand.ExecuteScalarAsync()) > 0;
                
                if (!attendanceBranchIdExists)
                {
                    var addAttendanceBranchId = "ALTER TABLE Attendance ADD BranchId INT NULL";
                    using var addAttendanceCommand = new SqlCommand(addAttendanceBranchId, connection);
                    await addAttendanceCommand.ExecuteNonQueryAsync();
                    
                    // Set branch for existing attendance based on student's branch
                    var updateAttendanceBranch = @"
                        UPDATE a
                        SET a.BranchId = s.BranchId
                        FROM Attendance a
                        INNER JOIN Students s ON a.StudentId = s.Id
                        WHERE a.BranchId IS NULL AND s.BranchId IS NOT NULL";
                    using var updateAttendanceCommand = new SqlCommand(updateAttendanceBranch, connection);
                    await updateAttendanceCommand.ExecuteNonQueryAsync();
                    
                    // Set default branch for any remaining NULLs (in case student doesn't have branch)
                    var updateRemainingNulls = @"
                        UPDATE Attendance 
                        SET BranchId = @defaultBranchId 
                        WHERE BranchId IS NULL";
                    using var updateRemainingCommand = new SqlCommand(updateRemainingNulls, connection);
                    updateRemainingCommand.Parameters.AddWithValue("@defaultBranchId", defaultBranchId);
                    await updateRemainingCommand.ExecuteNonQueryAsync();
                    
                    // Make BranchId NOT NULL after populating it
                    var makeAttendanceBranchIdNotNull = "ALTER TABLE Attendance ALTER COLUMN BranchId INT NOT NULL";
                    using var makeNotNullCommand = new SqlCommand(makeAttendanceBranchIdNotNull, connection);
                    await makeNotNullCommand.ExecuteNonQueryAsync();
                    
                    // Add foreign key constraint if it doesn't exist
                    var checkFkExists = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                        WHERE CONSTRAINT_NAME = 'FK_Attendance_Branches' AND TABLE_NAME = 'Attendance'";
                    using var checkFkCommand = new SqlCommand(checkFkExists, connection);
                    var fkExists = Convert.ToInt32(await checkFkCommand.ExecuteScalarAsync()) > 0;
                    
                    if (!fkExists)
                    {
                        var addAttendanceFk = "ALTER TABLE Attendance ADD CONSTRAINT FK_Attendance_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id)";
                        using var addFkCommand = new SqlCommand(addAttendanceFk, connection);
                        await addFkCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                System.Diagnostics.Debug.WriteLine($"Branch system migration error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Re-throw the exception so it's not silently ignored
                throw new Exception($"Branch system migration failed: {ex.Message}", ex);
            }
        }

        private static async Task MigrateGuardianStudentsTableAsync(SqlConnection connection)
        {
            try
            {
                using var useDbCommand = new SqlCommand("USE StudentReportDB", connection);
                await useDbCommand.ExecuteNonQueryAsync();

                // Check if GuardianStudents table exists
                var checkTableQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GuardianStudents'";
                
                using var checkCommand = new SqlCommand(checkTableQuery, connection);
                var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (!tableExists)
                {
                    // Create GuardianStudents junction table
                    var createTableQuery = @"
                        CREATE TABLE GuardianStudents (
                            GuardianId INT NOT NULL,
                            StudentId INT NOT NULL,
                            Relationship NVARCHAR(50) DEFAULT 'Parent',
                            CreatedDate DATETIME2 DEFAULT GETDATE(),
                            PRIMARY KEY (GuardianId, StudentId),
                            FOREIGN KEY (GuardianId) REFERENCES Users(Id),
                            FOREIGN KEY (StudentId) REFERENCES Students(Id)
                        )";
                    
                    using var createCommand = new SqlCommand(createTableQuery, connection);
                    await createCommand.ExecuteNonQueryAsync();
                    
                    // Migrate existing GuardianId relationships from Students table
                    var migrateQuery = @"
                        INSERT INTO GuardianStudents (GuardianId, StudentId, Relationship)
                        SELECT GuardianId, Id, 'Parent'
                        FROM Students
                        WHERE GuardianId IS NOT NULL
                        AND NOT EXISTS (
                            SELECT 1 FROM GuardianStudents 
                            WHERE GuardianId = Students.GuardianId 
                            AND StudentId = Students.Id
                        )";
                    
                    using var migrateCommand = new SqlCommand(migrateQuery, connection);
                    await migrateCommand.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GuardianStudents migration error: {ex.Message}");
                throw new Exception($"GuardianStudents migration failed: {ex.Message}", ex);
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
