namespace StudentReportInitial.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ProfessorId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Quarter { get; set; } = string.Empty; // Prelim, Midterm, PreFinal, Final
        public string ComponentType { get; set; } = string.Empty; // QuizzesActivities, PerformanceTask, Exam
        public string AssignmentType { get; set; } = string.Empty; // Legacy field for backward compatibility
        public string AssignmentName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public string? Comments { get; set; }
        public DateTime DateRecorded { get; set; }
        public DateTime DueDate { get; set; }
        
        // Navigation properties
        public Student? Student { get; set; }
        public User? Professor { get; set; }
    }
}
