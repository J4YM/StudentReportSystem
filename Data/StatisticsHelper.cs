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
        public static async Task<SystemStatistics> GetSystemStatisticsAsync()
        {
            var stats = new SystemStatistics();

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                // Get total students (exclude deleted/inactive)
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE IsActive = 1", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalStudents = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total professors (users with Role = 2, exclude deleted/inactive)
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 2 AND IsActive = 1", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalProfessors = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total users (exclude deleted/inactive)
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE IsActive = 1", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalUsers = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total subjects
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Subjects", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalSubjects = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total grades
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Grades", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalGrades = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get total attendance records
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Attendance", connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalAttendanceRecords = result != null ? Convert.ToInt32(result) : 0;
                }

                // Get active guardians (users with Role = 3, exclude deleted/inactive)
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 3 AND IsActive = 1", connection))
                {
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

