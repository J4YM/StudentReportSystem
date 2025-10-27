using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    public static class AdminHelper
    {
        public static async Task CreateAdminUser(string username, string password, string firstName, string lastName, string email)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // First check if the user already exists
            var checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            using var checkCommand = new SqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@username", username);

            var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
            if (exists)
            {
                throw new Exception($"User with username '{username}' already exists.");
            }

            // Hash the password
            PasswordHasher.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

            // Insert the new admin user
            var query = @"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Role, CreatedDate, IsActive)
                VALUES (@username, @passwordHash, @passwordSalt, @firstName, @lastName, @email, 1, GETDATE(), 1)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@email", email);

            await command.ExecuteNonQueryAsync();
        }
    }
}
