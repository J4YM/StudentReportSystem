using System.Data.SqlClient;
using System.Threading.Tasks;

namespace StudentReportInitial.Data
{
    internal static class SecurityAuditLogger
    {
        public static async Task LogSuperAdminChangeAsync(int userId, string action, string verificationMethod, string? details = null)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                INSERT INTO SecurityAuditLogs (UserId, Action, Details, VerificationMethod, LoggedDate)
                VALUES (@userId, @action, @details, @verificationMethod, GETDATE())";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@action", action);
            command.Parameters.AddWithValue("@details", string.IsNullOrWhiteSpace(details) ? (object)System.DBNull.Value : details);
            command.Parameters.AddWithValue("@verificationMethod", verificationMethod);

            await command.ExecuteNonQueryAsync();
        }
    }
}

