using StudentReportInitial.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace StudentReportInitial.Data
{
    public static class UserContextHelper
    {
        /// <summary>
        /// Retrieves the student record tied to the supplied user if the role is Student or Guardian.
        /// </summary>
        public static async Task<Student?> GetLinkedStudentAsync(User user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.Role != UserRole.Student && user.Role != UserRole.Guardian)
            {
                return null;
            }

            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = user.Role == UserRole.Student
                    ? "SELECT TOP 1 * FROM Students WHERE Username = @identifier AND IsActive = 1"
                    : "SELECT TOP 1 * FROM Students WHERE GuardianId = @identifier AND IsActive = 1 ORDER BY Id"
            };

            if (user.Role == UserRole.Student)
            {
                command.Parameters.AddWithValue("@identifier", user.Username);
            }
            else
            {
                command.Parameters.AddWithValue("@identifier", user.Id);
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapStudent(reader);
            }

            return null;
        }

        private static Student MapStudent(SqlDataReader reader)
        {
            return new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = GetString(reader, "StudentId"),
                FirstName = GetString(reader, "FirstName"),
                LastName = GetString(reader, "LastName"),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Gender = GetString(reader, "Gender"),
                Address = GetString(reader, "Address"),
                Phone = GetString(reader, "Phone"),
                Email = GetString(reader, "Email"),
                GradeLevel = GetString(reader, "GradeLevel"),
                Section = GetString(reader, "Section"),
                GuardianId = reader.GetInt32(reader.GetOrdinal("GuardianId")),
                EnrollmentDate = reader.GetDateTime(reader.GetOrdinal("EnrollmentDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Username = GetString(reader, "Username"),
                PasswordHash = GetString(reader, "PasswordHash"),
                PasswordSalt = GetString(reader, "PasswordSalt")
            };
        }

        private static string GetString(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}

