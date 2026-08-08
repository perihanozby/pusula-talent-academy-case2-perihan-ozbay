namespace StudentAutomation.Api.Domain;

public class Enrollment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public DateTime EnrolleAt { get; set; } = DateTime.UtcNow;
}
