using StudentReportInitial.Models;
using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    public static class AccountHelper
    {
        public static string GenerateStudentId(SqlConnection connection, SqlTransaction transaction)
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var query = "SELECT COUNT(*) FROM Students WHERE YEAR(EnrollmentDate) = @year";
            
            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@year", DateTime.Now.Year);
            var count = Convert.ToInt32(command.ExecuteScalar()) + 1;
            
            return $"{year}-{count:D4}";  // Format: YY-0001
        }

        public static async Task<int> CreateStudentAccountAsync(SqlConnection connection, SqlTransaction transaction, Student student)
        {
            // Use branch-specific email generation
            var studentUsername = await EmailDomainHelper.GenerateStudentEmailAsync(student.StudentId, student.BranchId);
            var studentPassword = GenerateStudentPassword(student);
            var studentEmail = await EmailDomainHelper.GenerateStudentEmailAsync(student.StudentId, student.BranchId);

            // Hash the password
            PasswordHasher.CreatePasswordHash(studentPassword, out string passwordHash, out string passwordSalt);

            var query = @"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Phone, Role, BranchId, CreatedDate, IsActive)
                VALUES (@username, @passwordHash, @passwordSalt, @firstName, @lastName, @email, @phone, @role, @branchId, @createdDate, @isActive);
                SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@username", studentUsername);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@email", studentEmail);
            command.Parameters.AddWithValue("@phone", student.Phone ?? "");
            command.Parameters.AddWithValue("@role", (int)UserRole.Student);
            command.Parameters.AddWithValue("@branchId", student.BranchId);
            command.Parameters.AddWithValue("@createdDate", DateTime.Now);
            command.Parameters.AddWithValue("@isActive", true);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public static string GenerateStudentPassword(Student student)
        {
            // Format: studentnumber@LastName
            return $"{student.StudentId}@{student.LastName}";
        }

        public static string GetAccountCredentialsMessage(string username, string password)
        {
            return $"Account created successfully!\n\n" +
                   $"Username: {username}\n" +
                   $"Password: {password}\n\n" +
                   "Please keep these credentials safe.";
        }
    }
}
