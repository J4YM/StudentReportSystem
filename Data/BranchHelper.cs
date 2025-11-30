using StudentReportInitial.Models;
using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    public static class BranchHelper
    {
        public static async Task<List<Branch>> GetAllBranchesAsync()
        {
            var branches = new List<Branch>();
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Code, Address, IsActive FROM Branches WHERE IsActive = 1 ORDER BY Name";
                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    branches.Add(new Branch
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Code = reader.GetString(2),
                        Address = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        IsActive = reader.GetBoolean(4)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading branches: {ex.Message}");
            }
            return branches;
        }

        public static async Task<Branch?> GetBranchByIdAsync(int branchId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Code, Address, IsActive FROM Branches WHERE Id = @id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", branchId);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Branch
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Code = reader.GetString(2),
                        Address = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        IsActive = reader.GetBoolean(4)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading branch: {ex.Message}");
            }
            return null;
        }

        public static async Task<int> GetUserBranchIdAsync(int userId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                // First check if BranchId column exists
                var checkColumnQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'BranchId'";
                using var checkColumnCommand = new SqlCommand(checkColumnQuery, connection);
                var columnExists = Convert.ToInt32(await checkColumnCommand.ExecuteScalarAsync()) > 0;

                if (!columnExists)
                {
                    // Column doesn't exist yet, migration hasn't run
                    return 0;
                }

                var query = "SELECT BranchId FROM Users WHERE Id = @userId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting user branch: {ex.Message}");
            }
            return 0;
        }

        public static async Task<bool> IsSuperAdminAsync(int userId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Role FROM Users WHERE Id = @userId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    return Convert.ToInt32(result) == (int)UserRole.SuperAdmin;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking super admin: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Gets the branch filter SQL clause for a user. Returns empty string for Super Admin, or "AND BranchId = @branchId" for regular admins.
        /// </summary>
        public static async Task<string> GetBranchFilterClauseAsync(int userId, string parameterName = "@branchId")
        {
            var isSuperAdmin = await IsSuperAdminAsync(userId);
            if (isSuperAdmin)
            {
                return ""; // Super Admin sees all branches
            }

            var branchId = await GetUserBranchIdAsync(userId);
            if (branchId > 0)
            {
                return $" AND BranchId = {parameterName}";
            }

            return ""; // Fallback if no branch assigned
        }

        /// <summary>
        /// Gets the branch filter parameter for a user. Returns null for Super Admin.
        /// </summary>
        public static async Task<int?> GetBranchFilterParameterAsync(int userId)
        {
            var isSuperAdmin = await IsSuperAdminAsync(userId);
            if (isSuperAdmin)
            {
                return null; // Super Admin sees all branches
            }

            var branchId = await GetUserBranchIdAsync(userId);
            return branchId > 0 ? branchId : null;
        }

        /// <summary>
        /// Adds branch filter parameter to a SQL command if needed.
        /// </summary>
        public static async Task AddBranchFilterParameterAsync(SqlCommand command, int userId, string parameterName = "@branchId")
        {
            var branchId = await GetBranchFilterParameterAsync(userId);
            if (branchId.HasValue)
            {
                command.Parameters.AddWithValue(parameterName, branchId.Value);
            }
        }
    }
}

