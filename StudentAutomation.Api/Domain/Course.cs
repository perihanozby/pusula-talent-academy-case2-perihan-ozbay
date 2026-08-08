namespace StudentAutomation.Api.Domain;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = default!;
    public CourseStatus Status { get; set; } = CourseStatus.Planned;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
