using StudentReportInitial.Models;
using System.Data.SqlClient;

namespace StudentReportInitial.Data
{
    /// <summary>
    /// Helper class for generating email addresses based on branch domains
    /// </summary>
    public static class EmailDomainHelper
    {
        /// <summary>
        /// Maps branch codes to email domain suffixes
        /// </summary>
        private static readonly Dictionary<string, string> BranchDomainMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Metro Manila
            { "ORTIGAS", "ortigas" },
            { "ALABANG", "alabang" },
            { "CALOOCAN", "caloocan" },
            { "GLOBAL", "global" },
            { "LASPINAS", "laspinas" },
            { "MAKATI", "makati" },
            { "MARIKINA", "marikina" },
            { "MUNOZ", "munoz" },
            { "NOVALICHES", "novaliches" },
            { "PARANAQUE", "paranaque" },
            { "PASAY", "pasay" },
            { "PASIG", "pasig" },
            { "QUEZONAVE", "quezonave" },
            { "RECTO", "recto" },
            { "SHAW", "shaw" },
            { "TAFT", "taft" },
            { "VALENZUELA", "valenzuela" },
            // Luzon (North)
            { "BALIUAG", "baliuag" },
            { "CALAMBA", "calamba" },
            { "CUBAO", "cubao" },
            { "FAIRVIEW", "fairview" },
            { "ZAPOTE", "zapote" },
            { "MALOLOS", "malolos" },
            { "MEYCAUAYAN", "meycauayan" },
            { "SANFERNANDO", "sanfernando" },
            { "STAMARIA", "stamaria" },
            // Luzon (South)
            { "BACOOR", "bacoor" },
            { "BATANGAS", "batangas" },
            { "DASMARINAS", "dasmarinas" },
            { "LUCENA", "lucena" },
            { "SANPABLO", "sanpablo" },
            { "STAROSA", "starosa" },
            // Visayas
            { "BACOLOD", "bacolod" },
            { "CEBU", "cebu" },
            { "ILOILO", "iloilo" },
            { "TACLOBAN", "tacloban" },
            // Mindanao
            { "BUTUAN", "butuan" },
            { "CDO", "cdo" },
            { "DAVAO", "davao" },
            { "GENSAN", "gensan" },
            { "ILIGAN", "iligan" },
            { "KORONADAL", "koronadal" },
            { "MARBEL", "marbel" }
        };

        /// <summary>
        /// Gets the email domain for a branch by branch ID
        /// </summary>
        public static async Task<string> GetEmailDomainAsync(int branchId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Code FROM Branches WHERE Id = @branchId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@branchId", branchId);

                var result = await command.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    var branchCode = result.ToString() ?? "";
                    if (BranchDomainMap.TryGetValue(branchCode, out var domain))
                    {
                        return domain;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting email domain: {ex.Message}");
            }

            // Default fallback
            return "baliuag";
        }

        /// <summary>
        /// Gets the email domain for a branch by branch code
        /// </summary>
        public static string GetEmailDomainByCode(string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
            {
                return "baliuag"; // Default fallback
            }

            if (BranchDomainMap.TryGetValue(branchCode, out var domain))
            {
                return domain;
            }

            // Fallback: use lowercase branch code if not in map
            return branchCode.ToLower();
        }

        /// <summary>
        /// Generates a student email address based on student ID and branch
        /// Format: [studentID]@[branch-domain].sti.edu.ph
        /// </summary>
        public static async Task<string> GenerateStudentEmailAsync(string studentId, int branchId)
        {
            var domain = await GetEmailDomainAsync(branchId);
            var cleanStudentId = studentId.Replace("-", "").Replace(" ", "").ToLower();
            return $"{cleanStudentId}@{domain}.sti.edu.ph";
        }

        /// <summary>
        /// Generates a student email address using branch code
        /// </summary>
        public static string GenerateStudentEmailByCode(string studentId, string branchCode)
        {
            var domain = GetEmailDomainByCode(branchCode);
            var cleanStudentId = studentId.Replace("-", "").Replace(" ", "").ToLower();
            return $"{cleanStudentId}@{domain}.sti.edu.ph";
        }

        /// <summary>
        /// Generates a guardian email address based on student ID and branch
        /// Format: guardian.[studentID]@[branch-domain].sti.edu.ph
        /// </summary>
        public static async Task<string> GenerateGuardianEmailAsync(string studentId, int branchId)
        {
            var domain = await GetEmailDomainAsync(branchId);
            var cleanStudentId = studentId.Replace("-", "").Replace(" ", "").ToLower();
            return $"guardian.{cleanStudentId}@{domain}.sti.edu.ph";
        }

        /// <summary>
        /// Generates a guardian email address using branch code
        /// </summary>
        public static string GenerateGuardianEmailByCode(string studentId, string branchCode)
        {
            var domain = GetEmailDomainByCode(branchCode);
            var cleanStudentId = studentId.Replace("-", "").Replace(" ", "").ToLower();
            return $"guardian.{cleanStudentId}@{domain}.sti.edu.ph";
        }
    }
}

