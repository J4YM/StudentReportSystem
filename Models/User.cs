namespace StudentReportInitial.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? BranchId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        
        public bool IsSuperAdmin => Role == UserRole.SuperAdmin;
    }

    public enum UserRole
    {
        SuperAdmin = 0,
        Admin = 1,
        Professor = 2,
        Guardian = 3,
        Student = 4
    }
}
