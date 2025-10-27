namespace StudentReportInitial.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int GuardianId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
        
        // Account credentials
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        
        // Navigation properties
        public User? Guardian { get; set; }
        public List<Subject> EnrolledSubjects { get; set; } = new();
        public List<Attendance> Attendances { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
    }
}
