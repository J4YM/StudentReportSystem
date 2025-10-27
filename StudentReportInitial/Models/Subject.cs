namespace StudentReportInitial.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ProfessorId { get; set; }  // Made nullable to support unclaimed subjects
        public string GradeLevel { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        // Navigation properties
        public User? Professor { get; set; }
        public List<Student> EnrolledStudents { get; set; } = new();
    }
}
