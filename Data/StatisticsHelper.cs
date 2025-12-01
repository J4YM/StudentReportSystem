using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    public class SystemStatistics
    {
        public int TotalStudents { get; set; }
        public int TotalProfessors { get; set; }
        public int TotalUsers { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalGrades { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public int ActiveGuardians { get; set; }
    }

    public static class StatisticsHelper
    {
        public static async Task<SystemStatistics> GetSystemStatisticsAsync(int? userId = null, int? branchFilterId = null)
        {
            var stats = new SystemStatistics();

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                // Determine branch filter
                string branchFilter = "";
                int? branchId = null;
                if (userId.HasValue)
                {
                    var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(userId.Value);
                    if (!isSuperAdmin)
                    {
                        // Branch admin - only see their branch
                        branchId = await BranchHelper.GetUserBranchIdAsync(userId.Value);
                        if (branchId > 0)
                        {
                            branchFilter = " AND BranchId = @branchId";
                        }
                    }
                    else if (branchFilterId.HasValue && branchFilterId > 0)
                    {
                        // Super Admin with branch filter selected
                        branchId = branchFilterId.Value;
                        branchFilter = " AND BranchId = @branchId";
                    }
                    // If Super Admin and no branch filter, see all (branchFilter remains empty)
                }

                // Get total students (exclude deleted/inactive)
                var studentsQuery = "SELECT COUNT(*) FROM Students WHERE IsActive = 1" + branchFilter;
                using (var cmd = new SqlCommand(studentsQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalStudents = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total professors (users with Role = 2, exclude deleted/inactive)
                var professorsQuery = "SELECT COUNT(*) FROM Users WHERE Role = 2 AND IsActive = 1" + branchFilter;
                using (var cmd = new SqlCommand(professorsQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalProfessors = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total users (exclude deleted/inactive, exclude SuperAdmin for branch admins)
                var usersQuery = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
                if (branchId.HasValue && branchId > 0)
                {
                    usersQuery += " AND (BranchId = @branchId OR Role = 0)"; // Include SuperAdmin
                }
                using (var cmd = new SqlCommand(usersQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalUsers = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total subjects
                var subjectsQuery = "SELECT COUNT(*) FROM Subjects WHERE IsActive = 1" + branchFilter;
                using (var cmd = new SqlCommand(subjectsQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalSubjects = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total grades
                var gradesQuery = "SELECT COUNT(*) FROM Grades" + (branchFilter != "" ? branchFilter : "");
                using (var cmd = new SqlCommand(gradesQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalGrades = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total attendance records
                var attendanceQuery = "SELECT COUNT(*) FROM Attendance" + (branchFilter != "" ? branchFilter : "");
                using (var cmd = new SqlCommand(attendanceQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalAttendanceRecords = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get active guardians (users with Role = 3, exclude deleted/inactive)
                var guardiansQuery = "SELECT COUNT(*) FROM Users WHERE Role = 3 AND IsActive = 1" + branchFilter;
                using (var cmd = new SqlCommand(guardiansQuery, connection))
                {
                    if (branchId.HasValue && branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@branchId", branchId.Value);
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    stats.ActiveGuardians = result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }

            return stats;
        }
    }
}

