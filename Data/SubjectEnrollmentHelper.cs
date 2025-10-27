using StudentReportInitial.Models;
using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    public static class SubjectEnrollmentHelper
    {
        public static async Task<bool> ClaimSubjectAsync(int subjectId, int professorId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Verify the user is a professor
                var verifyQuery = @"
                    SELECT Role FROM Users 
                    WHERE Id = @professorId AND Role = @professorRole AND IsActive = 1";

                using var verifyCommand = new SqlCommand(verifyQuery, connection, transaction);
                verifyCommand.Parameters.AddWithValue("@professorId", professorId);
                verifyCommand.Parameters.AddWithValue("@professorRole", 2); // Professor role
                
                var result = await verifyCommand.ExecuteScalarAsync();
                if (result == null)
                {
                    throw new InvalidOperationException("User is not an active professor");
                }

                // Check if subject is already claimed
                var checkQuery = "SELECT ProfessorId FROM Subjects WHERE Id = @subjectId";
                using var checkCommand = new SqlCommand(checkQuery, connection, transaction);
                checkCommand.Parameters.AddWithValue("@subjectId", subjectId);
                var currentProfessorId = await checkCommand.ExecuteScalarAsync();

                if (currentProfessorId != null && currentProfessorId != DBNull.Value)
                {
                    throw new InvalidOperationException("Subject is already claimed by another professor.");
                }

                // Update the subject with the professor ID
                var updateQuery = "UPDATE Subjects SET ProfessorId = @professorId WHERE Id = @subjectId";
                using var updateCommand = new SqlCommand(updateQuery, connection, transaction);
                updateCommand.Parameters.AddWithValue("@subjectId", subjectId);
                updateCommand.Parameters.AddWithValue("@professorId", professorId);
                await updateCommand.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task<bool> UnclaimSubjectAsync(int subjectId, int professorId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if subject is claimed by this professor
                var checkQuery = "SELECT ProfessorId FROM Subjects WHERE Id = @subjectId";
                using var checkCommand = new SqlCommand(checkQuery, connection, transaction);
                checkCommand.Parameters.AddWithValue("@subjectId", subjectId);
                var currentProfessorId = await checkCommand.ExecuteScalarAsync();

                if (currentProfessorId == null || currentProfessorId == DBNull.Value)
                {
                    throw new InvalidOperationException("Subject is not claimed by any professor.");
                }

                if (Convert.ToInt32(currentProfessorId) != professorId)
                {
                    throw new InvalidOperationException("Subject is claimed by another professor.");
                }

                // Remove the professor ID from the subject
                var updateQuery = "UPDATE Subjects SET ProfessorId = NULL WHERE Id = @subjectId";
                using var updateCommand = new SqlCommand(updateQuery, connection, transaction);
                updateCommand.Parameters.AddWithValue("@subjectId", subjectId);
                await updateCommand.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task<bool> EnrollStudentInSubjectAsync(int studentId, int subjectId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if student exists and is active
                var checkStudentQuery = "SELECT IsActive FROM Students WHERE Id = @studentId";
                using var checkStudentCommand = new SqlCommand(checkStudentQuery, connection, transaction);
                checkStudentCommand.Parameters.AddWithValue("@studentId", studentId);
                var isStudentActive = await checkStudentCommand.ExecuteScalarAsync();

                if (isStudentActive == null || !(bool)isStudentActive)
                {
                    throw new InvalidOperationException("Student does not exist or is inactive.");
                }

                // Check if the subject exists and is active
                var checkSubjectQuery = "SELECT IsActive FROM Subjects WHERE Id = @subjectId";
                using var checkSubjectCommand = new SqlCommand(checkSubjectQuery, connection, transaction);
                checkSubjectCommand.Parameters.AddWithValue("@subjectId", subjectId);
                var isSubjectActive = await checkSubjectCommand.ExecuteScalarAsync();

                if (isSubjectActive == null || !(bool)isSubjectActive)
                {
                    throw new InvalidOperationException("Subject does not exist or is inactive.");
                }

                // Check if student is already enrolled
                var checkEnrollmentQuery = "SELECT COUNT(*) FROM StudentSubjects WHERE StudentId = @studentId AND SubjectId = @subjectId";
                using var checkEnrollmentCommand = new SqlCommand(checkEnrollmentQuery, connection, transaction);
                checkEnrollmentCommand.Parameters.AddWithValue("@studentId", studentId);
                checkEnrollmentCommand.Parameters.AddWithValue("@subjectId", subjectId);
                var enrollmentExists = Convert.ToInt32(await checkEnrollmentCommand.ExecuteScalarAsync()) > 0;

                if (enrollmentExists)
                {
                    throw new InvalidOperationException("Student is already enrolled in this subject.");
                }

                // Enroll the student
                var enrollQuery = @"
                    INSERT INTO StudentSubjects (StudentId, SubjectId, EnrollmentDate)
                    VALUES (@studentId, @subjectId, @enrollmentDate)";
                using var enrollCommand = new SqlCommand(enrollQuery, connection, transaction);
                enrollCommand.Parameters.AddWithValue("@studentId", studentId);
                enrollCommand.Parameters.AddWithValue("@subjectId", subjectId);
                enrollCommand.Parameters.AddWithValue("@enrollmentDate", DateTime.Now);
                await enrollCommand.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task<bool> UnenrollStudentFromSubjectAsync(int studentId, int subjectId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if student is enrolled
                var checkEnrollmentQuery = "SELECT COUNT(*) FROM StudentSubjects WHERE StudentId = @studentId AND SubjectId = @subjectId";
                using var checkEnrollmentCommand = new SqlCommand(checkEnrollmentQuery, connection, transaction);
                checkEnrollmentCommand.Parameters.AddWithValue("@studentId", studentId);
                checkEnrollmentCommand.Parameters.AddWithValue("@subjectId", subjectId);
                var enrollmentExists = Convert.ToInt32(await checkEnrollmentCommand.ExecuteScalarAsync()) > 0;

                if (!enrollmentExists)
                {
                    throw new InvalidOperationException("Student is not enrolled in this subject.");
                }

                // Remove the enrollment record
                var unenrollQuery = "DELETE FROM StudentSubjects WHERE StudentId = @studentId AND SubjectId = @subjectId";
                using var unenrollCommand = new SqlCommand(unenrollQuery, connection, transaction);
                unenrollCommand.Parameters.AddWithValue("@studentId", studentId);
                unenrollCommand.Parameters.AddWithValue("@subjectId", subjectId);
                await unenrollCommand.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task<List<Subject>> GetUnclaimedSubjectsAsync(string? gradeLevel = null)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT s.Id, s.Name, s.Code, s.Description, s.GradeLevel, s.Section, s.IsActive
                FROM Subjects s
                WHERE s.ProfessorId IS NULL AND s.IsActive = 1";

            if (!string.IsNullOrEmpty(gradeLevel))
            {
                query += " AND s.GradeLevel = @gradeLevel";
            }

            query += " ORDER BY s.GradeLevel, s.Name";

            using var command = new SqlCommand(query, connection);
            if (!string.IsNullOrEmpty(gradeLevel))
            {
                command.Parameters.AddWithValue("@gradeLevel", gradeLevel);
            }

            var subjects = new List<Subject>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                subjects.Add(new Subject
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    GradeLevel = reader.GetString(reader.GetOrdinal("GradeLevel")),
                    Section = reader.GetString(reader.GetOrdinal("Section")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }

            return subjects;
        }

        public static async Task<List<Subject>> GetClaimedSubjectsAsync(int professorId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT s.Id, s.Name, s.Code, s.Description, s.GradeLevel, s.Section, s.IsActive,
                       COUNT(ss.StudentId) as EnrolledStudentCount
                FROM Subjects s
                LEFT JOIN StudentSubjects ss ON s.Id = ss.SubjectId
                WHERE s.ProfessorId = @professorId AND s.IsActive = 1
                GROUP BY s.Id, s.Name, s.Code, s.Description, s.GradeLevel, s.Section, s.IsActive
                ORDER BY s.GradeLevel, s.Name";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@professorId", professorId);

            var subjects = new List<Subject>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var subject = new Subject
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    GradeLevel = reader.GetString(reader.GetOrdinal("GradeLevel")),
                    Section = reader.GetString(reader.GetOrdinal("Section")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    ProfessorId = professorId
                };

                subjects.Add(subject);
            }

            return subjects;
        }

        public static async Task<List<Student>> GetEnrolledStudentsAsync(int subjectId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.Email, s.GradeLevel, s.Section,
                       ss.EnrollmentDate as SubjectEnrollmentDate
                FROM Students s
                INNER JOIN StudentSubjects ss ON s.Id = ss.StudentId
                WHERE ss.SubjectId = @subjectId AND s.IsActive = 1
                ORDER BY s.LastName, s.FirstName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@subjectId", subjectId);

            var students = new List<Student>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                    GradeLevel = reader.GetString(reader.GetOrdinal("GradeLevel")),
                    Section = reader.GetString(reader.GetOrdinal("Section"))
                });
            }

            return students;
        }

        public static async Task<List<Subject>> GetStudentSubjectsAsync(int studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT s.Id, s.Name, s.Code, s.Description, s.GradeLevel, s.Section, s.IsActive,
                       s.ProfessorId, ss.EnrollmentDate,
                       CONCAT(u.FirstName, ' ', u.LastName) as ProfessorName
                FROM Subjects s
                INNER JOIN StudentSubjects ss ON s.Id = ss.SubjectId
                LEFT JOIN Users u ON s.ProfessorId = u.Id
                WHERE ss.StudentId = @studentId AND s.IsActive = 1
                ORDER BY s.GradeLevel, s.Name";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", studentId);

            var subjects = new List<Subject>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var subject = new Subject
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    GradeLevel = reader.GetString(reader.GetOrdinal("GradeLevel")),
                    Section = reader.GetString(reader.GetOrdinal("Section")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    ProfessorId = reader.IsDBNull(reader.GetOrdinal("ProfessorId")) ? null : reader.GetInt32(reader.GetOrdinal("ProfessorId"))
                };

                if (!reader.IsDBNull(reader.GetOrdinal("ProfessorName")))
                {
                    var fullName = reader.GetString(reader.GetOrdinal("ProfessorName")).Split(' ');
                    subject.Professor = new User
                    {
                        FirstName = fullName[0],
                        LastName = string.Join(" ", fullName.Skip(1))
                    };
                }

                subjects.Add(subject);
            }

            return subjects;
        }
    }
}
