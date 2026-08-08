namespace StudentAutomation.Api.Domain;

public class Student
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string Number { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string Department { get; set; } = default!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Comment> CommentsReceived { get; set; } = new List<Comment>();
}
