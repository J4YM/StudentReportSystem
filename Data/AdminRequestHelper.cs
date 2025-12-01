using StudentReportInitial.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace StudentReportInitial.Data
{
    public static class AdminRequestHelper
    {
        public static async Task SubmitRequestAsync(int requesterUserId, int? branchId, string fullName, string email, string? reason)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                INSERT INTO AdminRequests (RequestedByUserId, BranchId, RequestedRole, FullName, Email, Reason)
                VALUES (@requestedByUserId, @branchId, @requestedRole, @fullName, @email, @reason)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@requestedByUserId", requesterUserId);
            command.Parameters.AddWithValue("@branchId", branchId.HasValue ? branchId.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@requestedRole", (int)UserRole.Admin);
            command.Parameters.AddWithValue("@fullName", fullName);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@reason", string.IsNullOrWhiteSpace(reason) ? DBNull.Value : reason);

            await command.ExecuteNonQueryAsync();
        }
    }
}

