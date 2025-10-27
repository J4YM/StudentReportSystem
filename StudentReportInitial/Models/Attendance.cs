namespace StudentReportInitial.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ProfessorId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime RecordedDate { get; set; }
        
        // Navigation properties
        public Student? Student { get; set; }
        public User? Professor { get; set; }
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4
    }
}
