namespace Namaya.Model
{
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // Present / Absent
        public string Branch { get; set; }
        public string Batch { get; set; }
    }

    // DTO for receiving bulk attendance from React
    public class AttendanceSubmissionDTO
    {
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public string Batch { get; set; }
        public List<StudentStatusDTO> Records { get; set; }
    }

    public class StudentStatusDTO
    {
        public int StudentId { get; set; }
        public string Status { get; set; }
    }
}